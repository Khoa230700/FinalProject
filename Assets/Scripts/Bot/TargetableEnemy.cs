using UnityEngine;

public class TargetableEnemy : MonoBehaviour
{
    public Transform aimTarget; // thường là head, hoặc chest nếu không có head

    public Transform GetAimTarget()
    {
        return aimTarget != null ? aimTarget : transform;
    }
}
