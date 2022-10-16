using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelsController : MonoBehaviour
{   
    [SerializeField] GameObject player;
    [SerializeField] GameObject camera;

    [Header("Goal Distance")]
    [SerializeField] GameObject goalDistanceTMP;
    private float distance;

    [Header("Exit Level")]
    [SerializeField] GameObject exitLevelPrompt;

    [Header("Level Completion")]
    [SerializeField] GameObject timeTMP;

    [Header("Game Options")]
    public float sensitivity;
    // Used for temporarily storing the sensitivity for when player pauses the game, so the game
    // can revert back to the original sensitivity upon player unpausing.
    private float tempSensitivity;

    private bool gamePaused = false;

    // Normalise the sensitivity value to between 0.1 to 1.0, so the player can't have such low sensitivity that
    // they can't move the camera up and down at all.
    void Awake() {
        sensitivity = ((PlayerPrefs.GetFloat("Sensitivity") * 0.9f) + 0.1f);
    }

    // If esc/escape key is pressed, then render the exit level prompt. 
    // Guard used to make sure unity doesn't "pause again" while game is paused.
    void Update() {
        if (Input.GetKey("escape") && gamePaused == false) {
            PauseGame();
        }

        updateGoalDistance();
    }

    private void updateGoalDistance() {
        // insert distance formula here.
        distance = 0.0f;

        goalDistanceTMP.GetComponent<TMPro.TextMeshProUGUI>().text = distance.ToString();
    }


    // Pause the game by bringing up exit level prompt and changing the sensitivity to be 0
    // so the player can't use mouse movements to rotate the player and camera.
    private void PauseGame() {
        gamePaused = true;
        
        exitLevelPrompt.SetActive(true);
        Cursor.lockState = CursorLockMode.None;

        tempSensitivity = sensitivity;
        sensitivity = 0.0f;

        player.GetComponent<PlayerControl>().updateSensitivity(sensitivity);
        camera.GetComponent<CameraControl>().updateSensitivity(sensitivity);
    }

    // Unpause by reverting the sensitivity to the temporary attribute stored on pause. 
    // Cursor is also locked again. The calling prompt will outside set themselves
    // to be inactive outside of this script.
    public void UnpauseGame() {
        sensitivity = tempSensitivity;

        player.GetComponent<PlayerControl>().updateSensitivity(sensitivity);
        camera.GetComponent<CameraControl>().updateSensitivity(sensitivity);

        Cursor.lockState = CursorLockMode.Locked;
        
        gamePaused = false;
    }

    public void ExitLevel() {
        SceneManager.LoadScene("LevelSelectMenu");
    }

    public void RestartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // When completing level, set the time text to the time used to clear the level
    // so that it can be displayed to the player.
    public void CompleteLevel() {
        timeTMP.GetComponent<TMPro.TextMeshProUGUI>().text = Time.time.ToString();
    }


}
