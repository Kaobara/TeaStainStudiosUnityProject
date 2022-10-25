using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGoals : MonoBehaviour
{
    private GoalController gc;

    private void Start()
    {
        gc = GameObject.Find("GoalController").GetComponent<GoalController>();
    }

    public void OnTriggerEnter(Collider other) {
        Goal goal = other.gameObject.GetComponent<Goal>();
        if (goal != null) {
            gc.HandleGoal(goal);
        }
    }

    public void GoalRetrival(Goal goal)
    {
        gc.HandleGoal(goal);
    }
}
