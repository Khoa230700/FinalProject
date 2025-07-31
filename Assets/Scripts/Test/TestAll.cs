using UnityEngine;

public class TestAll : MonoBehaviour
{
    public float damage = 10f;

    void Update()
    {
        if (Input.GetKey(KeyCode.T))
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>().TakeDamage(damage, 0f, transform.position);
        }

        // Movement controls
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);
        transform.Translate(movement * 5f * Time.deltaTime);
    }
}
