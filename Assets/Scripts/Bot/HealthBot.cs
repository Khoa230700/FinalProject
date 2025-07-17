using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class HealthBot : MonoBehaviour
{

    public float healthBot;
    public float currentHealth;
    private void Start()
    {
        healthBot = currentHealth;
    }
    
    public void TakeDame(float dame)
    {
        currentHealth -= dame;
        if (currentHealth < 0f)
        {
            Debug.Log("die");
        }
    }
}
