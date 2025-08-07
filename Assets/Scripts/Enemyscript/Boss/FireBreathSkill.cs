using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Fire Breath Skill", menuName = "ScriptableObject/Skills/Fire Breath")]
public class FireBreathSkill : SkillScriptableObject
{
    public float Duration = 3;
    public float TickRate = 0.5f;
    public float Range = 3;
    public PoolableObject Prefab;

    public override SkillScriptableObject ScaleUpForLevel(ScalingScriptableObject Scaling, int Level)
    {
        FireBreathSkill scaledSkill = CreateInstance<FireBreathSkill>();

        ScaleUpBaseValuesForLevel(scaledSkill, Scaling, Level);
        scaledSkill.Duration = Duration;
        scaledSkill.TickRate = TickRate;
        scaledSkill.Prefab = Prefab;

        return scaledSkill;
    }

    public override bool CanUseSkill(EnemiAI Enemy, Player Player, int Level)
    {
        return base.CanUseSkill(Enemy, Player, Level) && Vector3.Distance(Enemy.transform.position, Player.transform.position) <= Range ;
    }

    public override void UseSkill(EnemiAI Enemy, Player Player)
    {
        base.UseSkill(Enemy, Player);
        //Enemy.StartCoroutine(BreatheFire(Enemy,Player));
    }

    //private IEnumerator BreatheFire(EnemiAI Enemy, Player Player)
    //{
    //    Enemy.Animator.SetBool(Enemy)
    //}
}
