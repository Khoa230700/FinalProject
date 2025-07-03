using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneManager : MonoBehaviour
{
    // void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         DeathUI.Test();
    //     }
    // }

    public void LoadScene(string sceneName)
    {
        LoadingScreenUI.LoadScene(sceneName);
    }
}
