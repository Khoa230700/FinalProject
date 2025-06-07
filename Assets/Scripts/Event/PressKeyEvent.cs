using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressKeyEvent : MonoBehaviour
{
    [Header("Key")]
    [SerializeField] private KeyCode hotkey;

    [Header("Action")]
    [SerializeField] private UnityEvent pressActionOn;
    [SerializeField] private UnityEvent pressActionOff;

    private bool isPressed = false;

    void Update()
    {
        if (Input.GetKeyDown(hotkey))
        {
            if (pressActionOn.GetPersistentEventCount() > 0 && pressActionOff.GetPersistentEventCount() > 0)
            {
                (isPressed ? pressActionOff : pressActionOn)?.Invoke();
                isPressed = !isPressed;
            }
            else
            {
                pressActionOn?.Invoke();
            }
        }
    }

    public void SetIsPressed(bool value) => isPressed = value;
}
