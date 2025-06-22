using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float dynamicScale = 10f;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject runCrosshair;
    [SerializeField] private GameObject staticCrosshair;
    [SerializeField] private DynamicCrosshair dynamicCrosshair;

    private PlayerShoot playerShoot;
    private CrosshairData currentCrosshair;

    private void Update()
    {
        if (currentCrosshair == null) return;

        UpdateRunCrosshair();

        bool onLook = IsLookingAtEnemy();

        switch (currentCrosshair.type)
        {
            case CrosshairType.Dynamic:
                UpdateDynamicCrosshair(onLook);
                break;

            case CrosshairType.Static:
                UpdateStaticCrosshair(onLook);
                break;
        }
    }

    private void UpdateRunCrosshair()
    {
        if (playerMovement.IsRunning())
        {
            runCrosshair.SetActive(true);
            staticCrosshair.SetActive(false);
            dynamicCrosshair.gameObject.SetActive(false);
        }
        else
        {
            runCrosshair.SetActive(false);
            staticCrosshair.SetActive(currentCrosshair.type == CrosshairType.Static);
            dynamicCrosshair.gameObject.SetActive(currentCrosshair.type == CrosshairType.Dynamic);
        }

        return;
    }

    private void UpdateStaticCrosshair(bool onLook)
    {
        if (!staticCrosshair) return;

        staticCrosshair.SetActive(true);
        staticCrosshair.GetComponent<Image>().color = onLook
                ? currentCrosshair.enemyColor
                : currentCrosshair.normalColor;
    }

    private void UpdateDynamicCrosshair(bool onEntity)
    {
        if (!dynamicCrosshair) return;

        var dynamic = currentCrosshair.dynamicCrosshair;

        float targetScale = GetScale(dynamic);
        float targetDistance = targetScale * dynamicScale;
        float newDistance = Mathf.MoveTowards(dynamicCrosshair.distance, targetDistance, Time.deltaTime * dynamic.moveSpeed * dynamicScale);

        dynamicCrosshair.SetColor(onEntity ? currentCrosshair.enemyColor : currentCrosshair.normalColor);
        dynamicCrosshair.SetDistance(newDistance);
        BounceScale();
    }

    private void BounceScale()
    {
        if (currentCrosshair == null || currentCrosshair.type != CrosshairType.Dynamic || !playerShoot.IsShooting)
            return;

        var dynamic = currentCrosshair.dynamicCrosshair;

        float targetScale = GetScale(dynamic);
        float targetDistance = targetScale * dynamic.bounceSize * dynamicScale;
        float smoothed = Mathf.Lerp(dynamicCrosshair.distance, targetDistance, Time.deltaTime * 20f);
        
        dynamicCrosshair.SetDistance(smoothed);
    }

    private float GetScale(DynamicCrosshairSettings dynamic)
    {
        if (!playerMovement.IsGrounded())
            return dynamic.jumpScale;

        if (playerMovement.IsMoving())
            return dynamic.moveScale;

        return dynamic.idleScale;
    }

    private bool IsLookingAtEnemy()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 100f))
        {
            return hit.collider.CompareTag("Enemy");
        }
        return false;
    }

    public void SetCrosshairData(CrosshairData data)
    {
        currentCrosshair = data;

        staticCrosshair.SetActive(data.type == CrosshairType.Static);
        var image = staticCrosshair.GetComponent<Image>();
        image.color = data.normalColor;
        image.sprite = data.staticCrosshair.sprite;

        dynamicCrosshair.gameObject.SetActive(data.type == CrosshairType.Dynamic);
    }

    public void SetPlayerShoot(PlayerShoot shoot)
    {
        playerShoot = shoot;
    }
}
