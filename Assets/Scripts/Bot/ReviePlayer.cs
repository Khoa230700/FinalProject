using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ReviePlayer : MonoBehaviour
{
    public NavMeshAgent agent;
    public PlayerHealthTest player;
    public L4DBotController controller; // script AI gốc

    private bool isReviving = false;
    private float defaultStoppingDistance = 8f; // khoảng cách mặc định
    private float reviveStoppingDistance = 1.5f; // đứng gần khi revive

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindAnyObjectByType<PlayerHealthTest>();

        // set mặc định khi start
        agent.stoppingDistance = defaultStoppingDistance;
    }

    private void Update()
    {
        // Nếu player còn sống hoặc bot đang revive -> bỏ qua
        if (player == null || !player.IsDown || isReviving)
            return;

        // Tắt hành vi khác để ưu tiên revive
        if (controller != null && controller.enabled)
            controller.enabled = false;

        // Đặt stoppingDistance nhỏ để tiến sát player
        agent.stoppingDistance = reviveStoppingDistance;

        // Bot đi tới Player
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= 4f) // chỉ khi đủ gần mới revive
        {
            StartCoroutine(RevivePlayer());
        }
    }

    private IEnumerator RevivePlayer()
    {
        isReviving = true;

        agent.isStopped = true; // đứng yên khi revive
        Debug.Log("🤖 Bot is reviving the player...");

        yield return new WaitForSeconds(3f); // giả lập thời gian revive

        player.Revive(); // hồi sinh player
        Debug.Log("✅ Player revived!");

        // Reset lại trạng thái sau khi revive
        agent.isStopped = false;
        isReviving = false;

        // Trả về khoảng cách mặc định
        agent.stoppingDistance = defaultStoppingDistance;

        // Bật lại controller sau khi revive
        if (controller != null)
            controller.enabled = true;
    }
}
