using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "Sound", menuName = "Sound", order = 1)]
public class Sound : ScriptableObject
{
    public string soundName;
    public AudioClip clip;

    public float volume;

    [HideInInspector]
    public AudioSource source;

    public float bpm;

}
