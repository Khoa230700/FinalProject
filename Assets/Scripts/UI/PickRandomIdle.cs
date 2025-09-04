using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickRandomIdle : StateMachineBehaviour
{
    public int idleCount = 2;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat("IdleIndex", (float)Random.Range(0, idleCount));
    }
}