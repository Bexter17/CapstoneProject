using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] bool checkpointDebug;

    void Start()
    {
    }

    void OnTriggerEnter(Collider other)

    {
        if (other.CompareTag("Player"))
        {
            if(checkpointDebug)
            Debug.Log("checkpoint activated at " + this.transform.name);

            GameManager.Instance.UpdateCheckpoint(gameObject);
        }
    }
}
