using UnityEngine;

public class LookAtCameraUI : MonoBehaviour
{
    [SerializeField] private Transform cam;

    void Start()
    {
        if (cam == null)
            cam = FindAnyObjectByType<Camera>().transform;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward, cam.up);
    }
}
