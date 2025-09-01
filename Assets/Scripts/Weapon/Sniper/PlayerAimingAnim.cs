using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAimingAnim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private Animator armsAnimator;

    [Header("Settings")]
    public string aimInput = "Fire2"; // giữ chuột phải để ngắm

    private bool isAiming = false;
    private bool wasAimingLastFrame = false;

    void Update()
    {
        if (playerMovement == null || playerShoot == null || armsAnimator == null)
            return;

        isAiming = Input.GetButton(aimInput);

        if (!isAiming)
        {
            if (wasAimingLastFrame)
            {
                ResetAllAimingTriggers();
                armsAnimator.SetTrigger("Idle"); // Chuyển về Idle nếu có transition
                wasAimingLastFrame = false;
            }
            return;
        }

        // Đang ngắm
        if (playerShoot.IsShooting)
        {
            SetAimingTrigger("AimingShot");
        }
        else if (playerMovement.IsMoving())
        {
            SetAimingTrigger("AimingWalk");
        }
        else
        {
            SetAimingTrigger("AimingIdle");
        }

        wasAimingLastFrame = true;
    }

    private void SetAimingTrigger(string triggerName)
    {
        ResetAllAimingTriggers();
        armsAnimator.SetTrigger(triggerName);
    }

    private void ResetAllAimingTriggers()
    {
        armsAnimator.ResetTrigger("AimingIdle");
        armsAnimator.ResetTrigger("AimingWalk");
        armsAnimator.ResetTrigger("AimingShot");
    }
}
