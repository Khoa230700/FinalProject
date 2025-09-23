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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
