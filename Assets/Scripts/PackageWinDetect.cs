using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageWinDetect : MonoBehaviour
{
    private LevelsController lc;

    // Start is called before the first frame update
    void Start()
    {
        lc = GameObject.Find("LevelController").GetComponent<LevelsController>();
    }

    public void OnTriggerEnter(Collider other)
    {
        GoalZone goalZone = other.gameObject.GetComponent<GoalZone>();
        if (goalZone != null && goalZone.levelComplete) {
            lc.CompleteLevel();
        }
    }
}
