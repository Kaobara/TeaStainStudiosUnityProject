using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GoalController : MonoBehaviour
{
    [Header("Goals")]
    [SerializeField] private Goal[] goals;

    private int n;

    // Start is called before the first frame update
    void Start()
    {
        n = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleGoal(Goal goal)
    {
        if (n < goals.Length && goal.Equals(goals[n])) {
            goal.CompleteGoal();
            n += 1;
        }
    }

    public Vector3 GetGoalPos()
    {
        return goals[n].transform.position;
    }

}
