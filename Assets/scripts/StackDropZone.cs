using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class StackDropZone : MonoBehaviour
{
    StackableIngredient owner;
    StackableIngredient pendingIngredient;

    void Start()
    {
        owner = GetComponentInParent<StackableIngredient>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (pendingIngredient != null) return;

        var incoming = other.GetComponent<StackableIngredient>();
        if (incoming == null) incoming = other.GetComponentInParent<StackableIngredient>();
        if (incoming == null) return;
        if (incoming == owner) return;

        var stack = GetComponentInParent<FoodStack>();
        if (stack == null) return;
        if (stack.topLayer != owner) return; // only the top layer accepts drops

        if (!stack.CanAccept(incoming)) return;

        var grab = incoming.GetComponent<XRGrabInteractable>();
        if (grab == null || !grab.enabled) return;

        pendingIngredient = incoming;
        grab.selectExited.AddListener(OnReleased);
    }

    void OnTriggerExit(Collider other)
    {
        if (pendingIngredient == null) return;
        var outgoing = other.GetComponent<StackableIngredient>();
        if (outgoing == null) outgoing = other.GetComponentInParent<StackableIngredient>();
        if (outgoing != pendingIngredient) return;

        var grab = pendingIngredient.GetComponent<XRGrabInteractable>();
        if (grab != null) grab.selectExited.RemoveListener(OnReleased);

        pendingIngredient = null;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (pendingIngredient == null) return;

        var grab = pendingIngredient.GetComponent<XRGrabInteractable>();
        if (grab != null) grab.selectExited.RemoveListener(OnReleased);

        var stack = GetComponentInParent<FoodStack>();
        var toAdd = pendingIngredient;
        pendingIngredient = null; // clear BEFORE AddLayer

        if (stack != null && stack.CanAccept(toAdd))
            stack.AddLayer(toAdd);
    }

    public bool TryCompleteDesktopDrop(StackableIngredient incoming)
    {
        if (incoming == null) return false;
        if (incoming == owner) return false;

        var stack = GetComponentInParent<FoodStack>();
        if (stack == null) return false;
        if (stack.topLayer != owner) return false;
        if (!stack.CanAccept(incoming)) return false;

        if (pendingIngredient == incoming)
        {
            var grab = pendingIngredient.GetComponent<XRGrabInteractable>();
            if (grab != null) grab.selectExited.RemoveListener(OnReleased);
            pendingIngredient = null;
        }

        stack.AddLayer(incoming);
        return true;
    }
}
