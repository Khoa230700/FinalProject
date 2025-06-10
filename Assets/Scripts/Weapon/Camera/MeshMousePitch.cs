using UnityEngine;

public class MeshMouseLook : MonoBehaviour
{
    public float sensitivityX = 2f;
    public float sensitivityY = 2f;
    public float minPitch = -60f;
    public float maxPitch = 60f;

    private float pitch = 0f; // X - nhìn lên/xuống
    private float yaw = 0f;   // Y - quay trái/phải

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Ẩn và khoá chuột vào giữa màn hình
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
