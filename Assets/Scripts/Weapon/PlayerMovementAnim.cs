using UnityEngine;

public class PlayerMovementAnim : MonoBehaviour
{
    [SerializeField] private Animator armsAnimator;

    private bool isWalking = false;
    private bool isRunning = false;

    void Update()
    {
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                        Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        bool isRunningInput = Input.GetKey(KeyCode.LeftShift);

        if (isMoving)
        {
            if (isRunningInput)
            {
                // Đang nhấn shift + di chuyển => chạy
                if (!isRunning)
                {
                    armsAnimator.SetTrigger("Run");
                    isRunning = true;
                    isWalking = false;
                }
            }
            else
            {
                // Chỉ di chuyển, không shift => đi bộ
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
            // Không nhấn gì => Idle
            if (isWalking || isRunning)
            {
                armsAnimator.SetTrigger("Idle");
                isWalking = false;
                isRunning = false;
            }
        }
    }
}
