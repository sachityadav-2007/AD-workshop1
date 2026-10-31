using System;
using UnityEngine;

public enum GameState { MainMenu, Playing, GameOver }

/// <summary>Owns game state, score, timer and difficulty. Everything else listens to its events.</summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Rules")]
    [SerializeField, Min(5f)] private float gameDuration = 60f;

    [Header("Difficulty")]
    [Tooltip("Difficulty added per coin collected.")]
    [SerializeField] private float difficultyPerCoin = 0.03f;
    [Tooltip("Difficulty added per second of play.")]
    [SerializeField] private float difficultyPerSecond = 0.01f;
    [Tooltip("Upper limit so the game stays fair.")]
    [SerializeField] private float maxDifficulty = 2.5f;

    private const string BestScoreKey = "CoinCatcher.BestScore";

    public GameState State { get; private set; } = GameState.MainMenu;
    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public bool IsNewBest { get; private set; }
    public float TimeRemaining { get; private set; }
    public float GameDuration => gameDuration;

    /// <summary>1.0 at the start, rises with score and elapsed time, capped at maxDifficulty.</summary>
    public float Difficulty
    {
        get
        {
            float elapsed = gameDuration - TimeRemaining;
            return Mathf.Min(maxDifficulty, 1f + Score * difficultyPerCoin + elapsed * difficultyPerSecond);
        }
    }

    public event Action<GameState> StateChanged;
    public event Action<int> ScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        TimeRemaining = gameDuration;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (State != GameState.Playing) return;

        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            EndGame();
        }
    }

    public void StartGame()
    {
        Score = 0;
        IsNewBest = false;
        TimeRemaining = gameDuration;
        ScoreChanged?.Invoke(Score);
        SetState(GameState.Playing);
    }

    public void AddScore(int amount)
    {
        if (State != GameState.Playing) return;
        Score += amount;
        ScoreChanged?.Invoke(Score);
    }

    public void ReturnToMenu()
    {
        Score = 0;
        TimeRemaining = gameDuration;
        ScoreChanged?.Invoke(Score);
        SetState(GameState.MainMenu);
    }

    private void EndGame()
    {
        if (Score > BestScore)
        {
            BestScore = Score;
            IsNewBest = true;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }

        SetState(GameState.GameOver);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();
    }

    private void SetState(GameState newState)
    {
        State = newState;
        StateChanged?.Invoke(newState);
    }
}