using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{   [Header("Camera Control Thresholds")]
    [SerializeField] private float max_up_rotation_angle;
    [SerializeField] private float max_down_rotation_angle;
    
    [Header("Levels Controller")]
    [SerializeField] GameObject levelController;
    private float sensitivity;

    private float vertical_movement_y;
    private float vertical_movement_before;

    // Locks the cursor to the game screen so that Unity
    // can use the Player's mouse movements for
    // camera rotation
    void Start() {
        sensitivity = levelController.GetComponent<LevelsController>().sensitivity;
    }

    // Perform camera rotation on LateUpdate so player's position is updated first before applying changes to the camera
    // This section of camera control was referenced, studied and adapted for use from the following resources:
    // https://answers.unity.com/questions/1179680/how-to-rotate-my-camera.html
    // https://gamedevacademy.org/unity-audio-tutorial/
    // https://docs.unity3d.com/ScriptReference/Vector3.html
    // https://docs.unity3d.com/ScriptReference/Transform-eulerAngles.html
    void LateUpdate() {
        
        // Calculates the accumulated vertical movement through the mouse from the base of 0 (when level loads).
        vertical_movement_before = vertical_movement_y;
        vertical_movement_y += Input.GetAxis("Mouse Y") * sensitivity;
        
        // Using the vertical movement of the mouse to rotate about the x-axis (vertical rotation)
        // the vertical movement value is negated due to Unity's LHS coordinate system having counterintuitive
        // rotations. Essentially, negating it makes it so that moving the mouse upwards nets an upwards 
        // rotation and vice versa. The maximum rotation angles are determined by 2 parameters which are 
        // serialized and can be set in Unity.
        float post_rotation_x = transform.localRotation.x + -vertical_movement_y;

        // Apply rotation if the rotation is not past the maximum upwards or downwards rotation angles.
        if((post_rotation_x >= (-max_up_rotation_angle)) && (post_rotation_x <= max_down_rotation_angle)) {
            transform.localRotation = Quaternion.Euler(Vector3.right * -vertical_movement_y);
        } else {
            vertical_movement_y = vertical_movement_before;
        }
    
    }

    public void updateSensitivity(float sensitivity) {
        this.sensitivity = sensitivity;
    }
}
