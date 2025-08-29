using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneManager : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        LoadingScreenUI.LoadScene(sceneName);
        Debug.Log($"Loading scene: {sceneName}");
    }
}
