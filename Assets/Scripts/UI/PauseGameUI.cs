using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGameUI : MonoBehaviour
{
    public static PauseGameUI Instance { get; private set; }
    private MeshMouseLook meshMouseLook;

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        if (meshMouseLook != null) meshMouseLook.Hide();
        AudioManager.Instance.ResumeAll();
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        isPause = true;
        if (meshMouseLook != null) meshMouseLook.Show();
        AudioManager.Instance.PauseAll(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
