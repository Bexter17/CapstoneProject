using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; 

public class InputControl : MonoBehaviour
{
    #region Components

    CharacterController controller;

    [SerializeField] private GameObject respawnPoint;

    public bool endGame = false;

    #endregion

    #region Scripts

    AnimController ac;

    CharacterMechanics cm;

    InputBuffer ib;

    AbilitiesCooldown cooldown;

    AimShoot aim;

    CameraController cc;

    #endregion

    string[] controllerList;

    #region Debug

    [Header("Debugs")]

    [SerializeField] bool jumpDebug;

    [SerializeField] bool inputDebug;

    #endregion

    #region Movement

    enum controlType { Controller, Keyboard }

    controlType currentDevice;

    [SerializeField] bool invertX;

    [SerializeField] bool invertY;

    //Determines how fast the character moves
    [SerializeField] private float movementSpeed;

    [SerializeField] private float currentSpeed;

    //Rotation speed of the character
    [SerializeField] private float rotationSpeed;

    Vector2 inputVec;

    //Variable used to add force or direction to the character
    Vector3 moveDirection;

    Vector3 lookDirection;

    Vector3 strafeDirection;

    //How much we are boosting the speed by
    [SerializeField] private float speedBoost;

    #endregion

    #region Jumping and Falling

    //RaycastHit groundHit;

    //public float groundSearchLength = 0.6f;

    Vector3 characterSize;

    //Variable for how high the character will jump
    [SerializeField] private float minimumJumpPower;

    [SerializeField] private float currentJumpPower;

    [SerializeField] private float maxJumpPower;

    [SerializeField] private float jumpAmplifier;

    [SerializeField] float currentJumpTime;

    [SerializeField] float maxJumpTime;

    private float vSpeed = 0;

    //Amount of gravity set on the player
    [SerializeField] private float gravity;

    //Boolean to track if the player is on the ground or in the air
    public bool isGrounded;

    public bool isJumping;

    public bool isFalling;

    [SerializeField] GameObject raycastSpawn;

    #endregion

    #region Dash

    bool dashRequested;

    float accelerationTime;

    [SerializeField] float dashAcceleration;

    [SerializeField] float maxDashTime;

    float velocityReducer;

    float accelerationReducer;

    int currentDashFrame;

    int maxDashFrames;

    float initPositionZ;

    float initVelocityZ;

    float zPosition;

    #endregion

    #region Camera 

    public float mouseHorizontalRotationSpeed = 0.65f;

    public float mouseVerticalRotationSpeed = 0.65f;

    public float controllerHorizontalRotationSpeed = 0.65f;

    public float controllerVerticalRotationSpeed = 0.65f;

    GameObject thirdPersonCam;

    public Transform Target, Player;

    Vector2 mouseVec;

    float mouseX, mouseY;

    #endregion 

