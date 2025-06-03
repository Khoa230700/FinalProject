using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCrosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField]
    [Range(0f, 256f)] private float baseDistance = 32f;
    [SerializeField]
    [Range(0f, 256f)] private float maxDistance = 64f;
    [SerializeField]
    private float changeSpeed = 5f;

    [Header("Crosshair Parts")]
    [SerializeField] private Image top;
    [SerializeField] private Image down;
    [SerializeField] private Image left;
    [SerializeField] private Image right;

    private float currentDistance;
    private PlayerMoveTest playerMoveTest;
    private WeaponTest weaponTest;

    void Start()
    {
        currentDistance = baseDistance;
        playerMoveTest = FindFirstObjectByType<PlayerMoveTest>();
        weaponTest = FindFirstObjectByType<WeaponTest>();
    }

    void Update()
    {
        float targetDistance = baseDistance;

        if (weaponTest.isShooting)
        {
            targetDistance = maxDistance;
        }
        else if (playerMoveTest.isMoving)
        {
            targetDistance = maxDistance * 0.8f;
        }
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, changeSpeed * Time.deltaTime);
        UpdateCrosshairPosition();
    }

    void UpdateCrosshairPosition()
    {
        top.rectTransform.anchoredPosition = new Vector2(0f, currentDistance);
        down.rectTransform.anchoredPosition = new Vector2(0f, -currentDistance);
        left.rectTransform.anchoredPosition = new Vector2(-currentDistance, 0f);
        right.rectTransform.anchoredPosition = new Vector2(currentDistance, 0f);
    }
}
