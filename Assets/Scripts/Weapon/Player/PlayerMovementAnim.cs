// PlayerMovementAnim.cs
using UnityEngine;

public class PlayerMovementAnim : MonoBehaviour
{
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShoot playerShoot;

    void Update()
    {
        // Nếu đang scoped (sniper) thì bỏ qua chạy/chạy nhanh
        bool isAiming = playerShoot != null &&
                        playerShoot.csgoScope != null &&
                        playerShoot.csgoScope.IsScoped;
        if (isAiming)
            return;

        // Nếu đang bắn hoặc đang chuyển súng cũng bỏ qua
        bool isShooting = playerShoot != null && playerShoot.IsShooting;
        bool isSwitching = playerShoot != null && playerShoot.IsSwitchingWeapon;
        if (isShooting || isSwitching)
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Run", false);
            return;
        }

        bool isMoving = playerMovement.IsMoving();
        bool isRunning = playerMovement.IsRunning();

        if (isMoving && isRunning)
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Run", true);
        }
        else if (isMoving)
        {
            armsAnimator.SetBool("Walk", true);
            armsAnimator.SetBool("Run", false);
        }
        else
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Run", false);
        }
    }
    // :contentReference[oaicite:3]{index=3}
}
