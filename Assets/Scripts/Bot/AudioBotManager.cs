using UnityEngine;

public class AudioBotManager : MonoBehaviour
{
    public static AudioBotManager Instance;
    [Header("Bot Sounds")]
    [SerializeField] private AudioSource botAudioSource;
    [SerializeField] private AudioClip[] botSounds;
    [SerializeField] private AudioClip shoot;
    private int currentClipIndex = 0;
    private float shootSoundCooldown = 0f;
    private float soundCooldownTime = 0; // delay 0.5s giữa các lần phát
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

        public void PlayBotSound()
    {
        if (!botAudioSource.isPlaying && botSounds.Length > 0)
        {
            botAudioSource.clip = botSounds[currentClipIndex];
            botAudioSource.Play();
          
            currentClipIndex = (currentClipIndex + 1) % botSounds.Length;
        }
    }
    public void StopBotSound()
    {
        if (botAudioSource.isPlaying)
        {
            botAudioSource.Stop();
        }
    }
    public void ShootSound()
    {
        if (shoot != null && shootSoundCooldown <= 0f)
        {
            botAudioSource.volume = 0.3f;
            botAudioSource.PlayOneShot(shoot);
            shootSoundCooldown = soundCooldownTime;
        }
    }

}
