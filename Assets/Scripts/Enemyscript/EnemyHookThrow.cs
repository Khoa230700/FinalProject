using UnityEngine;
using System.Collections;

public class EnemyHookThrow : MonoBehaviour
{
    public GameObject hookPrefab;
    public Transform firePoint;
    public float pullSpeed = 10f;

    public void ThrowHook(Transform player)
    {
        GameObject hook = Instantiate(hookPrefab, firePoint.position, firePoint.rotation);
        hook.GetComponent<Hook>().Init(player, this);
    }

    public void StartPull(Transform player)
    {
        StartCoroutine(PullPlayer(player));
    }

    private IEnumerator PullPlayer(Transform player)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        playerRb.isKinematic = true;  // Optional: disable physics during pull

        while (Vector3.Distance(player.position, transform.position) > 1.5f)
        {
            player.position = Vector3.MoveTowards(player.position, transform.position, pullSpeed * Time.deltaTime);
            yield return null;
        }

        playerRb.isKinematic = false;  // Re-enable physics
    }
}
