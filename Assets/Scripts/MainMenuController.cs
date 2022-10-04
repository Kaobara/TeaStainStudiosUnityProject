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
    [SerializeField] string LevelSelectMenu;
    [SerializeField] Slider volumeSlider;
    private float curSessionVolume;

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
    }

    public void LoadLevelSelectMenu() {
        SceneManager.LoadScene(LevelSelectMenu);
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
}
