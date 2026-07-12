using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PauseScreen : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public Button resumeButton;
    public Button restartButton;

    [Header("Input")]
    public InputActionReference menuButton;

    [Header("Float Settings")]
    public float distanceFromCamera = 0.6f;
    public float floatSpeed = 3f;

    [Header("XR")]
    public NearFarInteractor leftInteractor;
    public NearFarInteractor rightInteractor;

    private Transform cameraTransform;
    private bool isPaused = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        panel.SetActive(false);
        cameraTransform = Camera.main.transform;

        resumeButton.onClick.AddListener(Resume);
        restartButton.onClick.AddListener(Restart);

        if (menuButton != null && menuButton.action != null)
        {
            menuButton.action.performed += OnMenuPressed;
            menuButton.action.Enable();
        }
    }

    void OnDestroy()
    {
        if (menuButton != null && menuButton.action != null)
            menuButton.action.performed -= OnMenuPressed;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && CanTogglePause())
        {
            if (isPaused) Resume();
            else Pause();
        }

        if (!isPaused) return;

        targetPosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        targetRotation = Quaternion.LookRotation(transform.position - cameraTransform.position);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * floatSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.unscaledDeltaTime * floatSpeed);
    }

    void OnMenuPressed(InputAction.CallbackContext ctx)
    {
        if (!CanTogglePause()) return;

        if (isPaused) Resume();
        else Pause();
    }

    bool CanTogglePause()
    {
        foreach (StartScreen startScreen in FindObjectsOfType<StartScreen>())
        {
            if ((startScreen.mainPanel != null && startScreen.mainPanel.activeInHierarchy) ||
                (startScreen.rulesPanel != null && startScreen.rulesPanel.activeInHierarchy))
                return false;
        }

        foreach (GameOverScreen gameOverScreen in FindObjectsOfType<GameOverScreen>())
        {
            if (gameOverScreen.panel != null && gameOverScreen.panel.activeInHierarchy)
                return false;
        }

        return true;
    }

    void Pause()
    {
        isPaused = true;
        panel.SetActive(true);
        GameManager.Instance.PauseGame();

        transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);

        if (leftInteractor != null) leftInteractor.enableFarCasting = true;
        if (rightInteractor != null) rightInteractor.enableFarCasting = true;
    }

    void Resume()
    {
        isPaused = false;
        panel.SetActive(false);
        GameManager.Instance.ResumeGame();

        if (leftInteractor != null) leftInteractor.enableFarCasting = false;
        if (rightInteractor != null) rightInteractor.enableFarCasting = false;
    }

    void Restart()
    {
        isPaused = false;
        panel.SetActive(false);
        if (leftInteractor != null) leftInteractor.enableFarCasting = false;
        if (rightInteractor != null) rightInteractor.enableFarCasting = false;
        GameManager.Instance.RestartGame();
    }
}
