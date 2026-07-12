using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GameOverScreen : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI ordersMissedText;
    public TextMeshProUGUI wrongOrdersServedText;
    public TextMeshProUGUI ordersCompletedText;
    public TextMeshProUGUI scoreText;
    public Button restartButton;

    [Header("Float Settings")]
    public float distanceFromCamera = 0.6f;
    public float floatSpeed = 3f;

    [Header("XR")]
    public NearFarInteractor leftInteractor;
    public NearFarInteractor rightInteractor;

    private Transform cameraTransform;
    private bool isShowing = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // toDollars converts score to dollars (ex. score of 405 to $40.50)
    private string ToDollars(int score)
    {
        return "$" + (score / 10f).ToString("F2");
    }

    void Start()
    {
        panel.SetActive(false);
        cameraTransform = Camera.main.transform;

        restartButton.onClick.AddListener(OnRestart);

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameEnded += Show;
        else
            Debug.LogError("GameOverScreen: GameManager.Instance is null");
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameEnded -= Show;
    }

    void Update()
    {
        if (!isShowing) return;

        targetPosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        targetRotation = Quaternion.LookRotation(transform.position - cameraTransform.position);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * floatSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.unscaledDeltaTime * floatSpeed);
    }

    void Show()
    {
        panel.SetActive(true);
        isShowing = true;

        transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);

        if (ScoreManager.Instance != null)
        {
            var sm = ScoreManager.Instance;
            ordersMissedText.text = $"Missed: {sm.ordersMissed}";
            wrongOrdersServedText.text = $"Wrong: {sm.wrongOrdersServed}";
            ordersCompletedText.text = $"Completed: {sm.ordersCompleted}";
            scoreText.text = $"Total: {ToDollars(sm.score)}";
        }

        if (leftInteractor != null) leftInteractor.enableFarCasting = true;
        if (rightInteractor != null) rightInteractor.enableFarCasting = true;
    }

    void OnRestart()
    {
        isShowing = false;
        panel.SetActive(false);
        if (leftInteractor != null) leftInteractor.enableFarCasting = false;
        if (rightInteractor != null) rightInteractor.enableFarCasting = false;
        GameManager.Instance.RestartGame();
    }
}