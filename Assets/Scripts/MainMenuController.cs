using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


// This script and the Main Menu Scene was created by studying, referencing and adapting the resources:
// MAIN MENU in Unity | All-In-One Tutorial: https://www.youtube.com/watch?v=Cq_Nnw_LwnI
// Level selection in your Unity game | Unity tutorial: https://www.youtube.com/watch?v=YAHFnF2MRsE
// Unity SceneManager Documentation: https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html
// Unity Slider Documentation: https://docs.unity3d.com/ScriptReference/UIElements.Slider.html (only value was used here)
// Unity PlayerPrefs Documentation: https://docs.unity3d.com/2020.1/Documentation/ScriptReference/PlayerPrefs.html
public class MainMenuController : MonoBehaviour
{   
    [Header("Audio Options")]
    [SerializeField] Slider volumeSlider;
    private float curSessionVolume;

    [Header("Sensitivity Options")]
    [SerializeField] Slider sensitivitySlider;
    private float curSessionSensitivity;

    [Header("UI Audio")]
    [SerializeField] AudioSource mainMenuUIAudio;
    [SerializeField] AudioClip clickButtonSound;

    [Header("Exploration Seed Input")]
    [SerializeField] GameObject seedInputFieldText;
    [SerializeField] GameObject seedInputErrorText;

    void Awake() {
        // If there is a volume setting found in the player preferences file, then use that value
        if(PlayerPrefs.HasKey("Volume")) {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume");
            AudioListener.volume = volumeSlider.value;
        // Otherwise use default value of 0.5
        } else {
            volumeSlider.value = 0.5f;
        }

        curSessionVolume = volumeSlider.value;

        if(PlayerPrefs.HasKey("Sensitivity")) {
            sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity");
        } else {
            sensitivitySlider.value = 0.5f;
            PlayerPrefs.SetFloat("Sensitivity", 0.5f);
        }

        curSessionSensitivity = sensitivitySlider.value;
    }

    public void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    // Set temporary volume as player moves slider around
    public void SetVolumeTemp() {
        AudioListener.volume = volumeSlider.value;
    }

    // If player exists the options menu without applying, revert volume change
    // to the value before they entered the options menu
    public void RevertVolumeChange() {
        volumeSlider.value = curSessionVolume;
        AudioListener.volume = curSessionVolume;
    }

    // Apply volume changes by saving to player preferences file
    public void ApplyVolumeChange() {
        PlayerPrefs.SetFloat("Volume", AudioListener.volume);
        curSessionVolume = volumeSlider.value;
    }

    public void RevertSensitivityChange() {
        sensitivitySlider.value = curSessionSensitivity;
    }

    // In-game sensitivity is only for in-game so no need to temporarily
    // store the value as user configures
    public void ApplySensitivityChange() {
        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
        curSessionSensitivity = sensitivitySlider.value;
    }

    public void PlayClickButtonSound() {
        mainMenuUIAudio.PlayOneShot(clickButtonSound);
    }

    public void ExitGame() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Parses the seed and determine if it's a valid int. If it is, then save the
    // seed to PlayerPrefs so it can be accessed in the "Exploration" scene and 
    // load the scene up. Otherwise, display input error text.
    public void SaveSeed() {
        string seedText = seedInputFieldText.GetComponent<TMPro.TextMeshProUGUI>().text;
        // Remove null character at the end of the string to ensure TryParse does not fail for integers.
        // If there is no number input, then there length of string is 0, and so there is no substring.
        // This logic works fine with the TryParse logic.
        seedText = seedText.Substring(0, seedText.Length - 1);
        
        int seed;

        if(int.TryParse(seedText, out seed) && (seed >= 0)) {
            PlayerPrefs.SetInt("Exploration Seed", seed);
            LoadScene("Exploration");
        } else {
            seedInputErrorText.SetActive(true);
        }
    }

}
