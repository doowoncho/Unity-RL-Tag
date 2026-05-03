using System;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    public HunterAgent hunter;
    public RunnerAgent runner;
    public float catchDistance = 1.0f;

    public int maxSteps;

    private void Start()
    {
        hunter.maxSteps = maxSteps;
        runner.maxSteps = maxSteps;
    }

    void FixedUpdate()
    {
        float dist = Vector3.Distance(
            hunter.transform.position,
            runner.transform.position);

        //Checks if hunter has caught runner
        if (dist <= catchDistance)
        {
            Debug.Log("runner was caught!");
            hunter.OnCaughtRunner();
            runner.OnCaught();
        }
    }
}