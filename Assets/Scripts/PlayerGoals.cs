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
        // handle goalzones that arent the level complete zone
        GoalZone goal = other.gameObject.GetComponent<GoalZone>();
        if (goal != null && !goal.levelComplete) {
            gc.HandleGoal(goal);
        }
    }

    // handle goal objects
    public void GoalRetrival(Goal goal)
    {
        gc.HandleGoal(goal);
    }
}
