using UnityEngine;

public class TestAll : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;

    void Update()
    {
        // Test damage
        if (Input.GetKeyDown(KeyCode.T))
        {
            GameObject.FindGameObjectWithTag("Player")
                .GetComponent<PlayerHealth>()
                .TakeDamage(Random.Range(10, 50), 0f, transform.position);
        }

        // Movement controls
        float moveVertical = KeyBindingManager.Instance.GetAxis("Vertical");   // W/S
        float turn = KeyBindingManager.Instance.GetAxis("Horizontal");        // A/D

        // Tiến lùi
        Vector3 movement = transform.forward * moveVertical;
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        // Xoay trái phải
        transform.Rotate(Vector3.up * turn * rotationSpeed * Time.deltaTime);
    }
}
