using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalZone : MonoBehaviour
{
    [SerializeField] public string msg;
    [SerializeField] public bool levelComplete;
    [SerializeField] GameObject promptToShow;
    [SerializeField] GameObject levelController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPrompt()
    {
        promptToShow.SetActive(true);
        levelController.GetComponent<LevelsController>().PauseGame();
    }
}
