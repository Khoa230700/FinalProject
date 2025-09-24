using UnityEngine;

public class SatelliteOrbit : MonoBehaviour
{
    public Transform planet;        // The object to orbit around
    [SerializeField]private float orbitSpeed = 10f;  // Degrees per second
    public Vector3 orbitAxis = Vector3.up;  // Axis of rotation

    void Update()
    {
        if (planet != null)
        {
            transform.RotateAround(planet.position, orbitAxis, orbitSpeed * Time.deltaTime);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<testPlayerHealth>().TakeDamage(40);
            Debug.Log("hit");
        }
    }
}
