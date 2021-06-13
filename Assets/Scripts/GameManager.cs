using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointSorter : IComparer {
    int IComparer.Compare(object x, object y) {
        return ((new CaseInsensitiveComparer()).Compare(((GameObject)x).name, ((GameObject)y).name));
    }
}

public class GameManager : MonoBehaviour
{
    GameObject Player;

    CharacterMechanics cm;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
        }

        try
        {
            Player = GameObject.FindGameObjectWithTag("Player");

            cm = Player.GetComponent<CharacterMechanics>();
        }

        catch (NullReferenceException e)
        {
            Debug.Log(e.Message);
        }
    }
    public void BuildCheckpointsList()
    {
        IComparer sorter = new CheckpointSorter();
        checkPoints = GameObject.FindGameObjectsWithTag("CheckPoint");
        Array.Sort(checkPoints, sorter);
    }

    public void UpdateCheckpoint(GameObject newCheckPoint)
    {
        Debug.Log("Checkpoint updated");
        currentCheckpoint = Array.IndexOf(checkPoints, newCheckPoint);
        Debug.Log("newCheckpoint = " + newCheckPoint);
        Debug.Log("currentCheckpoint = " + currentCheckpoint);
    }

    public GameObject GetCurrentCheckpoint()
    {
        return checkPoints[currentCheckpoint];
    
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    if (SceneManager.GetActiveScene().name == "MainMenu")
        //    {
        //        SceneManager.LoadScene("MainScene");
        //    }
        //    else if (SceneManager.GetActiveScene().name == "MainScene")
        //    {
        //        SceneManager.LoadScene("MainMenu");
        //    }
        //    else if (SceneManager.GetActiveScene().name == "EndScene")
        //    {
        //        SceneManager.LoadScene("MainMenu");
        //    }
        //}

        if (SceneManager.GetActiveScene().name == "Level_1")
        {
            if (Player && cm)
            {
                if (cm.isPlaying)
                {
                    Cursor.lockState = CursorLockMode.Locked;

                    Cursor.visible = false;
                }

                if (!cm.isPlaying)
                {
                    Cursor.lockState = CursorLockMode.None;

                    Cursor.visible = true;
                }
            }
        }


        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Cursor.lockState = CursorLockMode.None;

            Cursor.visible = true;
        }

        if (SceneManager.GetActiveScene().name == "CreditScene")
        {
            Cursor.lockState = CursorLockMode.None;

            Cursor.visible = true;
        }

        if (SceneManager.GetActiveScene().name == "EndScene")
        {
            Cursor.lockState = CursorLockMode.None;

            Cursor.visible = true;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level_1");
    }

    public void Return()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void TryAgain()
    {
        SceneManager.LoadScene("Level_1");
    }

    public void Controls()
    {
        SceneManager.LoadScene("Controls");
    }

    public void Options()
    {
        SceneManager.LoadScene("Options");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    private static GameManager instance;

    public static GameManager Instance { get => instance; }
    public bool HauntedHouse { get => hauntedHouse; set => hauntedHouse = value; }

    public GameObject[] checkPoints;
    public int currentCheckpoint;
    private bool hauntedHouse = false;
}