using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform listRoot;
    [SerializeField] private GameObject questStepPrefab;

    private bool isOpen = false;
    private List<GameObject> stepGOs = new();

    void Start()
    {
        animator ??= GetComponent<Animator>();
    }

    void Update()
    {
        if (KeyBindingManager.Instance.GetKeyDown("Open Quests"))
        {
            ToggleUI();
        }
    }

    public void Setup(string title)
    {
        titleText.text = title;
    }

    public void UpdateUI(List<string> steps)
    {
        // Đảm bảo số lượng stepGO phù hợp
        for (int i = 0; i < steps.Count; i++)
        {
            if (i < stepGOs.Count)
            {
                stepGOs[i].SetActive(true);
                stepGOs[i].transform.Find("Text").GetComponent<TextMeshProUGUI>().text = steps[i];
            }
            else
            {
                GameObject stepGO = Instantiate(questStepPrefab, listRoot);
                stepGO.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = steps[i];
                stepGOs.Add(stepGO);
            }
        }

        // Ẩn các step cũ không còn dùng nữa
        for (int i = steps.Count; i < stepGOs.Count; i++)
        {
            stepGOs[i].SetActive(false);
        }
    }

    private void ToggleUI()
    {
        isOpen = !isOpen;
        animator.Play(isOpen ? "In" : "Out");
    }
}
