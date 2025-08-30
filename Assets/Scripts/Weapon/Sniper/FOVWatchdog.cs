using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class FOVWatchdog : MonoBehaviour
{
    [Tooltip("Biên an toàn: FOV sẽ luôn được kẹp trong khoảng này.")]
    public float clampMin = 15f, clampMax = 90f;

    private Camera cam;
    private float last;

    void Awake()
    {
        cam = GetComponent<Camera>();
        last = cam.fieldOfView;
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, clampMin, clampMax);
    }

    void LateUpdate()
    {
        if (!cam) return;

        if (cam.fieldOfView != last)
        {
            Debug.Log($"[FOVWatchdog] FOV changed {last:F4} -> {cam.fieldOfView:F6}", this);
            last = cam.fieldOfView;
        }

        // luôn ép nằm trong biên an toàn
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, clampMin, clampMax);
    }
}
