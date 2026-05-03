using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class HunterAgent : Agent
{
    [Header("References")]
    public Transform runner;
    public Transform[] obstacles;

    [Header("Tuning")]
    public float moveForce = 8f;
    public int maxSteps = 2000;

    private Rigidbody rb;
    private Vector3 startPos;
    private int stepCount;
    private Vector3 prevPosition;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        prevPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        // Reset position
        transform.position = startPos;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        stepCount = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Own position and velocity (6)
        sensor.AddObservation(transform.position);
        sensor.AddObservation(rb.linearVelocity);

        // Runner relative position (3)
        sensor.AddObservation(runner.position - transform.position);

        // Distance to runner (1)
        sensor.AddObservation(Vector3.Distance(transform.position, runner.position));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        stepCount++;

        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 force = new Vector3(moveX, 0f, moveZ) * moveForce;
        rb.AddForce(force, ForceMode.Force);

        float dist = Vector3.Distance(transform.position, runner.position);
        float prevDist = Vector3.Distance(prevPosition, runner.position);
        AddReward(0.005f * (prevDist - dist));

        // Timeout
        if (stepCount >= maxSteps)
        {
            AddReward(-0.5f);
            EndEpisode();
        }

        prevPosition = transform.position;
    }

    // Called by the arena manager when hunter touches runner
    public void OnCaughtRunner()
    {
        AddReward(1.0f);
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;
        c[0] = Input.GetKey(KeyCode.D) ? 1f :
               Input.GetKey(KeyCode.A) ? -1f : 0f;
        c[1] = Input.GetKey(KeyCode.W) ? 1f :
               Input.GetKey(KeyCode.S) ? -1f : 0f;
    }
}