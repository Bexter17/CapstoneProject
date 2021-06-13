using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarnyWeapon : MonoBehaviour
{
    //Script to check if trigger box collider on weapon is currently colliding with player
    private bool weaponContact = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Knife in player");
            weaponContact = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        weaponContact = false;
    }
    public bool GetWeaponContact()
    {
        return weaponContact;
    }
}
