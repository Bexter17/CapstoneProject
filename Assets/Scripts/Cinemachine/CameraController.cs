using UnityEngine;
using System.Collections;
using Cinemachine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    #region Scripts

    AnimController ac;

    CharacterMechanics cm;

    InputBuffer ib;

    AbilitiesCooldown cooldown;

    AimShoot aim;

    InputControl ic;

    #endregion

    private bool ThirdPersonCamera = true;

    GameObject Player;

    public Image Aimer;

    public GameObject respawnPoint;

    [SerializeField]
    public CinemachineVirtualCamera vCam1; //Third Person Camera
    [SerializeField]
    public CinemachineVirtualCamera vCam2; //Free Look Camera
    [SerializeField]
    public CinemachineVirtualCamera vCam3; //End game camera


    void Awake()
    {
        Aimer.enabled = false;
    }

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

        cm = Player.transform.GetComponent<CharacterMechanics>();

        ic = Player.transform.GetComponent<InputControl>();

        #region Components


        //controllerList = Input.GetJoystickNames();

        cooldown = GameObject.FindGameObjectWithTag("Abilities").GetComponent<AbilitiesCooldown>();

        //ib = this.transform.GetComponent<InputBuffer>();

        //cm = this.transform.GetComponent<CharacterMechanics>();

        //ac = this.transform.GetComponent<AnimController>();

        //aim = this.transform.GetComponent<AimShoot>();

        #endregion

       // vCam2.transform.position = respawnPoint.transform.position;
    }
    

    void Update()
    {
        if (cm.IsAimOn)
        {
            Debug.Log("Right Mouse Button Clicked");
            ThirdPersonCamera = false;
            Aimer.enabled = true;
            SwitchPriority();
        }

        if (!cm.IsAimOn)
        {
           // vCam2.transform.position = respawnPoint.transform.position;
            Debug.Log("Right Mouse Button Let go");
            ThirdPersonCamera = true;
            Aimer.enabled = false;
            SwitchPriority();
        }
    }

    private void SwitchPriority()
    {
        Debug.Log("Switch State Running");
        if (ThirdPersonCamera)
        {
            vCam1.Priority = 3;
        }
        else
        {
            vCam2.Priority = 1;
        }

        if (ic.endGame == true)
        {
            vCam3.Priority = 10;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.CompareTag("EndCamera");
    }

}