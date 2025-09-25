using UnityEngine;

public class TestAll : MonoBehaviour
{
    public float time = 1f;
    public float maxDamge = 100f;

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
            player.GetComponent<PlayerHealthSystem>()
                  .TakeDamage(maxDamge, transform.position);
        }
    }
}