    // Start is called before the first frame update
    void Start()
    {
        #region Initialization

        try
        {
            #region Components

            controller = this.transform.GetComponent<CharacterController>();

            //       controllerList = Input.GetJoystickNames();

            cooldown = GameObject.FindGameObjectWithTag("Abilities").GetComponent<AbilitiesCooldown>();

            ib = this.transform.GetComponent<InputBuffer>();

            cm = this.transform.GetComponent<CharacterMechanics>();

            ac = this.transform.GetComponent<AnimController>();

            aim = this.transform.GetComponent<AimShoot>();

            cc = this.transform.GetComponent<CameraController>();

            #endregion

            #region Movement

            if (movementSpeed <= 0)
                movementSpeed = 6.0f;

            if (maxJumpPower <= 0)
                maxJumpPower = 2.0f;

            if (minimumJumpPower == 0)
                minimumJumpPower = 0.6f;

            currentJumpPower = 0;

            if (rotationSpeed <= 0)
                rotationSpeed = 2.0f;     //4.0f was original

            if (gravity <= 0)
                gravity = 9.81f;

            if (maxDashFrames == 0)
                maxDashFrames = 10;

            if (dashAcceleration == 0)
                dashAcceleration = 20f;

            if(maxDashTime == 0)
                maxDashTime = 0.03f;
            
            if (jumpAmplifier == 0)
                jumpAmplifier = 10;

            if (velocityReducer == 0)
                velocityReducer = 0.00000001f;

            if (accelerationReducer == 0)
                accelerationReducer = 0.00001f;

            if (maxJumpTime == 0)
                maxJumpTime = 0.5f;

            dashRequested = false;

            //Assigns a value to the variable
            moveDirection = Vector3.zero;

            characterSize = this.transform.localScale;

            raycastSpawn = GameObject.FindGameObjectWithTag("Raycast Spawn");

            raycastSpawn.transform.parent = this.transform;

            //raycastSpawn.transform.localPosition = new Vector3(0.0f, characterSize.y * 0.5f, 0.0f);

            // Changed the additive value to 1.25f from 0.2f
           // groundSearchLength = raycastSpawn.transform.position.y + 1.25f;

            //groundSearchLength = (characterSize.y * 0.5f);

            #endregion

            #region Camera

            thirdPersonCam = GameObject.FindGameObjectWithTag("Third Person Cam");

            Target = thirdPersonCam.transform;

            Player = this.transform;

            #endregion
        }

        catch (MissingReferenceException e)
        {
            Debug.LogError(e.Message);
        }

        #endregion

        #region Respawn

        //if (!respawnPoint)
        //    respawnPoint = GameObject.FindGameObjectWithTag("Starting Respawn Point");

        //            respawnPoint = GameManager.Instance.GetCurrentCheckpoint();

        if (GameManager.Instance.HauntedHouse)
        {
            respawnPoint = GameObject.FindWithTag("HauntedExit");
            if (respawnPoint != null)
            {
                GameManager.Instance.HauntedHouse = false;
            }
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "Level_1")
                respawnPoint = GameManager.Instance.GetCurrentCheckpoint();
        }


