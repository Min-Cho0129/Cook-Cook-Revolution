using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Bell : MonoBehaviour, IDesktopInteractable
{
    AudioSource audioSource;
    XRSimpleInteractable interactable;
    public event System.Action OnBellRung;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        interactable = gameObject.GetComponent<XRSimpleInteractable>();
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSelectEntered);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        Ring();
    }

    public void DesktopInteract()
    {
        Ring();
    }

    public void Ring()
    {
        if (audioSource == null)
            audioSource = gameObject.GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.Play();

        OnBellRung?.Invoke();
    }

    void OnDestroy()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelectEntered);
    }
}
