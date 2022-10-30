using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// enumeration of different player states that affect accel, drag and movement
// abilities
public enum PlayerState
{
    Ground,
    Steep,
    Glide,
    Rise,
    Fall
}

public class PlayerControl : MonoBehaviour
{   
    [Header("Lateral Movemnt")]
    [SerializeField] private float maxSpeed = 5f;
    private float maxSpeedSq;
    [SerializeField] private Transform orientation;

    [Header("Jumping")]
    [SerializeField] private float jumpCooldown = 0.05f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private KeyCode jumpKey;
    [SerializeField] private bool canJump = true;

    [Header("Gliding")]
    [SerializeField] private float glideGravProp = 0.8f;
    [SerializeField] private bool glideInput;
    [SerializeField] private bool canGlide = false;

    [Header("Interacting")]
    [SerializeField] private KeyCode useKey;
    [SerializeField] private bool canUse;
    [SerializeField] private float useCooldown = 0.2f;
    [SerializeField] private float useRange = 1f;
    [SerializeField] private float ejectForce = 5f;
    [SerializeField] private bool useInput;
    [SerializeField] private bool isHolding;
    [SerializeField] private InteractiveObject heldObject = null;
    [SerializeField] private float holdDistance = 1.5f;
    private Vector3 localHoldPos;
    [SerializeField] private float detachDistThreshold = 0.15f;

    [Header("Checks for Groundedness")]
    [SerializeField] private float heightEpsilon = 0.005f;
    //[SerializeField] private float playerHeight;
    private new CapsuleCollider collider;
    // transform scaling used on player, used to get unscaled radius of collider
    // may create issues with ground detection if z and x scaling are different
    [SerializeField] private readonly float playerScaleFactor;
    public LayerMask groundMask;
    private Vector3[] firingPoints;

    [Header("Walking on Slopes")]
    [SerializeField] private float maxAngle = 30f;
    [SerializeField] private float slopeThresholdAngle = 5f;
    [SerializeField] private float playerLength;
    [SerializeField] private Vector3 slopeNormal;
    [SerializeField] private float slopeAngle;

    [Header("Player State")]
    [SerializeField] private PlayerState state = PlayerState.Ground;

    [Header("Acceleration Constants")]
    [SerializeField] private float groundAccel = 25f;
    [SerializeField] private float airAccel = 5f;
    
    [Header("Drag Constants")]
    [SerializeField] private float groundDrag = 1f;
    [SerializeField] private float airDrag = 0.2f;

