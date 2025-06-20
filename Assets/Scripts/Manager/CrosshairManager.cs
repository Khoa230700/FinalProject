using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    
    [Header("Settings")]
    private float unitScale = 10f;
    [SerializeField] private bool applyUsePunch = true;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject staticCrosshair;
    [SerializeField] private DynamicCrosshair dynamicCrosshair;

    private CrosshairData currentCrosshair;


    public void SetCrosshairData(CrosshairData data)
    {
        currentCrosshair = data;

        staticCrosshair.SetActive(data.type == CrosshairType.Static);
        dynamicCrosshair.gameObject.SetActive(data.type == CrosshairType.Dynamic);

        if (data.type == CrosshairType.Dynamic)
        {
            float startDistance = data.dynamicCrosshair.idleScale * unitScale;
            dynamicCrosshair.SetDistance(startDistance);
        }
    }

    private void Update()
    {
        if (currentCrosshair == null) return;

        bool onEntity = false;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject.tag == "Enemy")
                onEntity = true;
        }

        if (currentCrosshair.type == CrosshairType.Dynamic && dynamicCrosshair != null)
        {
            UpdateDynamicCrosshair(onEntity);
            if (applyUsePunch) ApplyUsePunch();
        }
        else if (currentCrosshair.type == CrosshairType.Static && staticCrosshair != null)
        {
            staticCrosshair.SetActive(true);
            var image = staticCrosshair.GetComponent<Image>();
            if (image != null)
            {
                image.color = onEntity
                    ? currentCrosshair.onEntityColor
                    : currentCrosshair.normalColor;
            }
        }
    }

    private void UpdateDynamicCrosshair(bool onEntity)
    {
        var dynamic = currentCrosshair.dynamicCrosshair;

        // Color change
        dynamicCrosshair.SetColor(onEntity ? currentCrosshair.onEntityColor : currentCrosshair.normalColor);

        // Distance based on player state
        float targetDistance = dynamic.idleScale;

        if (!playerMovement.IsGrounded())
            targetDistance = dynamic.jumpScale;
        else if (playerMovement.IsRunning())
            targetDistance = dynamic.runScale;
        else if (playerMovement.IsMoving())
            targetDistance = dynamic.moveScale;

        float current = dynamicCrosshair.distance;
        float moveSpeed = dynamic.moveSpeed;

        float newDistance = Mathf.MoveTowards(current, targetDistance * unitScale, Time.deltaTime * moveSpeed * unitScale);
        dynamicCrosshair.SetDistance(newDistance);
    }

    private void ApplyUsePunch()
    {
        if (currentCrosshair == null || currentCrosshair.type != CrosshairType.Dynamic || !playerShoot.IsShooting)
            return;

        var dynamic = currentCrosshair.dynamicCrosshair;

        float targetDistance = dynamic.idleScale;

        if (!playerMovement.IsGrounded())
            targetDistance = dynamic.jumpScale;
        else if (playerMovement.IsRunning())
            targetDistance = dynamic.runScale;
        else if (playerMovement.IsMoving())
            targetDistance = dynamic.moveScale;

        // Punch effect
        float target = targetDistance * dynamic.punchSize * unitScale;

        float smoothed = Mathf.Lerp(dynamicCrosshair.distance, target, Time.deltaTime * 20f);
        dynamicCrosshair.SetDistance(smoothed);
    }
}
