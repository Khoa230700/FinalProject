using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShopUpgradeBarUI : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject upgradeItemPrefab; // Prefab cho mỗi level indicator

    [Header("Settings")]
    public Color activeColor = Color.yellow;
    public Color inactiveColor = Color.gray;

    private List<GameObject> levelItems = new List<GameObject>();
    private Transform parentTransform;

    private void Awake()
    {
        parentTransform = transform;
    }

    public void SetupUpgradeBar(int currentLevel, int maxLevel)
    {
        foreach (Transform child in parentTransform)
        {
            DestroyImmediate(child.gameObject);
        }

        ClearItems();
        CreateLevelItems(maxLevel);
        UpdateUI(currentLevel);
    }

    private void ClearItems()
    {
        foreach (var item in levelItems)
        {
            DestroyImmediate(item);
        }
        levelItems.Clear();
    }

    private void CreateLevelItems(int maxLevel)
    {
        // Create level indicators
        for (int i = 0; i < maxLevel; i++)
        {
            GameObject item = Instantiate(upgradeItemPrefab, parentTransform);

            levelItems.Add(item);
        }
    }

    private void UpdateUI(int currentLevel)
    {
        for (int i = 0; i < levelItems.Count; i++)
        {
            if (levelItems[i] == null) continue;

            // Level 0 = base level, so level 1 = first upgrade
            bool isActive = i < currentLevel;

            // Update color based on state
            Image image = levelItems[i].GetComponent<Image>();
            if (image != null)
            {
                image.color = isActive ? activeColor : inactiveColor;
            }
        }
    }

    public void UpdateLevel(int newLevel)
    {
        UpdateUI(newLevel);
    }

    public void PlayAnimation(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelItems.Count)
        {
            // Add upgrade animation here (scale, glow, etc.)
            var item = levelItems[levelIndex];
            if (item != null)
            {
                // Simple scale animation
                DOTween.To(() => item.transform.localScale, x => item.transform.localScale = x, Vector3.one * 1.2f, 0.2f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        DOTween.To(() => item.transform.localScale, x => item.transform.localScale = x, Vector3.one, 0.1f);
                    });
            }
        }
    }
}
