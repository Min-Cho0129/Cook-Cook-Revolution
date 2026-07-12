using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int ordersCompleted;
    public int ordersMissed;
    public int wrongOrdersServed;

    public int score;
    public int penalties;

    public int wrongServePenalty = 25;

    public event System.Action<int> OnScoreChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (RecipeManager.Instance != null)
        {
            RecipeManager.Instance.OnOrderCompleted += OnOrderCompleted;
            RecipeManager.Instance.OnOrderExpired += OnOrderExpired;
            ServeZone serveZone = FindObjectOfType<ServeZone>();
            if (serveZone != null)
            {
                serveZone.OnWrongServe += WrongServe;
            }
            else
            {
                Debug.LogError("ScoreManager: No ServeZone found in Start");
            }
        }
        else
        {
            Debug.LogError("ScoreManager: RecipeManager.Instance is null in Start");
        }
    }

    void OnDestroy()
    {
        if (RecipeManager.Instance != null)
        {
            RecipeManager.Instance.OnOrderCompleted -= OnOrderCompleted;
            RecipeManager.Instance.OnOrderExpired -= OnOrderExpired;
            ServeZone serveZone = FindObjectOfType<ServeZone>();
            if (serveZone != null)
            {
                serveZone.OnWrongServe -= WrongServe;
            }
        }
    }

    void OnOrderCompleted(Order order)
    {
        print("ScoreManager: Order completed, adding " + order.recipe.scoreValue + " points");
        ordersCompleted++;
        AddScore(order.recipe.scoreValue);
    }

    void OnOrderExpired(Order order)
    {
        print("ScoreManager: Order expired, applying penalty of " + wrongServePenalty + " points");
        AddScore(-wrongServePenalty);
        penalties++;
        ordersMissed++;
    }

    public void AddScore(int amount)
    {
        score = Mathf.Max(0, score + amount);
        OnScoreChanged?.Invoke(score);
    }

    public void WrongServe()
    {
        print("ScoreManager: Wrong serve, applying penalty of " + wrongServePenalty + " points");
        wrongOrdersServed++;
        AddScore(-wrongServePenalty);
        penalties++;
    }
}