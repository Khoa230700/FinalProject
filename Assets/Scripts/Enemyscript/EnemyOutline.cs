using EPOOutline;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyOutline : MonoBehaviour
{
    [SerializeField] private Outlinable outlinable;

    private SpawnManager spawnManager;
    private bool isNotScene = true;

    private void Start()
    {
        spawnManager = FindAnyObjectByType<SpawnManager>();

        if(SceneManager.GetActiveScene().name == "SceneMainGame")
        {
            isNotScene = false;
        }
    }

    private void OnEnable()
    {
        outlinable.enabled = false;
    }

    private void OnDisable()
    {
        outlinable.enabled = false;
    }

    private void Update()
    {
        if (isNotScene) return;

        if (spawnManager != null && spawnManager.ActiveEnemyCount < 5)
        {
            outlinable.enabled = true;
        }
    }
}
