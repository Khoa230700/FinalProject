using UnityEngine;
using UnityEngine.AI;

public class ReviePlayer : MonoBehaviour
{
    public NavMeshAgent agent;
    public PlayerHealthSystem player;

    private bool isReviving = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindAnyObjectByType<PlayerHealthSystem>();
    }

    private void Update()
    {
        if (player == null || !player.IsDown || isReviving) return;

        // Bot đi tới vị trí player
        agent.SetDestination(player.transform.position);

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance < 2f)
        {
            StartCoroutine(RevivePlayer());
        }
    }

    private System.Collections.IEnumerator RevivePlayer()
    {
        isReviving = true;
        agent.isStopped = true; // đứng lại khi revive

        Debug.Log("Bot is reviving the player...");
        yield return new WaitForSeconds(3f);
        player.Revive();

        agent.isStopped = false;
        isReviving = false;
    }
}
