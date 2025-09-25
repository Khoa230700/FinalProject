using UnityEngine;

public class AudioBotManager : MonoBehaviour
{
    public static AudioBotManager Instance;
    [Header("Bot Sounds")]
    [SerializeField] private AudioSource botAudioSource;
    [SerializeField] private AudioClip[] botSounds;
    [SerializeField] private AudioClip shoot;
    [SerializeField] private AudioClip[] melee;
    private int currentClipIndex = 0;
    private float shootSoundCooldown = 0f;
    private float soundCooldownTime = 0; // delay 0.5s giữa các lần phát
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
    }

        public void PlayBotSound()
    {
        if (!botAudioSource.isPlaying && botSounds.Length > 0)
        {
            botAudioSource.clip = botSounds[currentClipIndex];
            botAudioSource.volume = AudioManager.Instance.GetSFXVolume();
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
            botAudioSource.volume = AudioManager.Instance.GetSFXVolume();
            //botAudioSource.play
            AudioSource.PlayClipAtPoint(shoot, transform.position, AudioManager.Instance.GetSFXVolume());
            shootSoundCooldown = soundCooldownTime;
        }
    }
    public void MeleeSound()
    {
        if (melee == null || melee.Length == 0 || botAudioSource == null)
            return;

        if (!botAudioSource.isPlaying)
        {
            int randomIndex = Random.Range(0, melee.Length); // Chọn ngẫu nhiên từ 0 đến melee.Length - 1
            botAudioSource.clip = melee[randomIndex];
            botAudioSource.volume = AudioManager.Instance.GetSFXVolume();
            botAudioSource.Play();
        }

    }

}
