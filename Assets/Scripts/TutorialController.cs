using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    [Header("Goals")]
    [SerializeField] private GoalZone[] goals;

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

    public void CompleteGoal(GoalZone goalZone)
    {
        if (n < goals.Length && goalZone.Equals(goals[n])) {
            goalZone.showPrompt();
            n += 1;
        }
    }


}
