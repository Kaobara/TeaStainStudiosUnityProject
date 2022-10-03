using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


// This script and the Main Menu Scene was created by studying, referencing and adapting the resources:
// MAIN MENU in Unity | All-In-One Tutorial: https://www.youtube.com/watch?v=Cq_Nnw_LwnI
// Level selection in your Unity game | Unity tutorial: https://www.youtube.com/watch?v=YAHFnF2MRsE
// Unity SceneManager Documentation: https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html
public class MainMenuController : MonoBehaviour
{   
    [SerializeField] string LevelSelectMenu;
    [SerializeField] string OptionsMenu;

    public void LoadLevelSelectMenu() {
        SceneManager.LoadScene(LevelSelectMenu);
    }

    public void LoadOptionsMenu() {
        SceneManager.LoadScene(OptionsMenu);
    }
}
