using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAimingAnim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private CSGOScope csgoScope; // tùy chọn: ưu tiên đọc IsScoped

    [Header("Settings")]
    public string aimInput = "Fire2"; // giữ chuột phải để ngắm

    private bool isAiming = false;
    private bool wasAimingLastFrame = false;

    // Hash các trigger
    static readonly int T_AimingIdle = Animator.StringToHash("AimingIdle");
    static readonly int T_AimingWalk = Animator.StringToHash("AimingWalk");
    static readonly int T_AimingShot = Animator.StringToHash("AimingShot");
    static readonly int T_Idle = Animator.StringToHash("Idle");

    void Awake()
    {
        if (!armsAnimator) armsAnimator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        // nghe sự kiện bắn để đảm bảo animation AimingShot (tùy chọn)
        if (playerShoot != null) playerShoot.OnShotFired += OnShotFired;
    }

    void OnDisable()
    {
        if (playerShoot != null) playerShoot.OnShotFired -= OnShotFired;
    }

    void Update()
    {
        if (!armsAnimator || !playerShoot || !playerMovement) return;

        // Ưu tiên trạng thái từ CSGOScope nếu có
        bool aimingNow = csgoScope ? csgoScope.IsScoped : Input.GetButton(aimInput);
        isAiming = aimingNow;

        if (!isAiming)
        {
            if (wasAimingLastFrame)
            {
                ResetAimingTriggers();
                armsAnimator.SetTrigger(T_Idle);
                wasAimingLastFrame = false;
            }
            return;
        }

        // Đang ngắm
        if (playerShoot.IsShooting)
        {
            SetAimingTrigger(T_AimingShot);
        }
        else if (playerMovement.IsMoving()) // nếu dùng property -> đổi thành ".IsMoving"
        {
            SetAimingTrigger(T_AimingWalk);
        }
        else
        {
            SetAimingTrigger(T_AimingIdle);
        }

        wasAimingLastFrame = true;
    }

    private void OnShotFired()
    {
        if (!isAiming) return; // chỉ ưu tiên shot anim khi đang ngắm
        SetAimingTrigger(T_AimingShot);
    }

    private void SetAimingTrigger(int hash)
    {
        ResetAimingTriggers();
        armsAnimator.SetTrigger(hash);
    }

    private void ResetAimingTriggers()
    {
        armsAnimator.ResetTrigger(T_AimingIdle);
        armsAnimator.ResetTrigger(T_AimingWalk);
        armsAnimator.ResetTrigger(T_AimingShot);
        armsAnimator.ResetTrigger(T_Idle);
    }
}
