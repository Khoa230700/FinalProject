using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerClassNameUI : MonoBehaviour
{
    [SerializeField] private Image avatarImage;
    [SerializeField] private Image classImage;
    [SerializeField] private TMP_Text playerName;

    public void UpdateUI(PlayerSelectorUI playerSelectorUI)
    {
        avatarImage.sprite = playerSelectorUI.avatar;
        classImage.sprite = playerSelectorUI.classAvatar;
        playerName.text = playerSelectorUI.name;
    }
}
