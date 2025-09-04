using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
public class testEnemySound : MonoBehaviour
{
    public AudioClip attackClip;
    public AudioClip attackClip2;

    private AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
    }

    public void PlayAttackSound()
    {
        if (!audioSource.isPlaying  && attackClip != null)
        {
            audioSource.clip = attackClip;
            audioSource.Play();
            
        }
    }
}
