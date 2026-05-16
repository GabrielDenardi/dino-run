using UnityEngine;
using UnityEngine.SceneManagement;

public enum DinoGameState
{
    Ready,
    Playing,
    GameOver
}

public class DinoGameManager : MonoBehaviour
{
    private const string BestScoreKey = "DinoBestScore";

    public static DinoGameManager Instance { get; private set; }

    [SerializeField] private float baseSpeed = 6f;
    [SerializeField] private float maxSpeed = 14f;
    [SerializeField] private float speedIncreasePerSecond = 0.22f;
    [SerializeField] private float pointsPerSecond = 12f;

    public DinoGameState State { get; private set; } = DinoGameState.Ready;
    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public float CurrentSpeed { get; private set; }

    public bool IsPlaying => State == DinoGameState.Playing;
    public bool IsGameOver => State == DinoGameState.GameOver;

    private float elapsedTime;
    private float pointAccumulator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        ResetRun();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        StartRun();
    }

    private void Update()
    {
        if (State == DinoGameState.Playing)
        {
            elapsedTime += Time.deltaTime;
            CurrentSpeed = Mathf.Min(maxSpeed, baseSpeed + elapsedTime * speedIncreasePerSecond);

            pointAccumulator += pointsPerSecond * Time.deltaTime;
            if (pointAccumulator >= 1f)
            {
                var add = Mathf.FloorToInt(pointAccumulator);
                pointAccumulator -= add;
                Score += add;
                if (Score > BestScore)
                {
                    SetBestScore(Score);
                }
            }
        }

        if (State == DinoGameState.GameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartCurrentScene();
        }
    }

    public void StartRun()
    {
        ResetRun();
        State = DinoGameState.Playing;
        CurrentSpeed = baseSpeed;
    }

    public void GameOver(string reason = null)
    {
        if (State == DinoGameState.GameOver)
        {
            return;
        }

        State = DinoGameState.GameOver;
        if (Score > BestScore)
        {
            SetBestScore(Score);
        }
    }

    private void ResetRun()
    {
        State = DinoGameState.Ready;
        Score = 0;
        elapsedTime = 0f;
        pointAccumulator = 0f;
        CurrentSpeed = baseSpeed;
    }

    private void SetBestScore(int value)
    {
        if (value <= BestScore)
        {
            return;
        }

        BestScore = value;
        PlayerPrefs.SetInt(BestScoreKey, BestScore);
        PlayerPrefs.Save();
    }

    private static void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
