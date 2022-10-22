using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// This script and the Level Select Menu Scene was created by studying, referencing and adapting the resources:
// MAIN MENU in Unity | All-In-One Tutorial: https://www.youtube.com/watch?v=Cq_Nnw_LwnI
// Level selection in your Unity game | Unity tutorial: https://www.youtube.com/watch?v=YAHFnF2MRsE
// Unity SceneManager Documentation: https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html
// Various ways of disabling/removing buttons: https://www.w3schools.blog/how-to-disable-buttons-in-unity
// Unlock Levels - Levels Menu - Unity (PlayerPrefs) - Tutorial: https://www.youtube.com/watch?v=HhAEwhKnJJA (referenced the idea of a Button array)
// Unity GameObject.GetComponent Scripting API documentation: https://docs.unity3d.com/ScriptReference/GameObject.GetComponent.html
public class LevelSelectMenuController : MonoBehaviour
{
    [Header("Level Buttons and Medals")]
    [SerializeField] GameObject[] levelContainers;
    int curLevelsUnlocked;
    [SerializeField] Sprite goldMedal;
    [SerializeField] Sprite silverMedal;
    [SerializeField] Sprite bronzeMedal;

    private const int DEFAULT_LEVELS_UNLOCKED = 0;
    private const int ZERO_OFFSET = 1;

    [Header("Level Select Menu UI Sounds")]
    [SerializeField] AudioSource levelSelectMenuUIAudio;
    [SerializeField] AudioClip clickButtonSound;

    void Start() {
        // If a player has some progress on unlocked levels, fetch the "save data"
        // otherwise create new "save data" in the player preferences through the
        // constant default which is just 1.
        if(PlayerPrefs.HasKey("Unlocked Levels")) {
            curLevelsUnlocked = PlayerPrefs.GetInt("Unlocked Levels");
        } else {
            PlayerPrefs.SetInt("Unlocked Levels", DEFAULT_LEVELS_UNLOCKED);
            curLevelsUnlocked = PlayerPrefs.GetInt("Unlocked Levels");
        }

        // For debugging. Resets player level progression.
        // PlayerPrefs.SetInt("Unlocked Levels", DEFAULT_LEVELS_UNLOCKED);
        // curLevelsUnlocked = PlayerPrefs.GetInt("Unlocked Levels");

        // Depending on the levels that the player has unlocked based on the "save data"
        // in the PlayerPrefs File, set the buttons to be interactable. Then, also display the medals
        // based on their best clear time. The method of finding child game objects was referenced from
        // https://stackoverflow.com/questions/40752083/how-to-find-child-of-a-gameobject-or-the-script-attached-to-child-gameobject-via#:~:text=Finding%20child%20GameObject%20by%20index%3A&text=transform.,3%2C%20to%20the%20GetChild%20function.
        // and communication/teaching from Workshops.
        for(int i = 0; i < curLevelsUnlocked; i++) {
            levelContainers[i].transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
            
            if(PlayerPrefs.HasKey("Level " + i)) {
                GameObject levelMedal = levelContainers[i].transform.GetChild(1).gameObject;
                levelMedal.GetComponent<Image>().sprite = goldMedal;
                levelMedal.SetActive(true);
            }
        }

    }

    // Navigates to a certain level. Called by the level buttons on button press.
    public void GoToLevel(string levelToNavigateTo) {
        SceneManager.LoadScene(levelToNavigateTo);
    }

    public void GoToMainMenu() {
        SceneManager.LoadScene("StartScene");
    }

    public void PlayClickButtonSound() {
        levelSelectMenuUIAudio.PlayOneShot(clickButtonSound);
    }
}