        if (respawnPoint)
        {
            transform.position = respawnPoint.transform.position;
        }
        #endregion
    }

    private void FixedUpdate()
    {
        if (cm.isPlaying)
        {
            if (cm.isAlive)
            {
                /*
                if(currentDevice == controlType.Controller)
                {
                    UnityEngine.InputSystem.InputControlScheme = InputControlScheme.FindControlSchemeForDevices;
                }

                if (currentDevice == controlType.Keyboard)
                {

                }
                */

                if (this.transform.parent)
                    Debug.Log("Parent: " + this.transform.parent.name);

                isGrounded = groundCheck();

                if (!isGrounded)
                {
                    if (this.transform.parent)
                    {
                        if (this.transform.parent.tag == "Platform")
                            this.transform.parent = null;
                    }
                }

                #region Debug

                if (jumpDebug)
                    Debug.Log("jumpDebug: groundCheck returns = " + isGrounded);

                #endregion

                ac.setGrounded(isGrounded);

                currentSpeed = movementSpeed + speedBoost;

                #region Dash

                if (dashRequested)
                {
                    accelerationTime += Time.deltaTime;

                    currentDashFrame++;

                    if (currentDashFrame < maxDashFrames)
                    {

                        // zPosition = initPositionZ + (((initVelocityZ * accelerationTime) + (0.5F * dashAcceleration * (accelerationTime * accelerationTime)) * 0.5f) * 0.01f * 0.000000000000000000000000000000000000000000000000000000000000000000000000001f);
                        //zPosition = initPositionZ + (dashAcceleration / maxDashFrames);
                        zPosition = dashAcceleration / maxDashFrames;

                        Vector3 newPos = new Vector3(0, 0, zPosition);

                        newPos = this.transform.TransformDirection(newPos);
                        if (obstacleCheck(newPos))
                            controller.Move(newPos);

                        else
                            Debug.LogWarning("Dash blocked by obstacle!");

                        if (inputDebug)
                        {
                            Debug.Log("Dash Frame: " + currentDashFrame + " initPositionZ = " + initPositionZ);

                            Debug.Log("Dash Frame: " + currentDashFrame + " initVelocityZ = " + initVelocityZ);

                            Debug.Log("Dash Frame: " + currentDashFrame + " zPosition = " + zPosition);

                            Debug.Log("Dash Frame: " + currentDashFrame + " accelerationTime = " + accelerationTime);

                            Debug.Log("Dash Frame: " + currentDashFrame + " dashAcceleration = " + dashAcceleration);

                            Debug.Log("Dash Frame: " + currentDashFrame + " newPos = " + newPos);

                            //  Debug.Log("Dash Frame: " + frame + " + initPositionZ + (initVelocityZ * velocityReducer * accelerationTime");

                            Debug.Log("Dash Frame: " + currentDashFrame + " distance traveled = " + (this.transform.position.z - initPositionZ));

                            Debug.Log("Dash Frame: " + currentDashFrame + " initial velocity = " + initVelocityZ);

                            //  Debug.Log("Dash Frame: " + currentDashFrame + " adjusted velocity = " + (initVelocityZ * velocityReducer * accelerationTime * 0.1f));
                        }
                    }

                    else if (currentDashFrame >= maxDashFrames)
                        resetDashParameters();

                    //else if (accelerationTime >= maxDashTime)
                    //resetDashParameters();
                }

                #endregion

                #region Check Jumping and Falling States

                if (!isJumping)
                {
                    if (!isGrounded)
                    {
                        if (groundCheck())
                        {
                            land();

                            ac.setFalling(isFalling);
                            ac.setJumping(isJumping);
                        }

                        if (!groundCheck() && !isJumping)
                        {
                            if (!isFalling)
                            {
                                fall();

                                ac.setFalling(isFalling);
                            }
                        }
                    }
                }

                else
                {
                    currentJumpTime += Time.deltaTime;

                    if (currentJumpTime >= maxJumpTime)
                        JumpEnd();
                }

                #endregion

                #region Apply Gravity

                /*
                if (isJumping)
                {
                    isFalling = false;

                    if (jumpDebug)
                        Debug.Log("isJumping true");

                    if (currentJumpPower < maxJumpPower)
                    {
                        if (jumpDebug)
                            Debug.Log("jumpPower increased");
                        currentJumpPower++;
                    }

                    else if (currentJumpPower >= maxJumpPower)
                    {
                        if (jumpDebug)
                            Debug.Log("reached maxJumpPower");
                        JumpEnd();
                    }


                    if (jumpDebug)
                        Debug.Log("Jump force applied by JumpPower");

                 //   vSpeed += maxJumpPower * Time.deltaTime;// * jumpAmplifier;

                    if (jumpDebug)
                        Debug.Log("vSpeed = " + vSpeed);
                }
            */

                if (!isGrounded && !isJumping)
                    isFalling = true;

                if (isFalling)
                {
                    if (jumpDebug)
                        Debug.Log("isFalling = true");

                    vSpeed -= gravity * Time.deltaTime;

                    if (jumpDebug)
                    {
                        Debug.Log("vSpeed reduced by gravity");
                        Debug.Log("vSpeed = " + vSpeed);
                    }
                }

                moveDirection.y = vSpeed;

                if (!dashRequested)
                    if (controller)
                        controller.Move(moveDirection * Time.deltaTime * currentSpeed);

                    else
                        Debug.LogError("controller not assigned!");

                Debug.Log("moved controller by " + moveDirection * Time.deltaTime * currentSpeed);
                Debug.Log("moveDirection = " + moveDirection);

                #endregion

                #region Debug

                if (jumpDebug)
                {
                    Debug.Log("jumpDebug: isGrounded =" + isGrounded);

                    Debug.Log("jumpDebug: isFalling =" + isFalling);

                    Debug.Log("jumpDebug: isJumping =" + isJumping);

                    Debug.Log("Player transform position = " + Player.transform.position);

                    try
                    {
                        Debug.Log("Model transform position = " + Player.GetChild(7).position);
                    }

                    catch (MissingReferenceException e)
                    {
                        Debug.LogError(e.Message);
                    }
                }

                #endregion
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (cm)
            if (cm.isPlaying)
            {
                //controller.center = new Vector3(0, raycastSpawn.transform.position.y, 0);

                //Assign "moveDirection" to track vertical movement
                //   moveDirection = new Vector3(0, 0, Input.GetAxis("Vertical"));

                //   strafeDirection = new Vector3(0, Input.GetAxis("Horizontal"), 0);

                //Character rotation
                //transform.Rotate(0, Input.GetAxis("Horizontal") * rotationSpeed, 0);

                //track any applied speed boosts

                //Character movement
                //Vector3 forward = transform.TransformDirection(Vector3.forward);

                //Movement speed
                //float curSpeed = Input.GetAxis("Vertical") * currentSpeed;

                //Character controller movement
                //   controller.SimpleMove(transform.forward * (Input.GetAxis("Vertical") * currentSpeed));

                //   controller.SimpleMove(transform.right * (Input.GetAxis("Horizontal") * currentSpeed));

            }
        */
    }
    

    #region Camera

    private void LateUpdate()
    {
        if (!endGame)
        {
            CamControl();
        }
        else
        {
            cc.vCam3.Priority = 10;
            movementSpeed = 0;
            currentSpeed = 0;
        }
        
        // changeDirection();

        if (!isJumping && isFalling && isGrounded)
            land();
    }

    void CamControl()
    {
        if(!invertX)
        mouseX += mouseVec.x;

        else
            mouseX += -mouseVec.x;

        if (!invertY)
            mouseY -= mouseVec.y;

        else
            mouseY -= -mouseVec.y;

        mouseY = Mathf.Clamp(mouseY, -35, 60);

        thirdPersonCam.transform.LookAt(Target);

        Target.rotation = Quaternion.Euler(mouseY, mouseX, 0);

        Player.rotation = Quaternion.Euler(0, mouseX, 0);

        //Player.transform.localEulerAngles = new Vector3(0, mouseX, 0);
        //changeDirection();
        //lookDirection = transform.TransformDirection(lookDirection);

        moveDirection = new Vector3(inputVec.x, 0, inputVec.y);

        moveDirection = transform.TransformDirection(moveDirection);
    }

    #endregion

    #region Input System Commands
    public void OnMouseCamera(InputValue input)
    {
        if (cm)
        {
            if (cm.isPlaying)
            {
                mouseVec = input.Get<Vector2>();

                mouseVec.x *= mouseHorizontalRotationSpeed;

                mouseVec.x *= mouseHorizontalRotationSpeed;

                cm.rotatePlayer(mouseVec);
            }
        }
    }

    public void OnControllerCamera(InputValue input)
    {
        if (cm)
        {
            if (cm.isPlaying)
            {
                mouseVec = input.Get<Vector2>();

                mouseVec.x *= controllerHorizontalRotationSpeed;

                mouseVec.y *= controllerVerticalRotationSpeed;

                cm.rotatePlayer(mouseVec);
            }
        }
    }

    public void OnPlayerMove(InputValue input)
    {
        if (cm.isPlaying)
        {
            /*
            if (dashRequested)
            {
                Debug.Log("moved during dash, resetting dash");
                resetDashParameters();
            }

            if (cm.dashTemp)
                cm.dashEnds();
            */

            inputVec = Vector2.zero;
            inputVec = input.Get<Vector2>();
            Debug.Log("ALI - " + inputVec.y);

            //moveDirection = Vector3.zero;
        }
    }

    public void resetMovement()
    {
        inputVec = Vector2.zero;

        mouseVec = Vector2.zero;

        isJumping = false;

        dashRequested = false;
    }

    void OnPause()
    {
        cm.toggleIsPlaying();
    }

    void changeDirection(Vector3 direction)
    {

    }

    //  public void checkKeyboardInput()
    // {
    //float x = Input.GetAxisRaw("Horizontal");

    //float z = Input.GetAxisRaw("Vertical");

    //if (x > 0.01f || x < -0.01f)
    //{

    //}

    //if (z > 0.01f || z < -0.01f)
    //{

    //}

    //if (Input.GetButtonDown("Jump") && isGrounded)
    //{

    public void OnJump()
    {
        if (cm.isPlaying)
        {
            #region Debug Log

            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: Jump has been pressed");
            }

            #endregion

            if (dashRequested)
            {
                Debug.Log("ability used during dash, resetting dash");
                resetDashParameters();
            }

            if (ib.checkBuffer(ActionItem.InputAction.Jump))
            {
                #region Debug

                if (inputDebug)
                {
                    Debug.Log("input: checkBuffer returned true");
                }

                #endregion

                ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Jump, Time.time));
            }
        }
    }

    
    public void OnJumpEnd()
    {
        if (jumpDebug)
            Debug.Log("OnJmpEnd() called");

        JumpEnd();
    }
    

    public Vector2 getMouseData(Vector2 input)
    {
        input = mouseVec;

        return input;
    }


    //Enables the player to use Ability 1
    //if (Input.GetButtonDown("Fire1"))
    //{
    public void OnBasicAttack()
    {
        if (cm.isPlaying)
        {
            if (dashRequested)
            {
                Debug.Log("ability used during dash, resetting dash");
                resetDashParameters();
            }

            if (ib.checkBuffer(ActionItem.InputAction.Attack))
            {
                #region Debug

                if (inputDebug)
                {
                    Debug.Log("input: checkBuffer returned true");
                }

                #endregion

                if (ib.actionAllowed)
                {
                    #region Debug

                    if (inputDebug)
                        Debug.Log("input: action allowed");

                    #endregion

                    ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Attack, Time.time));
                    //AttackEnd();
                }
            }
        }
    }

    public void OnMouseAttack()
    {
        if (cm.isPlaying)
        {
            if (!cm.IsAimOn)
            {
                if (dashRequested)
                {
                    Debug.Log("ability used during dash, resetting dash");
                    resetDashParameters();
                }

                if (ib.checkBuffer(ActionItem.InputAction.Attack))
                {
                    #region Debug

                    if (inputDebug)
                    {
                        Debug.Log("input: checkBuffer returned true");
                    }

                    #endregion

                    if (ib.actionAllowed)
                    {
                        #region Debug

                        if (inputDebug)
                            Debug.Log("input: action allowed");

                        #endregion

                        ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Attack, Time.time));
                        //AttackEnd();
                    }
                }
            }

            else
            {
                if (dashRequested)
                {
                    Debug.Log("ability used during dash, resetting dash");
                    resetDashParameters();
                }

                Throw();
            }
        }
    }

    public void OnDash()
    {
        #region Debug

            if (inputDebug)
                Debug.Log("input: OnDash called");

            #endregion

        if (cm.isPlaying)
        {
            if (!cooldown.isCooldown1)
            {
                #region Debug

                if (inputDebug)
                    Debug.Log("input: dash not on cooldown");

                #endregion

        //        List<ActionItem> curBuffer = ib.inputBuffer;

                #region Debug

                if (inputDebug)
                {
                //    Debug.Log("input: curBuffer created");
          //          Debug.Log("input: curBuffer.Count = " + curBuffer.Count);
            //        Debug.LogError("input: ib.inputBuffer.Count = " + ib.inputBuffer.Count);
                }

                #endregion

                if (ib.checkBuffer(ActionItem.InputAction.Dash))
                {
                    #region Debug

                    if (inputDebug)
                    {
                        Debug.Log("input: checkBuffer returned true");
                    }

                    #endregion

                    if (ib.actionAllowed)
                    {
                        #region Debug

                        if (inputDebug)
                            Debug.Log("input: action allowed");

                        #endregion

                        //controller.Move(transform.TransformDirection(Vector3.forward) * inputVec.y * dashAcceleration);
                        ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Dash, Time.time));

                        if (inputDebug)
                            Debug.Log("input: action added to buffer");
                    }

                    else
                    {
                        #region Debug

                        if (inputDebug)
                            Debug.Log("input: action not allowed");

                        #endregion
                    }
                }

                else
                {
                    #region Debug

                    if (inputDebug)
                    {
                        Debug.Log("input: checkBuffer returned false");
                    }

                    #endregion
                }
            }

            else
            {
                #region Debug

                if (inputDebug)
                    Debug.Log("input: dash is on cooldown");

                #endregion
            }
        }
    }

    void OnHammerSmash()
    {
        if (cm.isPlaying)
        {
            //Enables the player to use Ability 3
            //if (Input.GetButtonDown("Fire3") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown3 == false)
            //{
            if (!cooldown.isCooldown3)
            {
                #region Debug Log

                if (ib.inputBufferDebug)
                {
                    Debug.Log("Input Buffer System: hammerSmash has been pressed");
                }

                #endregion

                if (dashRequested)
                {
                    Debug.Log("ability used during dash, resetting dash");
                    resetDashParameters();
                }

                if (ib.checkBuffer(ActionItem.InputAction.HammerSmash))
                {
                    #region Debug

                    if (inputDebug)
                    {
                        Debug.Log("input: checkBuffer returned true");
                    }

                    #endregion

                    ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.HammerSmash, Time.time));

                    //cm.AttackEnd();
                }
            }
        }
    }

    public void OnWhirlwind()
    {
        if (cm.isPlaying)
        {
            if (!cooldown.isCooldown2)
            {
                if (dashRequested)
                {
                    Debug.Log("ability used during dash, resetting dash");
                    resetDashParameters();
                }

                #region Debug Log
                if (ib.inputBufferDebug)
                {
                    Debug.Log("Input Buffer System: whirlwind has been pressed");
                }
                #endregion

                if (ib.checkBuffer(ActionItem.InputAction.Whirlwind))
                {
                    #region Debug

                    if (inputDebug)
                    {
                        Debug.Log("input: checkBuffer returned true");
                    }

                    #endregion

                    ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Whirlwind, Time.time));
                }
            }
            //cm.AttackEnd();
        }
    }

    public void OnAimIn()
    {
        if (cm.isPlaying)
        {
            if (dashRequested)
            {
                Debug.Log("ability used during dash, resetting dash");
                resetDashParameters();
            }

            cm.setAimTrue();
        }
    }

    public void OnAimOut()
    {
        if (cm.isPlaying)
        {
            if (dashRequested)
            {
                Debug.Log("ability used during dash, resetting dash");
                resetDashParameters();
            }

            cm.setAImFalse();
        }
    }

    public void OnThrow()
    {
        if (cm.isPlaying)
        {
            if (dashRequested)
            {
                Debug.Log("ability used during dash, resetting dash");
                resetDashParameters();
            }

            if (ib.checkBuffer(ActionItem.InputAction.Ranged))
            {
                #region Debug

                if (inputDebug)
                {
                    Debug.Log("input: checkBuffer returned true");
                }

                #endregion
                if(ib.actionAllowed)
                Throw();
            }
        }
    }

    #endregion

    #region Ability Functions

    void Throw()
    {
        #region Debug Log

        if (ib.inputBufferDebug)
        {
            Debug.Log("Input Buffer System: ranged attack has been pressed");
        }

        #endregion

        if (cm.hasRangedWeapon)
        {
            if (!cooldown.isCooldown4)
            {
                #region Debug

                if (inputDebug)
                    Debug.Log("input: Throw not on cooldown");

                #endregion

                if (ib.checkBuffer(ActionItem.InputAction.Ranged))
                {
                    #region Debug

                    if (inputDebug)
                    {

                    }

                    #endregion

                    if (ib.actionAllowed)
                    {
                        #region Debug

                        if (inputDebug)
                            Debug.Log("input: action allowed");

                        #endregion

                        ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Ranged, Time.time));
                        //cm.AttackEnd();
                    }
                    else
                    {
                        #region Debug

                        if (inputDebug)
                            Debug.Log("input: action not allowed");

                        #endregion
                    }
                }

                else
                {
                    #region Debug

                    if (inputDebug)
                    {
                        Debug.Log("input: not added to input buffer");
                    }

                    #endregion
                }
            }

            else
            {
                #region Debug

                if (inputDebug)
                    Debug.Log("input: Throw is on cooldown");

                #endregion
            }
        }

        else
        {
            #region Debug

            if (inputDebug)
                Debug.Log("input: player does not have Ranged attack yet!");

            #endregion
        }
    }

    void Attack()
    {
        #region Debug Log

        if (ib.inputBufferDebug)
        {
            Debug.Log("Input Buffer System: Attack has been pressed");
        }

        #endregion

        //if (ib.inputBuffer.Count < 2 && ib.inputBuffer[0].Action != ActionItem.InputAction.Attack && ib.inputBuffer[1].Action != ActionItem.InputAction.Attack)
        //{
        //    #region Debug

        //    if (inputDebug)
        //    {
        //        Debug.Log("input: Buffer Count = " + ib.inputBuffer.ToArray());

        //        Debug.Log("input: Buffer 1 = " + ib.inputBuffer[0].Action);

        //        Debug.Log("input: Buffer 2 = " + ib.inputBuffer[1].Action);
        //    }

        //    #endregion

            if (ib.actionAllowed)
            {
                #region Debug

                if (inputDebug)
                    Debug.Log("input: action allowed");

                #endregion

                ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Attack, Time.time));
                //AttackEnd();
            }

        //    else
        //    {
        //        #region Debug

        //        if (inputDebug)
        //            Debug.Log("input: action not allowed");

        //        #endregion
        //    }
        //}

        //else
        //{
        //    #region Debug

        //    if (inputDebug)
        //    {
        //        Debug.Log("input: not added to input buffer");

        //        Debug.Log("input: Buffer Count = " + ib.inputBuffer.ToArray());

        //        Debug.Log("input: Buffer 1 = " + ib.inputBuffer[0].Action);

        //        Debug.Log("input: Buffer 2 = " + ib.inputBuffer[1].Action);
        //    }

        //    #endregion
        //}
    }


    public void dash()
    {
        #region Debug

        if (inputDebug)
            Debug.Log("input: dash called");

        #endregion

        dashRequested = true;

        accelerationTime = 0;

        currentDashFrame = 0;

        initPositionZ = this.transform.position.z;

        initVelocityZ = inputVec.y;

        zPosition = 0;

        cm.createDashTemp();

        //zPosition = initPositionZ + (((initVelocityZ * velocityReducer * accelerationTime * 0.1f) + (0.5F * (dashAcceleration * accelerationReducer) * (accelerationTime * accelerationTime)) * 0.5f * 0.5f) * 0.1f);
        
       // zPosition = initPositionZ + ((initVelocityZ * velocityReducer) + (0.5f * (dashAcceleration * accelerationReducer) * 0.5f) * 0.0001f);
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (raycastSpawn)
            Gizmos.DrawSphere(raycastSpawn.transform.position, 0.5f);

        else
            Debug.LogError("raycastSpawn not found!");
    }

    #endregion

    #region Ground Check
    public bool groundCheck()
    {
        //Vector3 lineStart = raycastSpawn.transform.position;
        //Vector3 vectorToSearch = new Vector3(lineStart.x, lineStart.y - groundSearchLength, lineStart.z);

        //Debug.DrawLine(lineStart, vectorToSearch, Color.cyan);

        Collider[] potentialGrounds = Physics.OverlapSphere(raycastSpawn.transform.position, 0.5f);
        if (potentialGrounds.Length > 0)
        {
            for (int i = 0; i < potentialGrounds.Length; i++)
            {
                
                if (potentialGrounds[i].tag == "Floor" ||
                    potentialGrounds[i].tag == "Box" ||
                    potentialGrounds[i].tag == "Picnic Table" ||
                    potentialGrounds[i].tag == "Train Car" ||
                    potentialGrounds[i].tag == "Trash Can" ||
                    potentialGrounds[i].tag == "Test Of Strength" ||
                    potentialGrounds[i].tag == "Planks")
                {
                    if (this.transform.parent == potentialGrounds[i].transform)
                        this.transform.parent = null;

                    return true;
                }
                else if (potentialGrounds[i].tag == "Platform")
                {
                    this.transform.parent = potentialGrounds[i].transform;

                    return true;
                } 
            }
        }

        return false;

        //if (Physics.Linecast(lineStart, vectorToSearch, out groundHit))
        //{
        //    if (groundHit.transform.tag == "Floor" ||
        //        groundHit.transform.tag == "Box" ||
        //        groundHit.transform.tag == "Picnic Table" ||
        //        groundHit.transform.tag == "Train Car" ||
        //        groundHit.transform.tag == "Trash Can" ||
        //        groundHit.transform.tag == "Test Of Strength")
        //    {
        //        if (this.transform.parent == groundHit.transform)
        //            this.transform.parent = null;

        //        return true;
        //    }

        //    else if (groundHit.transform.tag == "Platform")
        //    {
        //        this.transform.parent = groundHit.transform;

        //        return true;
        //    }

        //    else
        //    {
        //        if (this.transform.parent == groundHit.transform)
        //            this.transform.parent = null;

        //        return false;
        //    }
        //}

        //else
        //    return false;
    }

    #endregion

    #region Obstacle Check

    bool obstacleCheck(Vector3 pos)
    {
        Vector3 lineStart = raycastSpawn.transform.position;
        Vector3 vectorToSearch = new Vector3(lineStart.x, lineStart.y, lineStart.z + pos.z);

        RaycastHit obstacleHit;

        Debug.DrawLine(lineStart, vectorToSearch, Color.cyan);

        if (Physics.Linecast(lineStart, vectorToSearch, out obstacleHit))
        {
            if (obstacleHit.transform.tag == "Fence" ||
                obstacleHit.transform.tag == "Train Car" ||
                obstacleHit.transform.tag == "Booth" ||
                obstacleHit.transform.tag == "Picnic Table")
            {
                return false;
            }

            else
                return true;
        }

        else return true;
    }

    #endregion

    #region Collisions 

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("OnControllerColliderHit: " + hit.gameObject.name);

        if (hit.gameObject.tag == "Floor")
        {
            //isGrounded = true;

            //isFalling = false;

            //        land();

            if (isJumping)
            {
                //          isJumping = false;

                //        ib.setBufferTrue();
            }

            ac.hitGround();
        }

        else if (hit.gameObject.tag == "Killbox")
        {
            respawnPoint = GameManager.Instance.GetCurrentCheckpoint();

            Debug.Log("respawnPoint = " + respawnPoint.transform.name);

            if (respawnPoint)
                gameObject.transform.position = respawnPoint.transform.position;

            if (ac)
                ac.respawn();
            //die();
        }
    }

    #endregion

    #region Jump

    public void jump()
    {
        #region Debug Log

        if (jumpDebug)
        {
            Debug.Log("jump has been called");
        }

        #endregion

        if (isGrounded)
        {
            cm.comboReset();

            if (cm.dashTemp)
                cm.dashEnds();

            currentJumpPower = minimumJumpPower;

            currentJumpTime = 0;

            isGrounded = false;

            isJumping = true;

            isFalling = false;

            ac.jump(isGrounded, isJumping, isFalling);

            ib.setBufferFalse();

            vSpeed = currentJumpPower;// * jumpAmplifier;

            #region Debug Log


            if (jumpDebug)
            {
                Debug.Log("vSpeed: " + vSpeed);

                Debug.Log("isGrounded = " + isGrounded);

                Debug.Log("isJumping = " + isJumping);

                Debug.Log("actionAllowed = " + ib.actionAllowed);
            }

        }

        #endregion
    }

    private void JumpEnd()
    {
        if (jumpDebug)
            Debug.Log("JumpEnd Called");

        isJumping = false;

        ac.setJumping(isJumping);

        currentJumpPower = 0;

        if (!groundCheck())
        {
            fall();
        }
    }
    void fall()
    {
        if (jumpDebug)
            Debug.Log("fall() Called");

        if (isGrounded)
            isGrounded = false;

        ac.setGrounded(isGrounded);

        if (isJumping)
            isJumping = false;

        ac.setJumping(isJumping);

        if (!isFalling)
            isFalling = true;

        ac.setFalling(isFalling);
    }

    void land()
    {
        if (jumpDebug)
            Debug.Log("jumpDebug: land() Called");

        if (isJumping)
            isJumping = false;

        ac.setJumping(isJumping);

        isFalling = false;

        ac.setFalling(isFalling);

        isGrounded = true;

        ac.setGrounded(isGrounded);

        vSpeed = 0;

        if (!ib.actionAllowed)
            ib.setBufferTrue();

    }

    #endregion

    public void updateValues()
    {
        ac.updateValues(isGrounded, isJumping, isFalling, inputVec.y, inputVec.x);
    }

    void resetDashParameters()
    {
        dashRequested = false;

        accelerationTime = 0;

        cm.dashEnds();
    }

    #region Look Inversion

    public void OnInvertY()
    {
        toggleCameraY();
    }

    public void OnInvertX()
    {
        toggleCameraX();
    }


    public bool toggleCameraY()
    {
        if (invertY)
            invertY = false;
        else
            invertY = true;

        return invertY;

    }

    public bool toggleCameraX()
    {
        if (invertX)
            invertX = false;
        else
            invertX = true;

        return invertX;
    }

    public void freeAction()
    {
        ib.setBufferTrue();
    }

    #endregion 
}
