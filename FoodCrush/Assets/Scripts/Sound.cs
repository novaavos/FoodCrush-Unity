using UnityEngine.Audio;
using UnityEngine;
using System;

[System.Serializable]
public class Sound
{

    public string name;

    public AudioClip clip;

    [Range(.5f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

}
