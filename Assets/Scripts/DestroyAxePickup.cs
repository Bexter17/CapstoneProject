using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAxePickup : MonoBehaviour
{
    CharacterMechanics cm;

    public GameObject Player;

    private void Start()
    {
        cm = Player.GetComponent<CharacterMechanics>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Bjorn recieves throwing axe");
            cm.hasRangedWeapon = true;
            Destroy(gameObject);
        }
    }
}