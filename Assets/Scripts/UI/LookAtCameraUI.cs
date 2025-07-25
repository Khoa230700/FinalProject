using UnityEngine;

public class LookAtCameraUI : MonoBehaviour
{
    [SerializeField] private Transform cam;

    void Start()
    {
        cam ??= Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward, cam.up);
    }
}
