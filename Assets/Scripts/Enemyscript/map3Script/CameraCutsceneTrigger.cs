using Unity.Cinemachine;
using UnityEngine;

public class CameraCutsceneTrigger : MonoBehaviour
{
    public CinemachineVirtualCamera cutsceneCam;
    public CinemachineVirtualCamera playerCam;
    public float cutsceneDuration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered cutscene trigger.");

            // Switch camera
            cutsceneCam.Priority = 20;
            playerCam.Priority = 10;

            // Revert after duration
            Invoke(nameof(RevertCamera), cutsceneDuration);
        }
    }

    private void RevertCamera()
    {
        Debug.Log("Reverting to player camera.");

        cutsceneCam.Priority = 10;
        playerCam.Priority = 20;
    }
}
