using UnityEngine;

public class TestAll : MonoBehaviour
{
   void Update()
   {
       // Test damage
       if (Input.GetKeyDown(KeyCode.T))
       {
            Debug.Log(GameObject.FindGameObjectWithTag("Player").name);
            GameObject.FindGameObjectWithTag("Player")
               .GetComponent<PlayerHealthSystem>()
               .TakeDamage(Random.Range(10, 50), transform.position);
       }
   }
}
