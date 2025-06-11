using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [Header("Audio")]
    public Sound[] musicSounds, sfxSounds;

    [Header("Resources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        PlayMusic("Test");
    } 
    public void PlayMusic(string name)
    {
        var sound = Array.Find(musicSounds, s => s.name == name);
        if (sound != null)
        {
            musicSource.clip = sound.clip;
            musicSource.Play();
        }
    }

    public void PlaySfx(string name)
    {
        var sound = Array.Find(sfxSounds, s => s.name == name);
        if (sound != null)
        {
            sfxSource.clip = sound.clip;
            sfxSource.Play();
        }
    }
}

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}
