using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomVoiceLines : MonoBehaviour
{
    private AudioSource _as;

    public AudioClip[] voiceLines;

    private bool istriggered = false;

    void Awake()
    {
        _as = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            istriggered = true;
            _as.clip = voiceLines[Random.Range(0, voiceLines.Length)];
            _as.PlayOneShot(_as.clip);
            
        }
        
            

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            gameObject.GetComponent<Collider>().enabled = false;
        }
            //gameObject.GetComponent<Collider>().enabled = false;
        

    }

    public void Fortuneteller(float volume = 1f)
    {
        _as.clip = voiceLines[Random.Range(0, voiceLines.Length)];
        _as.PlayOneShot(_as.clip);
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

}
