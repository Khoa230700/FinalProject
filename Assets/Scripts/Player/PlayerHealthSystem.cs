using UnityEngine;

public class PlayerHealthSystem : BaseHealthSystem
{
    public enum PlayerCLass
    {
        Sniper, 
        Soldier,
        Tanker
    }

    [Header("Player Class Settings")]
    [SerializeField] private PlayerCLass playerCLass;

    [Header("Shield Settings")]
    private float currentShield;
    private float maxShield;

    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f;
    private float currentMoveSpeed;

    public event System.Action<float, float> OnShieldChanged;

    protected override void Start()
    {
        
        base.Start();
        currentShield = 0;
        
    }
    private void SetStatsByClass()
    {
        maxHealth = 100f;
        switch (playerCLass)
        {
            case PlayerCLass.Sniper:
                maxShield = 20f;
                baseMoveSpeed = 6f;
                break;
            case PlayerCLass.Soldier:
                maxShield = 40f;
                baseMoveSpeed = 5f;
                break;
            case PlayerCLass.Tanker:
                maxShield = 60f;
                baseMoveSpeed = 4f;
                break;
        }
    }
    public float GetCurrentMoveSpeed(bool isHeavyWeapon)
    {
        return isHeavyWeapon ? baseMoveSpeed * 0.8f : baseMoveSpeed;
    }
    public override void TakeDamage(float damage)
    {
        float remainingDamage = damage;
        if(currentShield > 0)
        {
            float shieldAbsorb = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldAbsorb;
            remainingDamage -= shieldAbsorb;
            BroadcastShield();
        }
        base.TakeDamage(remainingDamage);
    }
    public void AddShield(float amount)
    {
        currentShield += amount;
        currentShield = Mathf.Clamp(currentShield, 0f, maxShield);
        BroadcastShield();
    }

    private void BroadcastShield()
    {
        OnShieldChanged?.Invoke(currentShield, maxShield);
    }
    protected override void Die()
    {
        Debug.Log("Player Dead!");
        EvenBus.PlayerDied();
        GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
    }
}
