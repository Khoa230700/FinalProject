using UnityEngine;

public class TestAll : MonoBehaviour
{
   void Update()
   {
       // Test damage
       if (Input.GetKeyDown(KeyCode.T))
       {
           GameObject.FindGameObjectWithTag("Player")
               .GetComponent<PlayerHealthSystem>()
               .TakeDamage(Random.Range(10, 50));
       }
   }
}
