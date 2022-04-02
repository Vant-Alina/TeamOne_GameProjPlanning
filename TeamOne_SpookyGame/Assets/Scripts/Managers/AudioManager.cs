using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] soundEffects;

    public Sound[] music;
    AudioSource mSource;

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound s in soundEffects)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
        }

        mSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(soundEffects, Sound => Sound.soundName == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }

        s.source.Play();
    }

    public void StopSFX(string name)
    {
        Sound s = Array.Find(soundEffects, Sound => Sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }

        s.source.Stop();
    }

    public float GetSFXLength(string name)
    {
        Sound s = Array.Find(soundEffects, Sound => Sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }

        return s.source.clip.length;
    }

    public void PlayMusic(string name)
    {
        mSource.Stop();

        Sound s = Array.Find(music, Sound => Sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
        }

        mSource.clip = s.clip;
        mSource.volume = s.volume;
        mSource.loop = true;

        mSource.Play();
        
    }

    public void StopMusic()
    {
        mSource.Stop();
    }
}
