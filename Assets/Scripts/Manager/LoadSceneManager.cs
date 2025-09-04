using UnityEngine;

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
}
