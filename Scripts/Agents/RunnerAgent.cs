using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static Unity.Collections.Unicode;

public class RunnerAgent : Agent
{
    [Header("References")]
    public Transform hunter;
    public Transform[] obstacles;

    [Header("Tuning")]
    public float moveForce = 10f;
    public float teleportDistance = 4f;
    public float teleportCooldown = 5f;
    public int maxSteps = 2000;

    private Rigidbody rb;
    private Vector3 startPos;
    private float teleportTimer;
    public int stepCount;
    private Vector3 prevPosition;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        prevPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPos;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        teleportTimer = 0f;
        stepCount = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Own position and velocity (6)
        sensor.AddObservation(transform.position);
        sensor.AddObservation(rb.linearVelocity);

        // Hunter relative position and distance (4)
        sensor.AddObservation(hunter.position - transform.position);
        sensor.AddObservation(Vector3.Distance(transform.position, hunter.position));

        // Teleport cooldown remaining � normalized 0..1 (1)
        sensor.AddObservation(teleportTimer / teleportCooldown);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        stepCount++;
        teleportTimer -= Time.fixedDeltaTime;

        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float teleportX = actions.ContinuousActions[2];
        float teleportZ = actions.ContinuousActions[3];
        bool wantsTeleport = actions.ContinuousActions[4] > 0.5f;

        Vector3 force = new Vector3(moveX, 0f, moveZ) * moveForce;
        rb.AddForce(force, ForceMode.Force);

        //// Teleport
        if (wantsTeleport && teleportTimer <= 0f)
        {
            Vector3 direction = new Vector3(teleportX, 0f, teleportZ).normalized;
            if (direction == Vector3.zero) direction = transform.forward;

            // Only hit walls, ignore obstacle layer
            int layerMask = ~LayerMask.GetMask("Obstacle");

            RaycastHit hit;
            Vector3 destination;

            if (Physics.Raycast(transform.position, direction, out hit, teleportDistance, layerMask))
            {
                // Wall is in the way — land just before it
                destination = transform.position + direction * (hit.distance - 0.5f);
            }
            else
            {
                // Nothing blocking — full teleport
                destination = transform.position + direction * teleportDistance;
            }

            transform.position = destination;
            rb.linearVelocity = Vector3.zero;
            teleportTimer = teleportCooldown;
            AddReward(0.05f);
        }


        // Reward for moving away from hunter
        float dist = Vector3.Distance(transform.position, hunter.position);
        float prevDist = Vector3.Distance(prevPosition, hunter.position);
        AddReward(0.005f * (dist - prevDist));


        AddReward(0.001f);  // Small reward just for being alive

        // Timeout � runner survived the whole episode
        if (stepCount >= maxSteps)
        {
            AddReward(1f);
            EndEpisode();
        }

        prevPosition = transform.position;
    }

    // Called by arena manager when hunter catches runner
    public void OnCaught()
    {
        AddReward(-1.0f);
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;
        c[0] = Input.GetKey(KeyCode.RightArrow) ? 1f :
               Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f;
        c[1] = Input.GetKey(KeyCode.UpArrow) ? 1f :
               Input.GetKey(KeyCode.DownArrow) ? -1f : 0f;
        c[4] = Input.GetKey(KeyCode.K) ? 1f : 0f;
    }
}