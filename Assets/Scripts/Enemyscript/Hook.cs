using UnityEngine;

public class Hook : MonoBehaviour
{
    public float speed = 900f;
    public float lifeTime = 1f;

    private Transform target;
    private EnemyHookThrow enemy;
    private Transform ropeStart;
    private LineRenderer lr;

    private Vector3 moveDir;
    private bool isFlying = true;

    // nhận sẵn firePoint + hướng đã tính
    public void Init(Transform targetPlayer, EnemyHookThrow enemyRef, Transform ropeStartPoint, Vector3 initialDir)
    {
        target = targetPlayer;
        enemy = enemyRef;
        ropeStart = ropeStartPoint;
        moveDir = initialDir; // <-- hướng bay cố định theo tia firePoint->player

        lr = GetComponent<LineRenderer>();
        if (lr)
        {
            lr.positionCount = 2;
            lr.useWorldSpace = true;
        }

        Destroy(gameObject, lifeTime); // tự hủy nếu không trúng
    }

    void Update()
    {
        if (isFlying)
            transform.position += moveDir * speed * Time.deltaTime;

        if (lr)
        {
            Vector3 startPos = ropeStart ? ropeStart.position : (enemy ? enemy.transform.position : transform.position);
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, transform.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // so khớp root để dính cả collider con của Player
        if (target && other.transform.root == target.root)
        {
            isFlying = false;
            enemy.StartPull(target);
            Destroy(gameObject);
        }
    }
}
