using System;
using Unity.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound")]
    public Sound[] musicSounds;
    public Sound[] sfxSounds;

    [Header("Resources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private SliderUI masterSlider;
    [SerializeField] private SliderUI musicSlider;
    [SerializeField] private SliderUI sfxSlider;

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private string stringPrefsSlider = "Slider";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(masterSlider.sliderTag + stringPrefsSlider, 100f));
        SetMusicVolume(PlayerPrefs.GetFloat(musicSlider.sliderTag + stringPrefsSlider, 100f));
        SetSFXVolume(PlayerPrefs.GetFloat(sfxSlider.sliderTag + stringPrefsSlider, 100f));

        PlayMusic("Test");
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value / 100f);
        ApplyVolumes();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value / 100f);
        ApplyVolumes();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value / 100f);
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        musicSource.volume = musicVolume * masterVolume;
        sfxSource.volume = sfxVolume * masterVolume;
    }

    public void PlayMusic(string name)
    {
        var sound = Array.Find(musicSounds, s => s.name == name);
        if (sound == null) return;

        musicSource.clip = sound.clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void PlaySFX(string name)
    {
        var sound = Array.Find(sfxSounds, s => s.name == name);
        if (sound == null) return;

        sfxSource.PlayOneShot(sound.clip, sfxVolume * masterVolume);
    }
}

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}
