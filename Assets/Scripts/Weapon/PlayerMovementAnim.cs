using UnityEngine;

public class PlayerMovementAnim : MonoBehaviour
{
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShoot playerShoot;

    void Update()
    {
        bool isMoving = playerMovement.IsMoving();
        bool isRunning = playerMovement.IsRunning();
        bool isShooting = playerShoot != null && playerShoot.IsShooting;
        bool isSwitching = playerShoot != null && playerShoot.IsSwitchingWeapon;

        // 1) Đang đổi súng → chặn tất cả animation di chuyển
        if (isSwitching)
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Run", false);
            return;
        }

        // 2) Đang bắn → chặn tất cả animation di chuyển
        if (isShooting)
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Run", false);
            return;
        }

        // 3) Đang chạy (Shift + di chuyển) → chỉ bật Run
        if (isMoving && isRunning)
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Idle", false);
            armsAnimator.SetBool("Run", true);
        }
        // 4) Đang đi bộ (di chuyển chậm) → chỉ bật Walk
        else if (isMoving)
        {
            armsAnimator.SetBool("Walk", true);
            armsAnimator.SetBool("Run", false);
        }
        // 5) Đứng yên → tắt cả hai
        else
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Run", false);
        }
    }
}
