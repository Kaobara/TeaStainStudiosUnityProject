using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{   
    [Header("Lateral Movemnt")]
    public float maxSpeed;
    public Transform orientation;

    [Header("Jumping")]
    public float jumpCooldown;
    public float jumpForce;
    public KeyCode jumpKey;
    private bool canJump;

    [Header("Checks for Groundedness")]
    public float heightEpsilon;
    public float playerHeight;    
    public LayerMask groundMask;
    private bool grounded;

    // storing inputs
    private float sidewaysInput;
    private float forwardsInput;
    private bool jumpInput;

    private Vector3 playerDir;
    private Rigidbody rigidBody;
    private float accel = 10f;

    [Header("Acceleration Constants")]
    [SerializeField] private float groundAccel = 5f;
    [SerializeField] private float airAccel = 1f;
    
    [Header("Drag Constants")]
    [SerializeField] private float groundDrag = 1f;
    [SerializeField] private float airDrag = 0.2f;
    
    private void ProcessInput()
    {
        forwardsInput = Input.GetAxisRaw("Vertical");
        sidewaysInput = Input.GetAxisRaw("Horizontal");
        jumpInput = Input.GetKey(jumpKey);
    }

    private void MovePlayer()
    {
        playerDir = orientation.forward * forwardsInput + orientation.right * sidewaysInput;
        rigidBody.AddForce(playerDir.normalized * maxSpeed * accel);
    }

    private void SpeedLimiter()
    {
        if (rigidBody.velocity.magnitude > maxSpeed) {
            rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
        }
    }

    private bool GroundCheck()
    {
        // shoot downward ray to check if on ground
        return Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + heightEpsilon, groundMask);
    }

    private void Jump()
    {
        // immediately disallow jumping
        canJump = false;

        // set y velocity to 0 to make every jump the same height
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);
        
        // add upward force
        rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void MakeReadyToJump()
    {
        canJump = true;
    }

    private void SetDrag()
    {
        if (grounded) {
            rigidBody.drag = groundDrag;
        } else {
            rigidBody.drag = airDrag;
        }
    }

    private void SetAccel()
    {
        if (grounded) {
            accel = groundAccel;
        } else {
            accel = airAccel;
        } 
    }
    
    private void Start()
    {
        // store rigidbody and disallow rotation, allow jumping from start
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
        canJump = true;
    }

    private void Update()
    {
        // store if player is on the ground for current frame
        grounded = GroundCheck();

        // process movement input keys
        ProcessInput();

        // jump if jumpkey is pressed, jump cooldown not active, and grounded
        if (jumpInput && canJump && grounded) Jump();

        // disallow jumping for set time
        Invoke(nameof(MakeReadyToJump), jumpCooldown);

        // set drag and acceleration values according to air/ground state
        SetDrag();
        SetAccel();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void LateUpdate()
    {
        SpeedLimiter();
    }

}
