using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ShopPoint : MonoBehaviour
{
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private GameObject shopNotification;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private float pathInterval = 1f;

    private bool playerInRange = false;
    private Transform player;

    void OnEnable()
    {
        StartCoroutine(PathLoop());
    }

    private void Start()
    {
        if (shopUI == null)
            shopUI = FindAnyObjectByType<ShopUI>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            shopUI.Show();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            shopNotification.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            shopNotification.SetActive(false);

            if (shopUI.IsOpen) shopUI.Hide();
        }
    }

    private IEnumerator PathLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(pathInterval);
            ActivePath();
        }
    }

    private void ActivePath()
    {
        if (player == null) return;

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(player.position, transform.position, NavMesh.AllAreas, path))
        {
            if (path.corners.Length > 1)
            {
                StartCoroutine(MovePathArrow(path.corners));
            }
        }
    }

    private IEnumerator MovePathArrow(Vector3[] waypoints)
    {
        GameObject arrow = Instantiate(arrowPrefab, waypoints[0], Quaternion.identity);

        int index = 1;
        while (arrow != null && index < waypoints.Length)
        {
            Vector3 targetPos = waypoints[index] + Vector3.up * 1f;

            while (Vector3.Distance(arrow.transform.position, targetPos) > 0.1f)
            {
                Vector3 dir = (targetPos - arrow.transform.position).normalized;
                arrow.transform.position += dir * arrowSpeed * Time.deltaTime;
                arrow.transform.forward = dir;
                yield return null;
            }

            index++;
        }

        if (arrow != null)
            Destroy(arrow, 0.5f);
    }
}
