using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Main
    {
        get
        {
            return Camera.main.GetComponent<AudioManager>();
        }
    }

    public HashSet<Sound> sounds =
       new HashSet<Sound>();



    public new AudioSource audio;

    void Start()
    {
        audio = this.GetComponent<AudioSource>();
    }

    /// Creates a new sound, registers it, gives it the properties specified, and starts playing it
    public Sound PlayNewSound(string soundName, bool loop = false, bool interrupts = false, Action<Sound> callback = null)
    {
        Sound sound = NewSound(soundName, loop, interrupts, callback);
        sound.playing = true;
        return sound;
    }

    /// Creates a new sound, registers it, and gives it the properties specified
    public Sound NewSound(string soundName, bool loop = false, bool interrupts = false, Action<Sound> callback = null)
    {
        Sound sound = new Sound(soundName);
        RegisterSound(sound);
        sound.loop = loop;
        sound.interrupts = interrupts;
        sound.callback = callback;
        return sound;
    }

    /// Registers a sound with the AudioManager and gives it an AudioSource if necessary
    /// Probably best to avoid calling this function directly and just use 
    /// NewSound and PlayNewSound instead
    public void RegisterSound(Sound sound)
    {
        sounds.Add(sound);
        sound.audioManager = this;
        if (sound.source == null)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = sound.clip;
            sound.source = source;
        }
    }

    private void Update()
    {
        sounds.ToList().ForEach(sound => {
            sound.Update();
        });
    }

    void FootstepL (float volume = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load("Foot Step 2"));
        audio.volume = volume;
    }

    void FootstepR(float volume = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load("Foot step 8"));
        audio.volume = volume;
    }

    void Jumpland(float volume = 1f, float spatialBlend = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load("Foot step 8"));
        audio.volume = volume;
        audio.spatialBlend = spatialBlend;
    }

    public void FirstAttackSFX(float volume = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load("Swing_01_WithReverb"));
        audio.volume = volume;

        //PlayNewSound("Swing_01_WithReverb", false, false, null);
    }

    public void SecondAttackSFX(float volume = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load("Swing_02_withReverb"));
        audio.volume = volume;

        //PlayNewSound("Swing_02_withReverb", false, false, null);
    }

    public void ThirdAttackSFX(float volume = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load("Swing_03_WithReverb"));
        audio.volume = volume;

        //PlayNewSound("Swing_03_WithReverb", false, false, null);
    }

    public void WhirlwindSFX(float volume = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load("Whirlwind_02_Voice"));
        audio.volume = volume;
    }

    public void HammerSmashSFX(float volume = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load("HammerSmash_01_Voice"));
        audio.volume = volume;
    }

    public void DashSFX(float volume = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load("Whirlwind_02_NoVoice"));
        audio.volume = volume;
    }

    public void BjornOnHitSFX(float volume = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load(""));
        audio.volume = volume;
    }

    //public void EnemyOnHitSFX(float volume = 1f)
    //{
    //    audio.PlayOneShot((AudioClip)Resources.Load("Body Hit"));
    //    audio.volume = volume;
    //}

    public void ThrowSFX(float volume = 1f)
    {
        audio.PlayOneShot((AudioClip)Resources.Load("Bjorn_LineA_Take03"));
        audio.volume = volume;
    }

}