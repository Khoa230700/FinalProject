using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private string musicName;
    [SerializeField] private float delay = 0f;
    [SerializeField] private float duration = 0f;

    private void Start()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(musicName))
        {
            Invoke(nameof(PlayMusicNow), delay);
        }
    }

    private void PlayMusicNow()
    {
        AudioManager.Instance.PlayMusic(musicName);
        AudioManager.Instance.FadeMusic(duration, true);
    }
}
