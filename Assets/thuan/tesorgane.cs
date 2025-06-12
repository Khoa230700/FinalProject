using UnityEngine;

public class tesorgane : MonoBehaviour
{
    public Transform _camera;
    public Transform hand;
    public float cameraSensitivity = 200.0f;
    public float cameraAcceleration = 5.0f;

    private float rotation_x_axsis;
    private float rotation_y_axsis;
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;

        rotation_x_axsis += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
        rotation_y_axsis += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;

        rotation_x_axsis = Mathf.Clamp(rotation_x_axsis, -90.0f, 90.0f);

        hand.localRotation = Quaternion.Euler(-rotation_x_axsis, rotation_y_axsis, 0);

        transform.localRotation = Quaternion.Lerp(transform.localRotation,
            Quaternion.Euler(0, rotation_y_axsis, 0), cameraAcceleration * Time.deltaTime);
        _camera.localRotation = Quaternion.Lerp(_camera.localRotation,
            Quaternion.Euler(-rotation_x_axsis, 0, 0), cameraAcceleration * Time.deltaTime);
    }
}
