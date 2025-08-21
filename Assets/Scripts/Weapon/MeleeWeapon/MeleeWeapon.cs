using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeWeapon : MonoBehaviour, IWeapon
{
    [Header("Data")]
    public MeleeData data;
    [Range(0, 20)] public int level = 0;   // 0 = base

    [Header("Refs")]
    public Animator armsAnimator;
    public Transform swingOrigin;          // đặt ở đầu lưỡi rìu/dao
    public Camera aimCamera;               // kéo thả Main Camera
    public WeaponUI weaponUI;

    [Header("Debug Draw")]
    public bool drawGizmos = true;         // vẽ vùng đánh trong Scene view
    public bool drawDuringPlay = true;     // vẽ tia khi chơi
    [Range(0.01f, 0.1f)] public float drawDuration = 0.05f;

    public bool IsSwitchingWeapon { get; private set; }

    // runtime
    bool canAttack = true;
    bool isHolding = false;

    public void OnSelected(WeaponUI ui)
    {
        weaponUI = ui;
        // weaponUI?.ClearAmmoUI(); // ẩn UI đạn nếu muốn
    }
    public void OnDeselected() { }

    public void StartFiring() { isHolding = true; }
    public void StopFiring() { isHolding = false; }

    public void FireOnce()
    {
        if (!canAttack || data == null || swingOrigin == null) return;
        StartCoroutine(DoSwing());
    }

    IEnumerator DoSwing()
    {
        canAttack = false;

        // Chọn animation
        string anim = (data.animSwings != null && data.animSwings.Length > 0)
            ? data.animSwings[Random.Range(0, data.animSwings.Length)]
            : data.animSwing; // fallback 1 anim
        if (armsAnimator) armsAnimator.Play(anim, 0, 0f);

        if (data.swingSfx) AudioSource.PlayClipAtPoint(data.swingSfx, swingOrigin.position);

        // “Damage window”
        yield return new WaitForSeconds(data.swingDelay);

        float dmg = data.GetDamage(level);
        float range = data.GetRange(level);
        float radius = data.baseRadius;
        int steps = Mathf.Max(1, data.sweepSteps);

        Vector3 dir = (aimCamera ? aimCamera.transform.forward : swingOrigin.forward).normalized;
        Vector3 start = swingOrigin.position + dir * 0.02f; // đẩy nhẹ 2cm để tránh bắt đầu bên trong collider
        float stepLen = range / steps;

        // ===== Khử trùng theo OWNER (mỗi kẻ địch 1 lần/swing) =====
        // key ưu tiên: EnemyM (ownerHealthSystem), fallback: IDamageable, cuối cùng: Transform root
        var hitTargets = new HashSet<object>();

        // A) Bắt mục tiêu đang dính ngay tại gốc (đứng sát người)
        Collider[] buf = new Collider[16];
        int n = Physics.OverlapSphereNonAlloc(
            swingOrigin.position, radius, buf, data.hitMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            TryApplyDamageOnce(buf[i], dmg, swingOrigin.position, hitTargets);
        }

        // B) Quét cung phía trước bằng nhiều SphereCast ngắn
        for (int i = 1; i <= steps; i++)
        {
            Vector3 stepEnd = start + dir * (stepLen * i);

            if (Physics.SphereCast(start, radius, dir, out RaycastHit hit,
                                   stepLen, data.hitMask, QueryTriggerInteraction.Ignore))
            {
                TryApplyDamageOnce(hit.collider, dmg, hit.point, hitTargets);

                if (drawDuringPlay)
                {
                    Debug.DrawLine(start, hit.point, Color.magenta, drawDuration);
                    Debug.DrawRay(hit.point, hit.normal * 0.25f, Color.yellow, drawDuration);
                }
            }

            if (drawDuringPlay) Debug.DrawLine(start, stepEnd, Color.cyan, drawDuration);
            start = stepEnd;
            yield return null; // tạo cảm giác “quét” theo frame
        }

        yield return new WaitForSeconds(Mathf.Max(0f, data.GetCooldown(level)));
        canAttack = true;
    }

    void TryApplyDamageOnce(Collider col, float dmg, Vector3 hitPoint, HashSet<object> hitTargets)
    {
        if (col == null) return;

        // 1) Ưu tiên Hitbox → lấy ownerHealthSystem làm key
        var hb = col.GetComponentInParent<Hitbox>();
        if (hb != null && hb.ownerHealthSystem != null)
        {
            if (hitTargets.Add(hb.ownerHealthSystem)) // chỉ khi chưa hit owner này
            {
                hb.ownerHealthSystem.TakeDamage(dmg);
                hb.OnHit(dmg, hitPoint);
                if (data.hitSfx) AudioSource.PlayClipAtPoint(data.hitSfx, hitPoint);
            }
            return;
        }

        // 2) Fallback: IDamageable trên parent
        var dmgable = col.GetComponentInParent<IDamageable>();
        if (dmgable != null)
        {
            if (hitTargets.Add(dmgable))
            {
                dmgable.TakeDamage(Mathf.RoundToInt(dmg));
                if (data.hitSfx) AudioSource.PlayClipAtPoint(data.hitSfx, hitPoint);
            }
            return;
        }

        // 3) Fallback cuối: gom theo Transform root để lỡ cấu trúc khác thường vẫn không nhân đòn
        var root = col.transform.root;
        if (hitTargets.Add(root))
        {
            // nếu muốn vẫn gọi OnHit effect ở đây, nhưng không biết health system
            // Fx only:
            var hbAny = col.GetComponentInParent<Hitbox>();
            if (hbAny != null) hbAny.OnHit(dmg, hitPoint);
        }
    }

    public Coroutine SwitchOut(MonoBehaviour runner) => runner.StartCoroutine(SwitchOutRoutine());
    IEnumerator SwitchOutRoutine()
    {
        IsSwitchingWeapon = true;
        if (armsAnimator) armsAnimator.SetTrigger(data.animHide);
        yield return new WaitForSeconds(0.3f);
    }

    public Coroutine SwitchIn(MonoBehaviour runner) => runner.StartCoroutine(SwitchInRoutine());
    IEnumerator SwitchInRoutine()
    {
        if (armsAnimator) armsAnimator.SetTrigger(data.animGet);
        yield return new WaitForSeconds(0.3f);
        IsSwitchingWeapon = false;
    }

    // Nếu dùng Animation Event để “đánh trúng” chính xác ở keyframe
    public void OnMeleeHitWindow()
    {
        if (!canAttack && data != null && swingOrigin != null)
            StartCoroutine(DoSwing()); // hoặc tách phần quét ra 1 hàm riêng rồi gọi tại đây
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || swingOrigin == null || data == null) return;

        Vector3 origin = swingOrigin.position;
        Vector3 dir = (aimCamera ? aimCamera.transform.forward : swingOrigin.forward).normalized;

        float radius = data.baseRadius;
        float range = Application.isPlaying ? data.GetRange(level) : data.baseRange;
        int steps = Mathf.Max(1, data.sweepSteps);
        float step = range / steps;

        // Ống quét: các “tiết diện” hình cầu + đoạn nối
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.6f);
        Vector3 start = origin;
        for (int i = 1; i <= steps; i++)
        {
            Vector3 end = origin + dir * (step * i);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, radius);
            start = end;
        }

        // mặt cắt tại gốc + đường hướng
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawWireSphere(origin, radius);
        Debug.DrawRay(origin, dir * range, Color.cyan);
    }
#endif
}
