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
    private AudioSource[] allAudioSources;

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
    }

    public void MuteAllExceptManager()
    {
        allAudioSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var audio in allAudioSources)
        {
            if (audio == musicSource || audio == sfxSource)
                continue;

            audio.mute = true;
        }
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
        var sounds = Array.FindAll(musicSounds, s => s.name == name);
        if (sounds == null || sounds.Length == 0) return;

        var sound = sounds[UnityEngine.Random.Range(0, sounds.Length)];
        musicSource.clip = sound.clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    public void PlaySFX(string name)
    {
        var sounds = Array.FindAll(sfxSounds, s => s.name == name);
        if (sounds == null || sounds.Length == 0) return;

        var sound = sounds[UnityEngine.Random.Range(0, sounds.Length)];
        sfxSource.PlayOneShot(sound.clip, sfxVolume * masterVolume);
    }

    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume * masterVolume;
    public float GetSFXVolume() => sfxVolume * masterVolume; 
}

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}
