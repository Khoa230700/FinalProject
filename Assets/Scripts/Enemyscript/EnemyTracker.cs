using UnityEngine;
using System;

public class EnemyTracker : MonoBehaviour
{
    public Action OnDeath;
    public Action OnDeath2;
    private QuestTracker questTracker;

    private void Start()
    {
        questTracker = GetComponent<QuestTracker>();
        OnDeath2 += Die;
    }

    public void Die()
    {
        OnDeath?.Invoke();
        questTracker.OnKilled();
        Debug.Log($"{gameObject.name} has died.");
        gameObject.SetActive(false);
    }
}