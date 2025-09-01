using UnityEngine;

public class TestAll : MonoBehaviour
{
   public float time = 1f;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Invoke(nameof(DoDamage), time); // gọi sau 1 giây
        }
    }

    void DoDamage()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log(player.name);
            player.GetComponent<PlayerHealthSystem>()
                  .TakeDamage(Random.Range(10, 50), transform.position);
        }
    }
}
