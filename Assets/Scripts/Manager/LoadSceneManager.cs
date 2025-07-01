using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneManager : MonoBehaviour
{
    [SerializeField] private LoadingScreenUI loadingScreenUI;

    public void LoadScene(string sceneName)
    {
        LoadingScreenUI.LoadScene(sceneName, loadingScreenUI);
    }
}
