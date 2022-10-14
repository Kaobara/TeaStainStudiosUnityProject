using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelsController : MonoBehaviour
{   
    [SerializeField] GameObject exitLevelPrompt;
    public float sensitivity;

    void Awake() {
        sensitivity = PlayerPrefs.GetFloat("Sensitivity");
    }

    // If esc/escape key is pressed, then render
    // the exit level prompt
    void Update() {
        if (Input.GetKey("escape")) {
            exitLevelPrompt.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ExitLevel() {
        SceneManager.LoadScene("LevelSelectMenu");
    }
}
