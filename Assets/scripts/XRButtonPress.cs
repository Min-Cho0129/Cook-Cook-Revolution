using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRButtonPress : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "Press";
    
    [Header("Optional")]
    [SerializeField] private bool useBoolInsteadOfTrigger = false;
    [SerializeField] private string boolParameterName = "IsPressed";
    
    [Header("Interactable (auto-found if left empty)")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    private void Awake()
    {
        if (interactable == null)
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        
        if (interactable == null)
            interactable = GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        if (interactable == null)
            Debug.LogError($"[{nameof(XRButtonPress)}] No XRBaseInteractable found on '{gameObject.name}' or its children. Add an XR Simple Interactable (or similar) component.", this);
        
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (interactable == null) return;
        
        interactable.selectEntered.AddListener(OnButtonPressed);
        interactable.selectExited.AddListener(OnButtonReleased);
    }

    private void OnDisable()
    {
        if (interactable == null) return;
        
        interactable.selectEntered.RemoveListener(OnButtonPressed);
        interactable.selectExited.RemoveListener(OnButtonReleased);
    }

    private void OnButtonPressed(SelectEnterEventArgs args)
    {
        if (animator == null) return;

        if (useBoolInsteadOfTrigger)
            animator.SetBool(boolParameterName, true);
        else
            animator.SetTrigger(triggerName);
    }

    private void OnButtonReleased(SelectExitEventArgs args)
    {
        if (animator == null) return;

        if (useBoolInsteadOfTrigger)
            animator.SetBool(boolParameterName, false);
    }
}