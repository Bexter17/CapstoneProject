using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimController : MonoBehaviour
{
    #region Player

    GameObject Player;

    #region Scripts

    CharacterMechanics cm;

    InputControl ic;

    InputBuffer ib;

    #endregion

    #endregion

    #region Components

    Animator animator;

    Text debugText;

    #endregion

    #region Animator Variables

    AnimatorClipInfo[] currentClipInfo;

    private string animName;

    [SerializeField] bool animDebug;

    #endregion

    #region Movement

    bool isFalling;

    bool isJumping;

    bool isGrounded;

    float Speed;

    float strafeSpeed;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        #region Player

        Player = GameObject.FindGameObjectWithTag("Player");

        #region Scripts

        cm = Player.GetComponent<CharacterMechanics>();

        ic = Player.GetComponent<InputControl>();

        ib = Player.GetComponent<InputBuffer>();

        #endregion

        #endregion

        #region Debug

        try
        {
            debugText = GameObject.FindGameObjectWithTag("Anim Debug Window").GetComponent<Text>();
        }

        catch (MissingComponentException e)
        {
            Debug.LogError(e.Message);
        }

        #endregion

        #region Animator

        animator = Player.GetComponent<Animator>();

        int idleId = Animator.StringToHash("Idle");

        int runId = Animator.StringToHash("Run");

        int attack1Id = Animator.StringToHash("Attack 1");

        int attack2Id = Animator.StringToHash("Attack 2");

        int attack3Id = Animator.StringToHash("Attack 3");

        AnimatorStateInfo animStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        //Automatically disables Root Motion (to avoid adding motion twice)
        animator.applyRootMotion = false;

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        ic.updateValues();

        updateParameters();

        currentClipInfo = this.animator.GetCurrentAnimatorClipInfo(0);

        //currentAnimLength = currentClipInfo[0].clip.length;

        // animName = currentClipInfo[0].clip.name;

        //if (animDebug)
        //{
        //    debugText.gameObject.SetActive(true);

        //    if (debugText)
        //    {
        //        debugText.text = "Animation Debug \nCurrent Animation: " + animName + "\nactionAllowed: " + ib.actionAllowed + "\nParameters:\nSpeed = " + animator.GetFloat("Speed")
        //            + "\nStrafe = " + animator.GetFloat("Strafe") + "\nisGrounded = " + animator.GetBool("isGrounded") + "\nisFalling = " + animator.GetBool("isFalling")
        //            + "\nisJumping = " + animator.GetBool("isJumping") + "\ncomboCount = " + animator.GetInteger("Counter");

        //        debugText.color = Color.green;
        //        //Debug.Log("Animation: animName = " + animName);
        //        //Debug.Log("Animation: actionAllowed = " + ib.actionAllowed);
        //    }
        //}

        //else
        //    debugText.gameObject.SetActive(false);

        if (animName == "Male Attack 1" && ib.actionAllowed || animName == "Male Attack 2" && ib.actionAllowed || animName == "Male Attack 3" && ib.actionAllowed)
        {
            cm.comboCount = 0;

            Debug.Log("Animation System: comboCount reset by update");
        }

        if (animName == "Idle" && !ib.actionAllowed)
        {
            ib.setBufferTrue();

            Debug.Log("actionAllowed reset by Idle");
        }
        #region Debug Log

        if (animDebug)
        {
            Debug.Log("Animator System: Anim Name" + animName);

            //Debug.Log("Animator System: Anim Length" + currentAnimLength);
        }

        #endregion
    }

    private void LateUpdate()
    {
        
        if (animName != "HammerCharge")
        {
            if (cm.dashTemp && cm.dashReady)
            {
                cm.dashEnds();

                Debug.LogError("dashTemp destroyed due to animState!");
                Debug.Log("animState = " + animName);
            }
        }
        
    }

    public void Die()
    {
        animator.SetTrigger("Die");
    }

    public void attackEnd()
    {
        Debug.Log("Triggers Reset!");

        animator.ResetTrigger("Attack");

        animator.ResetTrigger("Dash");

        animator.ResetTrigger("Hammer Smash");

        animator.ResetTrigger("Spin");

        animator.ResetTrigger("Throw");
    }


    public void updateValues(bool grounded, bool jumping, bool falling, float speed, float strafe)
    {

        Debug.Log("ALI - Speed: " + speed);
        Speed = speed;

        strafeSpeed = strafe;

        isGrounded = grounded;

        isJumping = jumping;

        isFalling = falling;
    }

    public void hitGround()
    {       
        animator.SetBool("isGrounded", isGrounded);

        animator.SetBool("isFalling", isFalling);

        if (isJumping)
        {
            isJumping = false;

            animator.SetBool("isJumping", isJumping);
        }
    }

    public void setAbilities(bool abilities)
    {
        animator.SetBool("isUsingAbilities", abilities);
    }

    private void updateParameters()
    {
        animator.SetFloat("Speed", Speed);

        animator.SetFloat("Strafe", strafeSpeed);

        animator.SetBool("isGrounded", isGrounded);

        animator.SetBool("isJumping", isJumping);

        animator.SetBool("isFalling", isFalling);
    }
    public void takeDamage()
    {
        animator.SetInteger("Counter", cm.comboCount);

       // animator.SetTrigger("Got Hit");
    }

    public void jump(bool _isGrounded, bool _isJumping, bool _isFalling)
    {
      //  animator.SetTrigger("Jump");

        animator.SetBool("isJumping", _isJumping);

        animator.SetBool("isGrounded", _isGrounded);

        animator.SetBool("isFalling", _isFalling);
    }

    public void setGrounded(bool _isGrounded)
    {
        animator.SetBool("isGrounded", _isGrounded);
    }

    public void setJumping(bool _isJumping)
    {
        animator.SetBool("isJumping", _isJumping);
    }

    public void setFalling(bool _isFalling)
    {
        animator.SetBool("isFalling", _isFalling);
    }

    public void attack(int _comboCount)
    {
        animator.SetInteger("Counter", _comboCount);

        animator.SetTrigger("Attack");
    }

    public void setComboCount(int _comboCount)
    {
        animator.SetInteger("Counter", _comboCount);
    }

    public void dash()
    {
        resetCounter();

        animator.SetTrigger("Dash");
    }

    public void smash()
    {
        resetCounter();

        animator.SetTrigger("Hammer Smash");
    }

    public void spin()
    {
        if (animDebug)
            Debug.LogError("animControl: spin called");

        resetCounter();

        animator.SetTrigger("Spin");
    }

    public void throw_()
    {
        Debug.Log("Ranged animations called");

        resetCounter();

        animator.SetTrigger("Throw");
    }

    public void respawn()
    {
        animator.SetTrigger("Respawn");
    }


    public void resetCounter()
    {
        animator.SetInteger("Counter", 0);
    }

    #region On Anim State Enter

    

    #endregion
}
