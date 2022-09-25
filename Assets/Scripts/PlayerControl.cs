using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{   
    
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
    
    }

    void LateUpdate() {

        float horizontal_movement_x = Input.GetAxis("Mouse X");
        
        // Doing a rotation around the y-axis makes the camera rotate the view horizontally. Vector3.up is a 
        // shorthand way of writing (0, 1, 0). The code is rotating the y-axis using the horizontal
        // movement of the player's mouse.
        transform.eulerAngles += Vector3.up * horizontal_movement_x; 
    }

}
