using UnityEngine;
using static Hitbox;

public class Hitbox : MonoBehaviour
{
    public enum HitboxType
    {
        Body,
        Head,
        WeakSpot
    }
    [Header("Loại vùng trúng đạn")]
    public HitboxType hitboxType = HitboxType.Body;

    [Tooltip("Reference đến HealthSystem của enemy chính")]
    public BaseHealthSystem ownerHealthSystem;
}
