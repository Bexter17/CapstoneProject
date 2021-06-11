using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVoices : MonoBehaviour
{
    private AudioSource _as;

    public AudioClip[] voiceLines;

    private void Awake()
    {
        _as = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _as.clip = voiceLines[Random.Range(0, voiceLines.Length)];
        _as.PlayOneShot(_as.clip);
    }
}
