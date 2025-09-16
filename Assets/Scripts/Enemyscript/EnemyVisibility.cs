using UnityEngine;

public class EnemyVisibility : MonoBehaviour
{
    public Transform player; 
    public float revealDistance = 5f; // Distance 

    private Renderer[] renderers;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool shouldBeVisible = distance <= revealDistance;

        foreach (Renderer rend in renderers)
        {
            rend.enabled = shouldBeVisible;
        }
    }
}
