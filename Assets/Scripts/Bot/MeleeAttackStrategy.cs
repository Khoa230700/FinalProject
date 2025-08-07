using UnityEngine;

public class MeleeAttackStrategy : IAttackStrategy
{
    private Animator animator;
    private float cooldown;
    private float lastAttackTime;

    public MeleeAttackStrategy(Animator animator, float cooldown)
    {
        this.animator = animator;
        this.cooldown = cooldown;
    }

    public bool CanAttack()
    {
        return Time.time - lastAttackTime > cooldown;
    }

    public void Attack(GameObject target)
    {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
    }
}
