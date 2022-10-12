using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneAudioController : MonoBehaviour
{
    void Awake() {
        // If there is a volume setting found in the player preferences file, then use that value
        if(PlayerPrefs.HasKey("Volume")) {
            AudioListener.volume = PlayerPrefs.GetFloat("Volume");
        // Otherwise use default value of 0.5
        } else {
            AudioListener.volume = 0.5f;
        }
    }
}
