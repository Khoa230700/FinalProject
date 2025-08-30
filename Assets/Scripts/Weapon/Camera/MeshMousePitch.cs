using UnityEngine;

public class MeshMouseLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensitivityX = 2f;   // yaw (trái/phải)
    public float sensitivityY = 2f;   // pitch (lên/xuống)
    public bool invertY = false;
    public bool useDeltaTime = false;

    [Header("Clamp")]
    public float minPitch = -60f;
    public float maxPitch = 60f;

    [Header("Targets (optional)")]
    [Tooltip("Transform chịu yaw (quay trái/phải). Để trống sẽ dùng chính gameObject này.")]
    public Transform yawTransform;
    [Tooltip("Transform chịu pitch (ngửa/gục). Để trống sẽ dùng yawTransform (hoặc chính object).")]
    public Transform pitchTransform;

    private float pitch = 0f; // delta từ góc ban đầu
    private float yaw = 0f; // delta từ góc ban đầu
    private bool mouseLookEnabled = false;

    private Quaternion yawBaseRot;
    private Quaternion pitchBaseRot;

    void Awake()
    {
        if (!yawTransform) yawTransform = transform;
        if (!pitchTransform) pitchTransform = yawTransform;

        yawBaseRot = yawTransform.localRotation;
        pitchBaseRot = pitchTransform.localRotation;
    }

    void Start()
    {
        Hide(); // khoá & ẩn chuột, bật mouselook
    }

    void Update()
    {
        if (!mouseLookEnabled) return;

        float mx = Input.GetAxis("Mouse X") * sensitivityX;
        float my = Input.GetAxis("Mouse Y") * sensitivityY;

        if (useDeltaTime)
        {
            mx *= Time.deltaTime * 60f;
            my *= Time.deltaTime * 60f;
        }

        yaw += mx;
        pitch += (invertY ? my : -my);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (pitchTransform == yawTransform)
        {
            // 1 transform: gộp yaw + pitch
            yawTransform.localRotation = yawBaseRot * Quaternion.Euler(pitch, yaw, 0f);
        }
        else
        {
            // 2 transform: áp riêng từng trục
            yawTransform.localRotation = yawBaseRot * Quaternion.Euler(0f, yaw, 0f);
            pitchTransform.localRotation = pitchBaseRot * Quaternion.Euler(pitch, 0f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Show();
    }

    public void Show()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mouseLookEnabled = false;
    }

    public void Hide()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mouseLookEnabled = true;
    }

    public void SetEnabled(bool enabledLook)
    {
        if (enabledLook) Hide(); else Show();
    }

    public void Recenter()
    {
        // đặt base = tư thế hiện tại, reset delta
        yawBaseRot = yawTransform.localRotation;
        pitchBaseRot = pitchTransform.localRotation;
        yaw = 0f;
        pitch = 0f;
    }
}
