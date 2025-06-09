using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }
    public static GameManager Instance { get; private set; }
    public GameState CurrentGameState { get; private set; } = GameState.MainMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ChangeState(GameState.MainMenu);
    }
    public void ChangeState(GameState newState)
    {
        if(CurrentGameState == newState) return;
        CurrentGameState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenu();
                break;
            case GameState.Playing:
                HandlePlaying();
                break;
            case GameState.Paused:
                HandlePaused();
                break;
            case GameState.GameOver:
                HandleGameOver();
                break;
        }
    }

    private void HandleMainMenu() { Time.timeScale = 0f; }
    private void HandlePlaying() { Time.timeScale = 1f; }
    private void HandlePaused() { Time.timeScale = 0f; }
    private void HandleGameOver() { Time.timeScale = 0f; EvenBus.GameOver(); }
}
