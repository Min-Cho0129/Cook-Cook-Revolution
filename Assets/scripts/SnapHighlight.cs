using UnityEngine;

// Attach to the same GameObject as StackDropZone.
// Assign a highlight material in the inspector — swap on valid hover, restore on exit.
public class SnapHighlight : MonoBehaviour
{
    [Header("Highlight")]
    public Material highlightMaterial;

    StackableIngredient owner;
    FoodStack parentStack;
    Renderer ownerRenderer;
    Material originalMaterial;

    void Start()
    {
        owner = GetComponentInParent<StackableIngredient>();
        parentStack = GetComponentInParent<FoodStack>();
        ownerRenderer = owner?.GetComponentInChildren<Renderer>();
        if (ownerRenderer != null)
            originalMaterial = ownerRenderer.material;
    }

    void OnTriggerEnter(Collider other)
    {
        var incoming = other.GetComponent<StackableIngredient>();
        if (incoming == null) return;
        if (!parentStack.CanAccept(incoming)) return;

        if (ownerRenderer != null && highlightMaterial != null)
            ownerRenderer.material = highlightMaterial;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<StackableIngredient>() == null) return;
        if (ownerRenderer != null && originalMaterial != null)
            ownerRenderer.material = originalMaterial;
    }
}
