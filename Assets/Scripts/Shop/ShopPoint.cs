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
    private Coroutine pathCoroutine;

    private void OnEnable()
    {
        pathCoroutine = StartCoroutine(PathLoop());
    }

    private void OnDisable()
    {
        if (shopNotification != null)
        shopNotification.SetActive(false);
    }

    private void Start()
    {
        player = SelectorSpawner.Instance.Player.transform;

        if (shopUI == null)
            shopUI = FindAnyObjectByType<ShopUI>();
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
            if(pathCoroutine != null) StopCoroutine(pathCoroutine);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            shopNotification.SetActive(false);

            if (shopUI.isOpen) shopUI.Hide();
            if(pathCoroutine != null) pathCoroutine = StartCoroutine(PathLoop());
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
        Vector3 spawnPos = player.position + player.forward * 4f + Vector3.up * 1.5f;
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        int index = 1;
        while (arrow != null && index < waypoints.Length)
        {
            Vector3 targetPos = waypoints[index] + Vector3.up;

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
            Destroy(arrow, 5f);
    }
}
