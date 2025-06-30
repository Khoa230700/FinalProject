using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float bobFrequency = 5f;
    public float bobAmplitude = 0.05f;
    public float speedThreshold = 0.1f; // Chỉ rung khi đang thực sự di chuyển

    [Header("References")]
    public Transform cameraTransform;
    public PlayerMovement playerMovement;

    private Vector3 initialLocalPos;
    private float timer = 0f;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        initialLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        if (playerMovement == null || cameraTransform == null)
            return;

        if (playerMovement.IsMoving() && playerMovement.IsGrounded())
        {
            timer += Time.deltaTime * bobFrequency;
            float bobOffset = Mathf.Sin(timer) * bobAmplitude;

            Vector3 newPos = initialLocalPos + new Vector3(0f, bobOffset, 0f);
            cameraTransform.localPosition = newPos;
        }
        else
        {
            // Quay lại vị trí gốc khi không di chuyển
            timer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, initialLocalPos, Time.deltaTime * 5f);
        }
    }
}
