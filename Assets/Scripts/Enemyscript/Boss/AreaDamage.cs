using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AreaDamage : MonoBehaviour
{
    public int Damage;
    public float TickRate;
    private IDamageable Damageable;

    private void OnTriggerExit(Collider other)
    {
        if (Damageable == null)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null )
            {
                Damageable = null;
            }
        }
    }

    private IEnumerator DealDamage()
    {
        WaitForSeconds Wait = new WaitForSeconds(TickRate);

        while (Damageable != null)
        {
            Damageable.TakeDamage(Damage);
            yield return Wait;
        }
    }

    private void OnDisable()
    {
        Damageable = null;
    }
}
