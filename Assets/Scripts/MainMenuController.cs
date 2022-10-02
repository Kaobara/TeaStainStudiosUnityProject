using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{   
    [SerializeField] string LevelSelectMenu;
    [SerializeField] string OptionsMenu;

    public void LoadLevelSelectMenu(){
        SceneManager.LoadScene(LevelSelectMenu);
    }

    public void LoadOptionsMenu(){
        SceneManager.LoadScene(OptionsMenu);
    }
}
