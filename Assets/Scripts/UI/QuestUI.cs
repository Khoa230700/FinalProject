using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool isOpen = false;

    void Start()
    {
        animator ??= GetComponent<Animator>();
    }

    void Update()
    {
        if (KeyBindingManager.Instance.GetKeyDown("Open Quest"))
        {
            if(!isOpen)
            {
                animator.Play("In");
                isOpen = true;
            }
            else
            {
                animator.Play("Out");
                isOpen = false;
            }
        }
    }
}
