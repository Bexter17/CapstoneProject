using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDestroy : MonoBehaviour
{
    [SerializeField] bool smashDebug;

    public GameObject ability;


    // Start is called before the first frame update
    void Start()
    {
        if(smashDebug)
        Debug.Log("Smash Prefab created!");
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(ability, 1); 
    }
}
