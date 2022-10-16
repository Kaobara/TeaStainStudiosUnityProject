using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGoals : MonoBehaviour
{
    private TutorialController tc;

    // Start is called before the first frame update
    void Start()
    {
        tc = GameObject.Find("TutorialController").GetComponent<TutorialController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other) {
        GoalZone goalZone = other.gameObject.GetComponent<GoalZone>();
        if (goalZone != null && !goalZone.levelComplete) {
            tc.CompleteGoal(goalZone);
        }
    }
}
