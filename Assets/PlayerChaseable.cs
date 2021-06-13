using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChaseable : MonoBehaviour
{
    private bool playerChaseable = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OffNavmesh"))
        {
            Debug.Log("Big Bear is here");
            playerChaseable = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OffNavmesh"))
        {
            Debug.Log("Big Bear is there");
            playerChaseable = true;
        }
    }

    public bool ReturnChaseable()
    {
        return playerChaseable;
    }
}
