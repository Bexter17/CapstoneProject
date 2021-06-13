using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameCamera : MonoBehaviour
{
    #region Scripts

    AnimController ac;

    CharacterMechanics cm;

    InputControl ic;

    AbilitiesCooldown cooldown;

    CinemachineDollyCart dc;

    CameraController cc;

    #endregion

    private AudioSource _as;
    
    enum Cinematic { EndGameCinematic };

    // Start is called before the first frame update
    void Start()
    {
        #region Initialization

        ic = GameObject.FindGameObjectWithTag("Player").GetComponent<InputControl>();

        cm = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMechanics>();

        dc = GameObject.FindGameObjectWithTag("dc").GetComponent<CinemachineDollyCart>();

        #endregion
    }

    void Awake()
    {
        _as = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        EndGameCinematic();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //Disable player cursor
            ic.endGame = true;
            //Dolly Cart Speed
            dc.m_Speed = 8f;
            _as.Play();
            StartCoroutine(EndGameCinematic());

        }
    }

    IEnumerator EndGameCinematic()
    {
        yield return new WaitForSeconds(12.2f);
        SceneManager.LoadScene("CreditScene");
    }
}