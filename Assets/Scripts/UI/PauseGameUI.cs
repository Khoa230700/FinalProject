using UnityEngine;

public class PauseGameUI : MonoBehaviour
{
    public static PauseGameUI Instance { get; private set; }
    private MeshMouseLook meshMouseLook;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        meshMouseLook = SelectorSpawner.Instance.Player.GetComponent<MeshMouseLook>();
    }

    public static bool isPause;

    public void Resume()
    {
        Time.timeScale = 1f;
        isPause = false;
        meshMouseLook.Hide();
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        isPause = true;
        meshMouseLook.Show();
        AudioManager.Instance.MuteAllExceptManager();
    }
}
