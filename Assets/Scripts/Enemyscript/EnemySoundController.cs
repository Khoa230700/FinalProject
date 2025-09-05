using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemySoundController : MonoBehaviour
{
    public AudioClip attackClip;
    public AudioClip attackClip2;
    public AudioClip deathClip;

    private AudioSource audioSource;
    private bool isDead = false;

    public static int CurrentPlayingSounds = 0;
    public static int MaxPlayingSounds = 1;
    public static int CurrentDeathSounds = 0;
    public static int MaxDeathSounds = 2;

    

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    public void PlayAttackSound()
    {
        if (!audioSource.isPlaying && !isDead && attackClip != null && CurrentPlayingSounds < MaxPlayingSounds)
        {
            audioSource.clip = attackClip;
            audioSource.Play();
            CurrentPlayingSounds++;

            StartCoroutine(ResetSound(attackClip.length));
        }
    }

    public void PlayAttackSound2()
    {
        if (!audioSource.isPlaying && !isDead && attackClip2 != null && CurrentPlayingSounds < MaxPlayingSounds)
        {
            audioSource.clip = attackClip2;
            audioSource.Play();
            CurrentPlayingSounds++;

            StartCoroutine(ResetSound(attackClip2.length));
        }
    }

    public void PlayDeathSound()
    {
        if (!isDead && deathClip != null && CurrentDeathSounds < MaxDeathSounds)
        {
            isDead = true;
            audioSource.Stop(); // Stop current sound

            audioSource.clip = deathClip;
            audioSource.Play();

            CurrentPlayingSounds++;
            CurrentDeathSounds++;

            StartCoroutine(ResetSound(deathClip.length)); // still count towards general sounds
            StartCoroutine(ResetDeathSound(deathClip.length));
        }
    }

    private IEnumerator ResetSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        CurrentPlayingSounds = Mathf.Max(0, CurrentPlayingSounds - 1);
    }

    private IEnumerator ResetDeathSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        CurrentDeathSounds = Mathf.Max(0, CurrentDeathSounds - 1);
    }
}
