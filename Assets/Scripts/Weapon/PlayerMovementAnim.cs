using UnityEngine;

public class PlayerMovementAnim : MonoBehaviour
{
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private PlayerMovement playerMovement;

    void Update()
    {
        if (playerShoot != null && playerShoot.IsShooting)
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Run", false);
            return;
        }

        bool isMoving = playerMovement.IsMoving();
        bool isRunning = playerMovement.IsRunning();

        armsAnimator.SetBool("Walk", isMoving && !isRunning);
        armsAnimator.SetBool("Run", isRunning);
    }
}
