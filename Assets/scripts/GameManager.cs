using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public float gameDurationInSeconds = 180f;
    private float gameTimeRemaining;
    private bool gameOver = false;

    public bool restart = false;

    public event System.Action OnGameStarted;
    public event System.Action OnGameEnded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float GetTimeRemaining() => gameTimeRemaining;

    IEnumerator DelayedStart()
    {
        yield return null;
        OnGameStarted?.Invoke();
    }

    void Start()
    {
        gameOver = false;
        Time.timeScale = 0f; // start paused, StartScreen unpauses
        gameTimeRemaining = gameDurationInSeconds;
        StartCoroutine(DelayedStart());
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (gameOver)
        {
            if (restart) RestartGame();
            return;
        }
        gameTimeRemaining -= Time.deltaTime;
        PhysicalTimer.SetTime(gameTimeRemaining);

        if (gameTimeRemaining <= 0)
        {
            gameOver = true;
            OnGameEnded?.Invoke();
            Time.timeScale = 0f;
        }
    }

    public void PauseGame()
    {
        if (gameOver) return;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (gameOver) return;
        Time.timeScale = 1f;
    }
}