using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] GameObject promptToShow;

    public void CompleteGoal()
    {
        Debug.Log("Goal Complete");
        if (promptToShow != null) {
            promptToShow.SetActive(true);
            GameObject.Find("LevelController").GetComponent<LevelsController>().PauseGame();
        }
    }
}

