using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerMovementAnim : MonoBehaviour
{
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private WeaponSwitcher weaponSwitcher; // <-- thay vì PlayerShoot

    void Update()
    {
        if (armsAnimator == null || playerMovement == null || weaponSwitcher == null)
            return;

        var current = weaponSwitcher.Current;

        // Nếu là súng, kiểm tra scoped & shooting như cũ
        bool isAiming = false;
        bool isShooting = false;
        bool isSwitching = current != null && current.IsSwitchingWeapon;

        if (current is PlayerShoot ps)
        {
            isAiming = (ps.csgoScope != null && ps.csgoScope.IsScoped);
            isShooting = ps.IsShooting;
        }

        // 1) Nếu đang scoped thì bỏ qua mọi animation chạy
        if (isAiming)
        {
            SetMoveBools(false, false);
            return;
        }

        // 2) Nếu đang bắn (đối với súng) hoặc đang chuyển vũ khí thì cũng bỏ qua
        if (isShooting || isSwitching)
        {
            SetMoveBools(false, false);
            return;
        }

        // 3) Tính trạng thái di chuyển & chạy
        bool isMoving = playerMovement.IsMoving();
        bool isRunning = playerMovement.IsRunning();

        // 4) Đặt animation flag
        if (isMoving && isRunning)
            SetMoveBools(false, true);
        else if (isMoving)
            SetMoveBools(true, false);
        else
            SetMoveBools(false, false);
    }

    private void SetMoveBools(bool walk, bool run)
    {
        armsAnimator.SetBool("Walk", walk);
        armsAnimator.SetBool("Run", run);
    }
}
