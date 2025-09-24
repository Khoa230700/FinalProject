using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private string musicName;

    private void Start()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(musicName))
        {
            AudioManager.Instance.PlayMusic(musicName);
        }
    }
}
