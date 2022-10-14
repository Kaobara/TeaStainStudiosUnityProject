using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    [SerializeField] GameObject exitGamePrompt;

    // If esc/escape key is pressed, then render
    // the exit game prompt and unrender the main menu
    void Update()
    {
        if (Input.GetKey("escape")) {
            exitGamePrompt.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}
