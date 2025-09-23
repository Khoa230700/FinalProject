using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    void Start()
    {
        // Preload instance khi game bắt đầu
        LoadingScreenUI.Preload();
    }

    public void LoadScene(string sceneName)
    {
        LoadingScreenUI.LoadScene(sceneName);
        Debug.Log($"Loading scene: {sceneName}");
    }

    public void ReloadCurrentScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PauseGameUI.Instance.Resume();
        SaveLoadManager.Instance.LoadNow();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
