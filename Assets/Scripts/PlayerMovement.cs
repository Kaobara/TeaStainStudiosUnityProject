using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// enumeration of different player states that affect accel, drag and movement
// abilities
enum playerState
{
    Ground,
    Glide,
    Rise,
    Fall
}
public class PlayerMovement : MonoBehaviour
{   
    [Header("Lateral Movemnt")]
    [SerializeField] private float maxSpeed = 5f;
    private float maxSpeedSq;
    [SerializeField] private Transform orientation;

    [Header("Jumping")]
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float jumpForce = 5f;
    public KeyCode jumpKey;
    [SerializeField] private bool canJump = true;

    [Header("Gliding")]
    [SerializeField] private float glideGravProp = 0.8f;
    [SerializeField] private bool canGlide = false;

    [Header("Checks for Groundedness")]
    [SerializeField] private float heightEpsilon = 0.005f;
    [SerializeField] private float playerHeight;    
    public LayerMask groundMask;

    [Header("Player State")]
    [SerializeField] private playerState state = playerState.Ground;

    [Header("Acceleration Constants")]
    [SerializeField] private float groundAccel = 5f;
    [SerializeField] private float airAccel = 1f;
    
    [Header("Drag Constants")]
    [SerializeField] private float groundDrag = 1f;
    [SerializeField] private float airDrag = 0.2f;

    // storing inputs
    private float sidewaysInput;
    private float forwardsInput;
    private bool jumpInput;

    private Vector3 playerDir;
    private Rigidbody rigidBody;
    private float accel;

    // store inputs from movement keys
    private void ProcessInput()
    {
        forwardsInput = Input.GetAxisRaw("Vertical");
        sidewaysInput = Input.GetAxisRaw("Horizontal");
        jumpInput = Input.GetKey(jumpKey);
    }

    // move player according to movement keys and current player state
    private void MovePlayer()
    {
        playerDir = orientation.forward * forwardsInput + orientation.right * sidewaysInput;
        rigidBody.AddForce(playerDir.normalized * maxSpeed * accel);
    }

    // limit non vertical speed to a designated max speed
    private void SpeedLimiter()
    {
        Vector3 vertVel = Vector3.up * rigidBody.velocity.y;
        Vector3 flatVel = rigidBody.velocity - vertVel;
        if (flatVel.sqrMagnitude > maxSpeedSq) {
            rigidBody.velocity = flatVel.normalized * maxSpeed + vertVel;
        }
    }

    // check if player is on the ground, within margin for error
    private bool GroundCheck()
    {
        // shoot downward ray to check if on ground
        return Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + heightEpsilon, groundMask);
    }

    // cause the player to jump
    private void Jump()
    {
        // immediately disallow jumping
        canJump = false;

        // set y velocity to 0 to make every jump the same height
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);
        
        // add upward force
        rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // cause the player to glide, reduces gravity by glideGravProp*100 percent
    // for fixed timestep in which its called
    private void Glide()
    {
        rigidBody.AddForce(Physics.gravity * rigidBody.mass * -glideGravProp);
    }

    // allow player to jump again
    private void MakeReadyToJump()
    {
        canJump = true;
    }

    // set accel and drag multipliers according to player state
    private void SetAccelAndDrag()
    {
        if (state == playerState.Ground) {
            accel = groundAccel;
            rigidBody.drag = groundDrag;
        } else {
            accel = airAccel;
            rigidBody.drag = airDrag;
        } 
    }
    
    private void Start()
    {
        // store rigidbody and disallow rotation, allow jumping from start
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
        canJump = true;
        canGlide = false;
        maxSpeedSq = maxSpeed * maxSpeed;
    }

    private void Update()
    {
        // process movement input keys
        ProcessInput();

        // store if player is on the ground for current frame
        if (GroundCheck() && state != playerState.Rise) {
            state = playerState.Ground;
            canGlide = false;
        }

        // when player starts falling, set new state and allow gliding
        if (state == playerState.Rise & rigidBody.velocity.y < 0) {
            state = playerState.Fall;
            canGlide = true;
        }

        if (jumpInput) {

            // if player is on ground and can jump, jump
            if (state == playerState.Ground && canJump) {

                Jump();

                // set new player state
                state = playerState.Rise;

                // disallow jumping for set time
                Invoke(nameof(MakeReadyToJump), jumpCooldown);
            }
            // if player is not on ground and can glide, glide
            else if (canGlide) {
                state = playerState.Glide;
            }
        }
        // if jump key is not pressed and player was gliding, change state
        // to falling and disallow gliding
        else if (state == playerState.Glide) {
            state = playerState.Fall;
            canGlide = false;
        }
        
        // set drag and acceleration values according to air/ground state
        SetAccelAndDrag();
    }

    private void FixedUpdate()
    {
        // move player
        MovePlayer();
        
        // glide if player is in glide player state
        if (state == playerState.Glide) Glide();
    }

    private void LateUpdate()
    {
        // trigger speed limiter to limit speed if needed
        SpeedLimiter();
    }

}
