using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LoadingMode { Single, Additive }

public class LoadSceneManager : MonoBehaviour
{
    public LoadingMode loadingMode = LoadingMode.Single;
    public string sceneToLoad;

    public void LoadScene(string sceneName)
    {
        if (loadingMode == LoadingMode.Single)
        {
            LoadingScreenUI.LoadScene(sceneName);
        }
        else if (loadingMode == LoadingMode.Additive)
        {
            LoadingScreenUI.LoadSceneAdditive(sceneName);
        }
    }
}
