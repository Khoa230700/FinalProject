using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyBindingPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject hotkeysSettingPanel;
    [SerializeField] private GameObject closeButtonKeyBinding;
    private Animator animator;
    private bool isListen;
    private Action<KeyCode> OnComplete;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!isListen) return;

        if (Input.GetMouseButtonDown(0) && IsPointerOver(closeButtonKeyBinding.transform))
        {
            Hide();
            return;
        }

        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                if (keyCode == KeyCode.Escape)
                {
                    Complete(KeyCode.None);
                    return;
                }

                Complete(keyCode);
                return;
            }
        }
    }

    public void Show(Action<KeyCode> callback, string keyName)
    {
        OnComplete = callback;
        descriptionText.text = "Press a key for " + keyName.ToUpper();
        hotkeysSettingPanel.SetActive(false);
        animator.Play("In");

        isListen = true;
    }

    public void Hide()
    {
        hotkeysSettingPanel.SetActive(true);
        isListen = false;
        animator.Play("Out");
    }

    private void Complete(KeyCode keyCode)
    {
        Hide();
        OnComplete?.Invoke(keyCode);
    }

    bool IsPointerOver(Transform target)
    {
        var data = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        return results.Exists(r => r.gameObject.transform.IsChildOf(target));
    }
}
