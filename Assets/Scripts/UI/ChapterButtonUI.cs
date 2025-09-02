using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChapterButtonUI : MonoBehaviour
{
    [Header("References")]
    public Button button;
    public GameObject statusNone, statusCompleted, statusLocked, bgLocked;

    [Header("Settings")]
    public string sceneName;
    public string prevScene;

    void Start()
    {
        UpdateState();
    }

    public void UpdateState()
    {
        bool prevCompleted = true;

        if (!string.IsNullOrEmpty(prevScene))
        {
            prevCompleted = PlayerPrefs.GetInt(prevScene + "_Completed", 0) == 1;
        }

        bool completed = PlayerPrefs.GetInt(sceneName + "_Completed", 0) == 1;

        if (!prevCompleted) //Locked
        {
            bgLocked.SetActive(true);
            statusLocked.SetActive(true);
            statusNone.SetActive(false);
            statusCompleted.SetActive(false);
            button.interactable = false;
        }
        else if (completed) //Completed
        {
            statusCompleted.SetActive(true);
            statusNone.SetActive(false);
            statusLocked.SetActive(false);
            bgLocked.SetActive(false);
            button.interactable = true;

            button.onClick.AddListener(() => LoadLevel());
        }
        else //None
        {
            statusNone.SetActive(true);
            statusCompleted.SetActive(false);
            statusLocked.SetActive(false);
            bgLocked.SetActive(false);
            button.interactable = true;

            button.onClick.AddListener(() => LoadLevel());
        }
    }

    private void LoadLevel()
    {
        FindAnyObjectByType<LoadSceneManager>().LoadScene(sceneName);
    }
}
