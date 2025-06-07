using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManagerUI : MonoBehaviour
{
    [Header("Panel List")]
    [SerializeField] private List<PanelOfSettings> panels = new();

    //* Index
    [SerializeField] private int currentIndex = 0;
    private int newIndex;

    //* GameObect
    private GameObject currentPanel;
    private GameObject nextPanel;
    private GameObject currentButton;
    private GameObject nextButton;

    public void OpenFirstPanel()
    {
        if (currentIndex != 0)
        {
            OpenPanel(panels[0].panelName);
        }
        else
        {
            currentPanel = panels[currentIndex].panel;
            currentButton = panels[currentIndex].button;

            currentPanel.GetComponent<Animator>().Play("In");
            currentButton.GetComponent<Animator>().Play("In");
        }
    }

    public void OpenPanel(string newPanel)
    {
        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i].panelName == newPanel)
            {
                newIndex = i;
                break;
            }
        }

        if (newIndex != currentIndex)
        {
            //* GameObject
            currentPanel = panels[currentIndex].panel;
            currentButton = panels[currentIndex].button;
            nextPanel = panels[newIndex].panel;
            nextButton = panels[newIndex].button;

            currentIndex = newIndex;

            //* Animator
            currentPanel.GetComponent<Animator>().Play("Out");
            currentButton.GetComponent<Animator>().Play("Out");
            nextPanel.GetComponent<Animator>().Play("In");
            nextButton.GetComponent<Animator>().Play("In");
        }
    }

    public void NextPage()
    {
        if (currentIndex <= panels.Count - 2)
        {
            currentPanel = panels[currentIndex].panel;
            currentButton = panels[currentIndex].button;

            currentIndex += 1;

            nextPanel = panels[currentIndex].panel;
            nextButton = panels[currentIndex].button;

            currentPanel.GetComponent<Animator>().Play("Out");
            // currentButtonAnimator = currentButton.GetComponent<Animator>();
            // currentButtonAnimator.Play(buttonFadeNormal);

            nextPanel.GetComponent<Animator>().Play("In");
            // nextButtonAnimator = nextButton.GetComponent<Animator>();
            // nextButtonAnimator.Play(buttonFadeIn);
        }
    }

    public void PrevPage()
    {
        if (currentIndex >= 1)
        {
            currentPanel = panels[currentIndex].panel;
            currentButton = panels[currentIndex].button;
            currentIndex -= 1;
            nextPanel = panels[currentIndex].panel;
            nextButton = panels[currentIndex].button;

            currentPanel.GetComponent<Animator>().Play("Out");
            // currentButtonAnimator = currentButton.GetComponent<Animator>();
            // currentButtonAnimator.Play(buttonFadeNormal);

            nextPanel.GetComponent<Animator>().Play("In");
            // nextButtonAnimator = nextButton.GetComponent<Animator>();
            // nextButtonAnimator.Play(buttonFadeIn);
        }
    }
}

[Serializable]
public class PanelOfSettings
{
    public string panelName;
    public GameObject panel;
    public GameObject button;
}
