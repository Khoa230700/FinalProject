using UnityEngine;

public class CrosshairBloomUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform topArm, bottomArm, leftArm, rightArm; // 4 nhánh
    public PlayerMovement movement;     // để biết đang di chuyển (nếu có)
    public PlayerShoot shooter;         // để nghe sự kiện OnShotFired

    [Header("Settings")]
    public float baseGap = 8f;          // khoảng cách cơ bản
    public float maxExtraGap = 18f;     // nở tối đa thêm
    public float fireKick = 8f;         // mỗi lần bắn nở thêm bao nhiêu
    public float moveFactor = 0.6f;     // đang di chuyển nở thêm bao nhiêu (tỷ lệ so với maxExtraGap)
    public float relaxSpeed = 16f;      // tốc độ thu nhỏ lại

    float extraGap;                     // khoảng nở hiện tại

    void OnEnable()
    {
        if (!shooter) shooter = FindAnyObjectByType<PlayerShoot>();
        if (shooter != null)
            shooter.OnShotFired += OnShot;
    }

    void OnDisable()
    {
        if (shooter != null)
            shooter.OnShotFired -= OnShot;
    }

    void Update()
    {
        float moveAdd = 0f;
        // FIX: IsMoving là METHOD -> phải gọi ()
        if (movement != null && movement.IsMoving())
            moveAdd = maxExtraGap * moveFactor;

        extraGap = Mathf.MoveTowards(extraGap, 0f, relaxSpeed * Time.deltaTime);

        float gap = baseGap + Mathf.Min(maxExtraGap, extraGap + moveAdd);
        ApplyGap(gap);
    }

    void ApplyGap(float g)
    {
        if (topArm) topArm.anchoredPosition = new Vector2(0f, g);
        if (bottomArm) bottomArm.anchoredPosition = new Vector2(0f, -g);
        if (leftArm) leftArm.anchoredPosition = new Vector2(-g, 0f);
        if (rightArm) rightArm.anchoredPosition = new Vector2(g, 0f);
    }

    void OnShot()
    {
        extraGap = Mathf.Min(maxExtraGap, extraGap + fireKick);
    }
}
