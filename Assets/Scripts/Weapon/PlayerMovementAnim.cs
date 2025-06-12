using UnityEngine;

public class PlayerMovementAnim : MonoBehaviour
{
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private PlayerShoot playerShoot;

    private bool isWalking = false;
    private bool isRunning = false;

    void Update()
    {
        // Nếu đang bắn thì không thực hiện chuyển đổi run/walk
        if (playerShoot != null && playerShoot.IsShooting)
            return;

        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                        Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        bool isRunningInput = Input.GetKey(KeyCode.LeftShift);

        if (isMoving)
        {
            if (isRunningInput)
            {
                if (!isRunning)
                {
                    armsAnimator.SetTrigger("Run");
                    isRunning = true;
                    isWalking = false;
                }
            }
            else
            {
                if (!isWalking)
                {
                    armsAnimator.SetTrigger("Walk");
                    isWalking = true;
                    isRunning = false;
                }
            }
        }
        else
        {
            if (isWalking || isRunning)
            {
                armsAnimator.SetTrigger("Idle");
                isWalking = false;
                isRunning = false;
            }
        }
    }
}
