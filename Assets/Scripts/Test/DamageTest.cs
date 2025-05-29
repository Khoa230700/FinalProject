using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTest : MonoBehaviour
{
    public Health targetHealth;
    public Transform firePoint;

    private void Start()
    {
        firePoint ??= transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Vector3 hitPoint = firePoint.position;
            targetHealth.TakeDamage(30f, 0f, hitPoint);
        }
    }
}

