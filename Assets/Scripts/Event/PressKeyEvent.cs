using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressKeyEvent : MonoBehaviour
{
    [Header("Key")]
    [SerializeField] private KeyCode hotkey;

    [Header("Events")]
    [SerializeField] private UnityEvent pressEvent;

    void Update()
    {
        if (Input.GetKeyDown(hotkey))
        {
            pressEvent?.Invoke();
        }
    }
}
