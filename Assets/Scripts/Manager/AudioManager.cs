using System;
using System.Collections;
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
    private bool isFadingMusic;
    private bool isFadingSFX;

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
        if (!isFadingMusic)
            musicSource.volume = musicVolume * masterVolume;

        if (!isFadingSFX)
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

    public void FadeMusic(float duration, bool fadeIn)
    {
        StartCoroutine(FadeAudio(musicSource, fadeIn, duration));
    }

    public void FadeSFX(float duration, bool fadeIn)
    {
        StartCoroutine(FadeAudio(sfxSource, fadeIn, duration));
    }

    private IEnumerator FadeAudio(AudioSource source, bool fadeIn, float duration)
    {
        bool isMusic = source == musicSource;
        if (isMusic) isFadingMusic = true;
        else isFadingSFX = true;

        float time = 0f;

        if (fadeIn && !source.isPlaying)
            source.Play();

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            float currentTarget = (source == musicSource)
                ? musicVolume * masterVolume
                : sfxVolume * masterVolume;

            float factor = fadeIn ? t : 1f - t;

            source.volume = currentTarget * factor;

            yield return null;
        }

        if (!fadeIn)
            source.Stop();

        if (isMusic) isFadingMusic = false;
        else isFadingSFX = false;

        ApplyVolumes();
    }

    public void PauseAll(bool stopCompletely)
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            if (stopCompletely)
                musicSource.Stop();
            else
                musicSource.Pause();
        }

        if (sfxSource != null && sfxSource.isPlaying)
        {
            if (stopCompletely)
                sfxSource.Stop();
            else
                sfxSource.Pause();
        }

        allAudioSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var audio in allAudioSources)
        {
            if (audio == musicSource || audio == sfxSource) continue;

            if (stopCompletely)
                audio.Stop();  
            else
                audio.Pause();
        }
    }

    public void ResumeAll()
    {
        if (musicSource != null)
            musicSource.UnPause();

        if (sfxSource != null)
            sfxSource.UnPause();

        allAudioSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var audio in allAudioSources)
        {
            if (audio == musicSource || audio == sfxSource) continue;
            audio.UnPause();
        }
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
