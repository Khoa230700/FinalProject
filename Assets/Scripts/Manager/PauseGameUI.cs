using UnityEngine;

public class PauseGameUI : MonoBehaviour
{
    public static PauseGameUI Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public static bool isPause;

    public void Resume()
    {
        Time.timeScale = 1f;
        isPause = false;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        isPause = true;
    }
}