    // Audio Components. Ideas were studied, referenced and adapted from https://gamedevacademy.org/unity-audio-tutorial/
    [Header("Audio Components")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip[] footstepSounds;
    // determines the rate at which to play footstep audio clips
    [SerializeField] float playFootstepAudioRate; 
    // determines the threshold for the player's velocity at which footstep audio clips should be played
    [SerializeField] float playerVelocityFootstepThreshold; 

    [Header("Animation")]
    [SerializeField] PlayerAnimator playerAnimator;

    [Header("Level Controller")]
    [SerializeField] GameObject levelController;
    private float sensitivity;

    // storing inputs
    private float sidewaysInput;
    private float forwardsInput;
    private bool jumpInput;

    private Vector3 playerDir;
    private Rigidbody rigidBody;
    private float accel;

    // Some variables to track for playing audio
    private float lastFootstepTime;

    private float horizontal_movement_x = 0;

    private void Start()
    {
        // store player components and child components
        rigidBody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<PlayerAnimator>();
        sensitivity = levelController.GetComponent<LevelsController>().sensitivity;
        collider = gameObject.GetComponentInChildren<CapsuleCollider>();

        // freeze player rigidbody rotation
        rigidBody.freezeRotation = true;

        // initialise control bools
        canJump = true;
        canGlide = false;
        canUse = true;

        // initialise max speed squared variable for faster calculations
        maxSpeedSq = maxSpeed * maxSpeed;

        // stores the global positions from which groundcheck rays are casted
        firingPoints = new Vector3[5];

        // store local position of held objects
        localHoldPos = orientation.localPosition + Vector3.forward * holdDistance;
    }

    private void Update()
    {
        // process movement input keys
        ProcessInput();

        // store if player is on the ground for current frame
        if (GroundCheck() && state != PlayerState.Rise) {
            if (state != PlayerState.Ground) {

                // disallow jumping for set time after landing
                Invoke(nameof(MakeReadyToJump), jumpCooldown);
            }
            state = PlayerState.Ground;
            canGlide = false;
        }
        // if no longer grounded by didnt jump, set state
        else if (state == PlayerState.Ground) {
            if (rigidBody.velocity.y > 0) {
                state = PlayerState.Rise;
            }
            else {
                state = PlayerState.Fall;
            }
            playerAnimator.TriggerFall();     
        }

        // when player starts falling, set new state and allow gliding
        if (state == PlayerState.Rise & rigidBody.velocity.y < 0) {
            state = PlayerState.Fall;
            canGlide = true;
        }
        // if player is on ground and can jump, jump
        if (jumpInput && state == PlayerState.Ground && canJump) {

            Jump();

            // set new player state
            state = PlayerState.Rise;

        }
        // if player is not on ground and can glide, glide
        else if (glideInput && canGlide && !isHolding) {
            state = PlayerState.Glide;
        }
        // if jump key is not pressed and player was gliding, change state
        // to falling and disallow gliding
        else if (state == PlayerState.Glide) {
            state = PlayerState.Fall;
            canGlide = false;

            // trigger jump animation trigger for glide -> mid jump transition
            playerAnimator.TriggerJumpMid();
        }

        // player pressing use key and not gliding
        if (useInput && state != PlayerState.Glide && canUse) {

            // if holding object, eject
            if (isHolding) {
                heldObject.Eject(orientation.forward, ejectForce);
                heldObject = null;
                isHolding = false;
                canUse = false;
                playerAnimator.TriggerDetach();
            }
            // if not holding object, pick up naerest object within range
            else {

                InteractiveObject[] objs = (InteractiveObject[]) GameObject.FindObjectsOfType(typeof(InteractiveObject));
                InteractiveObject nearest = null;
                float minDist = useRange;

                // loop through interactive objects in scene
                foreach (InteractiveObject obj in objs) {

                    // calculate distance between player and object
                    float distance = Vector3.Distance(transform.TransformPoint(localHoldPos), obj.transform.position);
                
                    // only consider objects in range
                    if (distance < minDist) {
                        minDist = distance;
                        nearest = obj;
                    }
                }
                // attach nearest object
                if (nearest != null) {

                    nearest.Attach(gameObject, localHoldPos);
                    heldObject = nearest;
                    isHolding = true;
                    canUse = false;
                    playerAnimator.TriggerAttach();
                    
                    // trigger goal if object is a goal
                    Goal goal = nearest.GetComponent<Goal>();
                    if (goal != null) {
                        GetComponent<PlayerGoals>().GoalRetrival(goal);
                    }
                }                
            }

            if (! canUse) {
                Invoke(nameof(MakeReadyToUse), useCooldown);
            }
        }
        
        // set drag and acceleration values according to air/ground state
        SetAccelAndDrag();
    }

    private void FixedUpdate()
    {
        // move player
        MovePlayer();

        // check if held object has moved significantly from relative pos
        AutoDetachHeldObject();

        // conditionally make footsteps
        MakeFootsteps();
        
        // glide if player is in glide player state
        if (state == PlayerState.Glide) Glide();
    }

    // When player moves their mouse horizontally, Chiki also turns, and the camera turns along with Chiki
    // This section of Chiki + camera control was referenced, studied and adapted for use from the following resources:
    // https://answers.unity.com/questions/1179680/how-to-rotate-my-camera.html
    // https://gamedevacademy.org/unity-audio-tutorial/
    // https://docs.unity3d.com/ScriptReference/Vector3.html
    // https://docs.unity3d.com/ScriptReference/Transform-eulerAngles.html
    // https://www.youtube.com/watch?v=CxI2OBdhLno (for idea of accumulating mouse movement)
    // https://docs.unity3d.com/ScriptReference/Quaternion.Euler.html
    private void LateUpdate()
    {
        // trigger speed limiter to limit speed if needed
        SpeedLimiter();

        // Accumulate the horizontal_movement conducted by the mouse from the base of 0 (whenever level loads).
        horizontal_movement_x += Input.GetAxis("Mouse X") * sensitivity;
        
        // Doing a rotation around the y-axis makes the camera rotate the view horizontally. Vector3.up is a 
        // shorthand way of writing (0, 1, 0). The code is rotating the y-axis using the horizontal
        // movement of the player's mouse.
        transform.localRotation = Quaternion.Euler(Vector3.up * horizontal_movement_x); 
    }
    // store inputs from movement keys
    private void ProcessInput()
    {
        forwardsInput = Input.GetAxisRaw("Vertical");
        sidewaysInput = Input.GetAxisRaw("Horizontal");
        jumpInput = Input.GetKeyDown(jumpKey);
        glideInput = Input.GetKey(jumpKey);
        useInput = Input.GetKeyDown(useKey);
    }

    // move player according to movement keys and current player state
    // Content researched or code adapted from sources (applies to other parts
    // of this script as well):
    // https://docs.unity3d.com/ScriptReference/Rigidbody.html
    // https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
    // https://youtu.be/xCxSjgYTw9c
    private void MovePlayer()
    {
        // calculate direction of player based on move keys and orientation
        playerDir = orientation.forward * forwardsInput + orientation.right * sidewaysInput;
        
        // project direction onto plane of a slope, if on sufficiently steep slope
        if (slopeAngle > slopeThresholdAngle && slopeAngle < maxAngle) {
            playerDir = Vector3.ProjectOnPlane(playerDir, slopeNormal);
        }

        rigidBody.AddForce(playerDir.normalized * accel, ForceMode.Acceleration);

        // set idle or walk animation triggers
        if (forwardsInput == 0 && sidewaysInput == 0) {
            playerAnimator.TriggerIdle();
        }
        else {
            playerAnimator.TriggerWalk();
        }
    
    }

    // Make footstep noises based on velocity and cooldown
    private void MakeFootsteps()
    {
        // If the player's acceleration is higher than the threshold parameter and the time elapsed since
        // the last footstep sound was played is sufficient, then log the current time (to be used for the
        // next footstep sound) and play a random footstep sound clip.
        if(rigidBody.velocity.magnitude > playerVelocityFootstepThreshold && (Time.time - lastFootstepTime) > playFootstepAudioRate) {
            
            if(state == PlayerState.Ground) {
                lastFootstepTime = Time.time;
                if (footstepSounds.Length > 0) {
                    audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
                }
            }
        }
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

    // check if player is on the ground and if so store important data, 
    // such as angle and normal of slope
    private bool GroundCheck()
    {
        // shoot ray downward, only as far as player could be standing on
        // steepest walkable slope
        slopeAngle = 0;

        Debug.Assert(
            transform.localScale.x == transform.localScale.z &&
            collider.transform.localScale.x == collider.transform.localScale.z
        );

        Vector3 centre = collider.transform.TransformPoint(collider.center);
        float playerHeight = collider.height * transform.localScale.y * collider.transform.localScale.y;
        float radius = collider.radius * transform.localScale.x * collider.transform.localScale.x;
        float playerLength = radius * 2f;

        Debug.Log(radius);
        Debug.DrawLine(centre + Vector3.right * radius, centre + Vector3.left * radius, Color.red, 1f / 25f, false);

        // get current global positions of collider corners and centre
        firingPoints[0] = centre;
        firingPoints[1] = new Vector3(centre.x + radius, centre.y, centre.z);
        firingPoints[2] = new Vector3(centre.x - radius, centre.y, centre.z);
        firingPoints[3] = new Vector3(centre.x, centre.y, centre.z + radius);
        firingPoints[4] = new Vector3(centre.x, centre.y, centre.z - radius);

        // store data from each raycast
        List<float> angles = new List<float>();
        List<float> distances = new List<float>();
        List<Vector3> normals = new List<Vector3>();
        RaycastHit slopeHit;

        // iterate through firing points, storing info if raycast hits
        for (int i = 0; i < 5; i++) {
            bool hit = Physics.Raycast(
                firingPoints[i],
                Vector3.down,
                out slopeHit,
                0.5f * (playerHeight + playerLength * Mathf.Tan((maxAngle / 180) * Mathf.PI)) + heightEpsilon,
                groundMask
            );
            Debug.DrawRay(firingPoints[i], Vector3.down, Color.green, 1f / 25f, false);
            if (hit) {
                angles.Add(Vector3.Angle(Vector3.up, slopeHit.normal));
                distances.Add(slopeHit.distance);
                normals.Add(slopeHit.normal);
            }           
        }

        // return false if no hits
        if (angles.Count == 0) {
            return false;
        }

        // calculate min angle and average angle
        float minAngle = maxAngle;
        float tempAngle = 0;
        Vector3 tempNormal = Vector3.zero;
        for (int i = 0; i < angles.Count; i++) {
            if (angles[i] < minAngle) {
                minAngle = angles[i];
            }
            tempAngle += angles[i];
            tempNormal += normals[i];
        }
        slopeAngle = tempAngle / angles.Count;
        slopeNormal = tempNormal / normals.Count;

        // false if min angle too steep
        if (minAngle > maxAngle) {
            return false;
        }

        // calculate maximum distance away from surface according to slope angle
        float maxDist = 0.5f * (playerHeight + playerLength * Mathf.Tan((slopeAngle / 180) * Mathf.PI)) + heightEpsilon;
        //float minDist = 0.25f * Mathf.Sin((maxAngle / 180) * Mathf.PI) * playerHeight - heightEpsilon;

        // calulate min distance
        float smallestDist = maxDist;
        for (int i = 0; i < distances.Count; i++) {
            if (distances[i] < smallestDist) {
                smallestDist = distances[i];
            }
        }

        // make sure player isnt too far above any surfaces
        if (smallestDist >= maxDist) {
            return false;
        }

        // if just in air, set landing animation trigger
        if (state == PlayerState.Fall || state == PlayerState.Glide) {
            playerAnimator.TriggerLanding();
        }

        return true;
    }

    // cause the player to jump
    private void Jump()
    {
        // immediately disallow jumping
        canJump = false;

        // play jump sound
        audioSource.PlayOneShot(jumpSound);

        // set y velocity to 0 to make every jump the same height
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);
        
        // add upward force
        rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // set jump animation trigger
        playerAnimator.TriggerJumpStart();
    }

    // cause the player to glide, reduces gravity by glideGravProp*100 percent
    // for fixed timestep in which its called
    private void Glide()
    {
        rigidBody.AddForce(Physics.gravity * rigidBody.mass * -glideGravProp);

        // set glider animation trigger
        playerAnimator.TriggerGlide();
    }

    // allow player to jump again
    private void MakeReadyToJump()
    {
        canJump = true;
    }

    // allow player to use again
    private void MakeReadyToUse()
    {
        canUse = true;
    }

    // set accel and drag multipliers according to player state
    private void SetAccelAndDrag()
    {
        if (state == PlayerState.Ground) {
            accel = groundAccel;
            rigidBody.drag = groundDrag;
        } else {
            accel = airAccel;
            rigidBody.drag = airDrag;
        } 
    }

    // auto detaches held object if it gets too far away from in front of player
    private void AutoDetachHeldObject()
    {
        if (!isHolding) return;

        Vector3 posDiff = heldObject.transform.position - transform.TransformPoint(heldObject.localPos);

        if (posDiff.sqrMagnitude > detachDistThreshold) {
            heldObject.Detach();
            isHolding = false;
            heldObject = null;
            playerAnimator.TriggerDetach();
        }
    }

    public PlayerState GetPlayerState()
    {
        return state;
    }
    
    public void updateSensitivity(float sensitivity) {
        this.sensitivity = sensitivity;
    }
    
    public Vector3 GetPlayerPos() {
        return this.gameObject.transform.position;
    }

}
