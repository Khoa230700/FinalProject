using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExitGameManager : MonoBehaviour
{
    [SerializeField] private List<PressKeyEvent> pressKeyEvents;

    private void Start()
    {
        if (pressKeyEvents.Count == 0)
        {
            pressKeyEvents = new List<PressKeyEvent>(FindObjectsByType<PressKeyEvent>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void EnablePressKeyEvents(bool enable)
    {
        foreach (var pressKeyEvent in pressKeyEvents)
        {
            pressKeyEvent.enabled = enable;
        }
    }
}
