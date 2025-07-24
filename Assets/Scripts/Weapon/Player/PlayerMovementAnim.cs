using UnityEngine;

public class PlayerMovementAnim : MonoBehaviour
{
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShoot playerShoot;

    void Update()
    {
        bool isMoving = playerMovement.IsMoving();
        bool isRunning = playerMovement.IsRunning();
        bool isShooting = playerShoot != null && playerShoot.IsShooting;
        bool isSwitching = playerShoot != null && playerShoot.IsSwitchingWeapon;

        if (isSwitching || isShooting)
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Run", false);
            return;
        }

        if (isMoving && isRunning)
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Run", true);
        }
        else if (isMoving)
        {
            armsAnimator.SetBool("Walk", true);
            armsAnimator.SetBool("Run", false);
        }
        else
        {
            armsAnimator.SetBool("Walk", false);
            armsAnimator.SetBool("Run", false);
        }
    }
}
