using UnityEngine;

public class EnemyHookThrow : MonoBehaviour
{
    public GameObject hookPrefab;
    public Transform firePoint;
    public float pullSpeed = 10f;
    public float aimHeightOffset; // ngắm tầm ngực/đầu

    public void ThrowHook(Transform player)
    {
        Vector3 aimPoint = GetAimPoint(player);
        Vector3 dir = (aimPoint - firePoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        var go = Instantiate(hookPrefab, firePoint.position, rot);
        go.GetComponent<Hook>().Init(player, this, firePoint, dir); // pass firePoint + dir
    }

    public void StartPull(Transform player) => StartCoroutine(PullPlayer(player));

    private System.Collections.IEnumerator PullPlayer(Transform player)
    {
        var rb = player.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        while (Vector3.Distance(player.position, transform.position) > 1.5f)
        {
            player.position = Vector3.MoveTowards(player.position, transform.position, pullSpeed * Time.deltaTime);
            yield return null;
        }

        if (rb) rb.isKinematic = false;
    }

    private Vector3 GetAimPoint(Transform t)
    {
        if (t && t.TryGetComponent<CharacterController>(out var cc))
            return t.position + Vector3.up * (cc.height * 0.5f); // giữa thân
        return t ? t.position + Vector3.up * aimHeightOffset : firePoint.position;
    }
}
