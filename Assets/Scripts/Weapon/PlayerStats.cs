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
}
