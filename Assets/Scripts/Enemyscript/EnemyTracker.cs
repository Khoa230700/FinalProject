using UnityEngine;
using System;

public class EnemyTracker : MonoBehaviour
{
    public Action OnDeath;

    public void Die()
    {
        OnDeath?.Invoke();
        gameObject.SetActive(false);
    }
}
