using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class StartScreen : MonoBehaviour
{
    [Header("UI")]
    public GameObject mainPanel;
    public GameObject rulesPanel;
    public Button startButton;
    public Button rulesButton;
    public Button backButton;

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

    void Start()
    {
        cameraTransform = Camera.main.transform;
        startButton.onClick.AddListener(OnStart);
        rulesButton.onClick.AddListener(OnShowRules);
        backButton.onClick.AddListener(OnBack);
        Show();
    }

    void Update()
    {
        if (!isShowing) return;

        HandleDesktopKeyboardShortcuts();

        targetPosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        targetRotation = Quaternion.LookRotation(transform.position - cameraTransform.position);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * floatSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.unscaledDeltaTime * floatSpeed);
    }

    void HandleDesktopKeyboardShortcuts()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (mainPanel.activeSelf)
        {
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
                OnStart();
            else if (keyboard.rKey.wasPressedThisFrame)
                OnShowRules();
        }
        else if (rulesPanel.activeSelf)
        {
            if (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
                OnBack();
        }
    }

    void Show()
    {
        isShowing = true;
        mainPanel.SetActive(true);
        rulesPanel.SetActive(false);

        transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);

        if (leftInteractor != null) leftInteractor.enableFarCasting = true;
        if (rightInteractor != null) rightInteractor.enableFarCasting = true;
    }

    void OnShowRules()
    {
        mainPanel.SetActive(false);
        rulesPanel.SetActive(true);
    }

    void OnBack()
    {
        mainPanel.SetActive(true);
        rulesPanel.SetActive(false);
    }

    void OnStart()
    {
        isShowing = false;
        mainPanel.SetActive(false);
        rulesPanel.SetActive(false);
        GameManager.Instance.ResumeGame();

        if (leftInteractor != null) leftInteractor.enableFarCasting = false;
        if (rightInteractor != null) rightInteractor.enableFarCasting = false;
    }
}
