using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager Instance { get; private set; }

    public QuestEvents questEvents;
    public PlayerEvents playerEvents;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Game Events Manager in the scene.");
            Destroy(gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(this);

        questEvents = new QuestEvents();
        playerEvents = new PlayerEvents();
    }
}
