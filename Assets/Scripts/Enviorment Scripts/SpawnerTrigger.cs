using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerTrigger : MonoBehaviour
{


    public GameObject sandwall;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.tag == "Player")
    //    {
    //        GameObject.Find("EnemySpawn").GetComponent<EnemySpawn>().spawn = false;
    //    }
    //}
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("SPAWN");
            GameObject.Find("EnemySpawn 1").GetComponent<EnemySpawn>().spawn = true;
            GameObject.Find("EnemySpawn 2").GetComponent<EnemySpawn>().spawn = true;
            GameObject.Find("EnemySpawn 3").GetComponent<EnemySpawn>().spawn = true;
            sandwall.SetActive(true);
        }
    }
}
