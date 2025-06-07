using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsUI : MonoBehaviour
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

    //* Animator
    string panelIn = "In";
    string panelOut = "Out";
    string buttonIn = "Normal to Pressed";
    string buttonOut = "Pressed to Dissolve";
    string buttonNormal = "Pressed to Normal";

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

            currentPanel.GetComponent<Animator>().Play(panelIn);
            currentButton.GetComponent<Animator>().Play(buttonIn);
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
            currentPanel.GetComponent<Animator>().Play(panelOut);
            currentButton.GetComponent<Animator>().Play(buttonOut);
            nextPanel.GetComponent<Animator>().Play(panelIn);
            nextButton.GetComponent<Animator>().Play(buttonIn);
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

            currentPanel.GetComponent<Animator>().Play(panelOut);
            currentButton.GetComponent<Animator>().Play(buttonOut);

            nextPanel.GetComponent<Animator>().Play(panelIn);
            nextButton.GetComponent<Animator>().Play(buttonIn);
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

            currentPanel.GetComponent<Animator>().Play(panelOut);
            currentButton.GetComponent<Animator>().Play(buttonOut);

            nextPanel.GetComponent<Animator>().Play(panelIn);
            nextButton.GetComponent<Animator>().Play(buttonIn);
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
