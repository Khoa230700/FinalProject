using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SettingButtonUI : MonoBehaviour, IPointerEnterHandler
{
    public string title;
    [TextArea] public string description;
    public Sprite iconSprite;
    public Sprite iconBackground;

    [Header("References")]
    public Image detailIcon;
    public Image detailBackground;
    public TextMeshProUGUI detailTitle;
    public TextMeshProUGUI detailDescription;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (detailIcon == null || detailBackground == null
            || detailTitle == null || detailDescription == null)
        {
            return;
        }

        detailIcon.gameObject.SetActive(true);
        detailBackground.gameObject.SetActive(true);
        detailIcon.sprite = iconSprite;
        detailBackground.sprite = iconBackground;

        detailTitle.text = title;
        detailDescription.text = description;
    }
}
