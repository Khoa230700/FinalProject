using UnityEngine;

public class EnemyFireBreath : MonoBehaviour
{
    public float channelTime = 3f;
    public float damagePerSecond = 10f;
    public ParticleSystem fireFX;
    public Collider fireDamageArea;

    //
    public Transform player;
    public float triggerRange = 10f;
    public float fireCooldown = 5f;

    private float nextFireTime = 0f;


    private bool isChanneling = false;

    private void Start()
    {
        fireFX.Stop();
        fireDamageArea.enabled = false;
    }


    void Update()
    {
        if (isChanneling || Time.time < nextFireTime) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= triggerRange && HasLineOfSight())
        {
            StartFireBreath();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    public void StartFireBreath()
    {
        if (isChanneling) return;

        isChanneling = true;
        fireFX.Play();
        fireDamageArea.enabled = true;

        // Stop channeling after some time
        Invoke(nameof(StopFireBreath), channelTime);
    }

    void StopFireBreath()
    {
        fireFX.Stop();
        fireDamageArea.enabled = false;
        isChanneling = false;
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (fireDamageArea.enabled && other.CompareTag("Player"))
    //    {
    //        Debug.Log("Player inside fire area");
    //        // Apply damage over time
    //        var health = other.GetComponent<testPlayerHealth>();
    //        if (health != null)
    //        {
    //            //health.TakeDamage(damagePerSecond * Time.deltaTime);
    //            health.TakeDamage(10);
    //        }
    //    }
    //}

    bool HasLineOfSight()
    {
        Ray ray = new Ray(transform.position + Vector3.up, (player.position - transform.position).normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, triggerRange))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }
}
