using UnityEngine;

public class PlayerBeingPull : MonoBehaviour
{
    private bool isBeingPulled = false;
    private Transform pullTarget;
    public float pullSpeed = 10f;

    public void StartPull(Transform enemyTransform)
    {
        pullTarget = enemyTransform;
        isBeingPulled = true;
    }

    void Update()
    {
        if (isBeingPulled && pullTarget != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, pullTarget.position, pullSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, pullTarget.position) < 1f)
            {
                isBeingPulled = false; // Stop pulling
            }
        }
    }
}
