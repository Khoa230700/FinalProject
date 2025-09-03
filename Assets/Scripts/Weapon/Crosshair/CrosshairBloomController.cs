using UnityEngine;

public class CrosshairBloomController : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform root;            // CrosshairRoot
    public RectTransform topBar, bottomBar, leftBar, rightBar;
    public PlayerMovement playerMovement; // để biết Move/Run
    public PlayerShoot currentGun;        // để biết IsShooting, spread
    public CSGOScope scope;               // để biết IsScoped

    [Header("Layout")]
    public float baseGap = 12f;           // khoảng cách tâm -> đầu mỗi nhánh khi đứng yên
    public float barLength = 12f;         // chiều dài mỗi nhánh (sizeDelta.y hoặc x)
    public float barThickness = 2f;       // độ dày nhánh (sizeDelta.x hoặc y)

    [Header("Bloom Additions")]
    public float moveExtra = 4f;          // thêm khi đi bộ
    public float runExtra = 8f;          // thêm khi chạy
    public float shootExtra = 6f;          // thêm khi bắn
    public float scopeMultiplier = 0.2f;  // scoped → gap *= multiplier (thu nhỏ)
    public float aimingDampen = 0.6f;   // nếu có “ngắm không scope” có thể giảm bloom

    [Header("Smoothing")]
    [Range(0f, 30f)] public float gapLerp = 20f;

    float _curGap;

    void Start()
    {
        _curGap = baseGap;
        ApplyBarSizes();
    }

    void ApplyBarSizes()
    {
        if (topBar) topBar.sizeDelta = new Vector2(barThickness, barLength);
        if (bottomBar) bottomBar.sizeDelta = new Vector2(barThickness, barLength);
        if (leftBar) leftBar.sizeDelta = new Vector2(barLength, barThickness);
        if (rightBar) rightBar.sizeDelta = new Vector2(barLength, barThickness);
    }

    void Update()
    {
        float targetGap = baseGap;

        // Move/Run
        bool moving = playerMovement && playerMovement.IsMoving();
        bool running = playerMovement && playerMovement.IsRunning();
        if (moving) targetGap += moveExtra;
        if (running) targetGap += runExtra;

        // Shoot
        if (currentGun && currentGun.IsShooting) targetGap += shootExtra;

        // Nếu có spreadAngle của súng, có thể cộng thêm theo tỷ lệ:
        // (tuỳ bạn) float spreadFactor = currentGun ? Mathf.InverseLerp(0f, 8f, currentGun.gunData.spreadAngle) : 0f;
        // targetGap += spreadFactor * 6f;

        // Scoped → thu nhỏ
        bool scoped = scope && scope.IsScoped;
        if (scoped) targetGap *= scopeMultiplier;

        // Lerp mượt
        _curGap = Mathf.Lerp(_curGap, targetGap, Time.deltaTime * gapLerp);

        // Đặt vị trí bốn nhánh đối xứng quanh gốc
        if (topBar) topBar.anchoredPosition = new Vector2(0f, _curGap + topBar.sizeDelta.y * 0.5f);
        if (bottomBar) bottomBar.anchoredPosition = new Vector2(0f, -(_curGap + bottomBar.sizeDelta.y * 0.5f));
        if (leftBar) leftBar.anchoredPosition = new Vector2(-(_curGap + leftBar.sizeDelta.x * 0.5f), 0f);
        if (rightBar) rightBar.anchoredPosition = new Vector2(_curGap + rightBar.sizeDelta.x * 0.5f, 0f);
    }
}
