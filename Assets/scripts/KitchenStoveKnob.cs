using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class KitchenStoveKnob : MonoBehaviour, IDesktopInteractable
{
    public bool isOn = false;
    public float rotationAngle = 90f; // angle to rotate when turned on
    XRSimpleInteractable interactable;

    float currentRotationAngle = 0f;

    void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
            interactable.selectEntered.AddListener(OnSelectEntered);
    }

    void Update()
    {
        currentRotationAngle = Mathf.Lerp(currentRotationAngle, isOn ? rotationAngle : 0, Time.deltaTime * 10f);
        transform.localRotation = Quaternion.Euler(0, currentRotationAngle, 0);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        Toggle();
    }

    public void DesktopInteract()
    {
        Toggle();
    }

    public void Toggle()
    {
        isOn = !isOn;
    }

    void OnDestroy()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelectEntered);
    }
}
