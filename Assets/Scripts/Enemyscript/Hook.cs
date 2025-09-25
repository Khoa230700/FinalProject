using UnityEngine;

public class Hook : MonoBehaviour
{
    public float speed = 900f;
    private Transform target;
    private EnemyHookThrow enemy;

    private LineRenderer lineRenderer;

    private bool isFlying = true;

    public void Init(Transform targetPlayer, EnemyHookThrow enemyRef)
    {
        target = targetPlayer;
        enemy = enemyRef;
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (isFlying)
            transform.position += transform.forward * speed * Time.deltaTime;
        // update rope position
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, enemy.transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            Debug.Log("hook hit");
            isFlying = false;
            enemy.StartPull(target);  // Start pulling the player
            Destroy(gameObject);     // Remove the hook
        }
    }
}
