using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public enum HixboxType
    {
        Body,
        Head,
        WeakSpot
    }

    [Tooltip("Reference đến HealthSystem của enemy chính")]
    public BaseHealthSystem ownerHealthSystem;
}
