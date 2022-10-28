using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelsController : MonoBehaviour
{   
    [SerializeField] GameObject player;
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject zoomOutCamera;

    [Header("Goal Distance")]
    [SerializeField] GameObject goalDistanceTMP;
    [SerializeField] GameObject goalController;
    private float distance;

    [Header("Exit Level")]
    [SerializeField] GameObject exitLevelPrompt;

    [Header("Level Completion")]
    [SerializeField] GameObject currentTimeTMP;
    [SerializeField] GameObject bestTimeTMP;
    [SerializeField] GameObject levelCompletePrompt;
    [SerializeField] int levelNum;
    [SerializeField] bool ExploreMode;

    [Header("Game Options")]
    public float sensitivity;
    // Used for temporarily storing the sensitivity for when player pauses the game, so the game
    // can revert back to the original sensitivity upon player unpausing.
    private float tempSensitivity;

    [Header("Camera Switching")]
    [SerializeField] float cameraSwitchTimeThreshold = 1.0f;
    private float timeLastSwitchCamera;

    [Header("Medal Time Thresholds")]
    [SerializeField] float goldTimeThreshold;
    [SerializeField] float silverTimeThreshold;
    [SerializeField] GameObject silverTimeThresholdTMP;
    [SerializeField] GameObject goldTimeThresholdTMP;


    private bool gamePaused = false;
    private int curLevelsUnlocked;
    private const int tutorialLevelNum = 0;
    private float levelCompleteTime;

    // Time elapsed since the player last paused or unpaused the game so the player/Unity doesn't
    // spam pause/unpause.
    private float lastGamePauseSwitchTime = 0.0f;

    // Normalise the sensitivity value to between 0.1 to 1.0, so the player can't have such low sensitivity that
    // they can't move the camera up and down at all.
    void Awake() {
        sensitivity = ((PlayerPrefs.GetFloat("Sensitivity") * 0.9f) + 0.1f);

        if(levelNum == 0) {
            PauseGame();
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1.0f;
        }

    }

    // If esc/escape key is pressed, then render the exit level prompt. 
    // Guard used to make sure unity doesn't "pause again" while game is paused.
    void Update() {
        if (Input.GetKey("escape")) {
            if(gamePaused == false && (Time.time - lastGamePauseSwitchTime) > 0.5f) {
                lastGamePauseSwitchTime = Time.time;
                exitLevelPrompt.SetActive(true);
                PauseGame();
            }

            if(gamePaused == true && (Time.time - lastGamePauseSwitchTime) > 0.5f) {
                lastGamePauseSwitchTime = Time.time;
                exitLevelPrompt.SetActive(false);
                UnpauseGame();
            }
        }

        if (Input.GetKey("c") && (Time.time - timeLastSwitchCamera) > cameraSwitchTimeThreshold) {
            timeLastSwitchCamera = Time.time;
            playerCamera.SetActive(!playerCamera.activeInHierarchy);
            zoomOutCamera.SetActive(!zoomOutCamera.activeInHierarchy);
        }

        if(!ExploreMode){
            updateGoalDistance();
        }
    }

    private void updateGoalDistance() {
        // insert distance formula here.
        distance = Vector3.Distance(player.GetComponent<PlayerControl>().GetPlayerPos(), goalController.GetComponent<GoalController>().GetGoalPos());

        goalDistanceTMP.GetComponent<TMPro.TextMeshProUGUI>().text = distance.ToString("n2");
    }


    // Pause the game by bringing up exit level prompt and changing the sensitivity to be 0
    // so the player can't use mouse movements to rotate the player and camera.
    public void PauseGame() {

        Time.timeScale = 0f;
        
        gamePaused = true;

        Cursor.lockState = CursorLockMode.None;

        tempSensitivity = sensitivity;
        sensitivity = 0.0f;

        player.GetComponent<PlayerControl>().updateSensitivity(sensitivity);
        playerCamera.GetComponent<CameraControl>().updateSensitivity(sensitivity);
        zoomOutCamera.GetComponent<CameraControl>().updateSensitivity(sensitivity);
    }

    // Unpause by reverting the sensitivity to the temporary attribute stored on pause. 
    // Cursor is also locked again. The calling prompt will outside set themselves
    // to be inactive outside of this script.
    public void UnpauseGame() {

        Time.timeScale = 1.0f;

        sensitivity = tempSensitivity;

        player.GetComponent<PlayerControl>().updateSensitivity(sensitivity);
        playerCamera.GetComponent<CameraControl>().updateSensitivity(sensitivity);
        zoomOutCamera.GetComponent<CameraControl>().updateSensitivity(sensitivity);

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
        levelCompleteTime = Time.timeSinceLevelLoad;
        currentTimeTMP.GetComponent<TMPro.TextMeshProUGUI>().text = "Current: " + levelCompleteTime.ToString("n2");

        silverTimeThresholdTMP.GetComponent<TMPro.TextMeshProUGUI>().text = silverTimeThreshold.ToString("n2");
        goldTimeThresholdTMP.GetComponent<TMPro.TextMeshProUGUI>().text = goldTimeThreshold.ToString("n2");

        PauseGame();

        // Check if player has an existing completion time for this level.
        if(PlayerPrefs.HasKey("Level " + levelNum) && PlayerPrefs.HasKey("Level " + levelNum + " Time")) {
            // Check if this is the player's best completion time. If it is then save
            // the new completion time and then adjust the medal to be displayed
            // on the level select menu.
            if(levelCompleteTime < PlayerPrefs.GetFloat("Level " + levelNum + " Time")) {
                saveAndDetermineMedal(levelCompleteTime);
            }
        // Otherwise, this is the player's first clear, so save the time and determine the medal.
        } else {
            saveAndDetermineMedal(levelCompleteTime);
        }

        bestTimeTMP.GetComponent<TMPro.TextMeshProUGUI>().text = "Best: " + PlayerPrefs.GetFloat("Level " + levelNum + " Time").ToString("n2");
        
        levelCompletePrompt.SetActive(true);
        
        curLevelsUnlocked = PlayerPrefs.GetInt("Unlocked Levels");

        // Increment number of levels the player has unlocked so far by 1
        // then save it into the PlayerPrefs file.
        if(levelNum == curLevelsUnlocked) {
            curLevelsUnlocked++;
            PlayerPrefs.SetInt("Unlocked Levels", curLevelsUnlocked);
        }
    }

    private void saveAndDetermineMedal(float levelCompleteTime) {
        PlayerPrefs.SetFloat("Level " + levelNum + " Time", levelCompleteTime);

        if(levelCompleteTime < goldTimeThreshold) {
            PlayerPrefs.SetString("Level " + levelNum, "Gold");
        } else if (levelCompleteTime < silverTimeThreshold) {
            PlayerPrefs.SetString("Level " + levelNum, "Silver");
        } else {
            PlayerPrefs.SetString("Level " + levelNum, "Bronze");
        }
    }


}
