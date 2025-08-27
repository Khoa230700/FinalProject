using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Player/Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Range(0f, 1f)]
    public float airControlMultiplier = 0.5f; // điều khiển trên không

    // >>> NEW: Vitals <<<
    [Header("Vitals")]
    public float maxHealth = 100f;
    public float maxShield = 50f;

    [Tooltip("Hồi shield mỗi giây (0 = tắt hồi)")]
    public float shieldRegenPerSecond = 15f;

    [Tooltip("Trễ hồi shield sau khi bị trúng đòn")]
    public float shieldRegenDelay = 3f;
}
