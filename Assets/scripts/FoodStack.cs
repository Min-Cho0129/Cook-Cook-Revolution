using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


public class FoodStack : MonoBehaviour
{
    public List<StackableIngredient> layers = new();
    public StackableIngredient topLayer => layers.Count > 0 ? layers[^1] : null;

    public event System.Action<Recipe> OnRecipeComplete;
    public event System.Action OnStackComplete;

    public bool CanAccept(StackableIngredient incoming)
    {
        if (topLayer == null) return false;
        return System.Array.Exists(topLayer.validNextLayers,
            id => id == incoming.ingredientID);
    }

    public void AddLayer(StackableIngredient ingredient)
    {
        if (ingredient == null) return;
        var grab = ingredient.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab != null) Destroy(grab);

        var rb = ingredient.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        Transform snapTo = topLayer.snapPoint;
        ingredient.transform.SetParent(snapTo);
        ingredient.transform.localPosition = Vector3.zero;
        ingredient.transform.localRotation = Quaternion.identity;

        layers.Add(ingredient);
        CheckRecipes();
    }

    void CheckRecipes()
    {
        if (RecipeManager.Instance == null) return;

        foreach (var recipe in RecipeManager.Instance.activeRecipes)
        {
            if (MatchesRecipe(recipe))
            {
                OnRecipeComplete?.Invoke(recipe);
                return;
            }
        }

        OnStackComplete?.Invoke();
    }

    public bool MatchesRecipe(Recipe recipe)
    {
        if (layers.Count - 1 != recipe.requiredLayers.Length) return false;
        for (int i = 0; i < recipe.requiredLayers.Length; i++)
        {
            if (layers[i + 1].ingredientID != recipe.requiredLayers[i].ingredientID) return false;
        }
        return true;
    }

    public void ResetStack()
    {
        layers.Clear();
        var baseIngredient = GetComponent<StackableIngredient>();
        if (baseIngredient != null) layers.Add(baseIngredient);
    }

    // add back XR Grab Interactables, rigidbodies, and unparent and remove from stack for each layer except the base
    public void Dismantle()
    {
        for (int i = layers.Count - 1; i >= 1; i--)
        {
            var layer = layers[i];
            layer.transform.SetParent(null);

            XRGrabInteractable xr = layer.GetComponent<XRGrabInteractable>();
            if (xr == null) xr = layer.gameObject.AddComponent<XRGrabInteractable>();
            xr.movementType = XRBaseInteractable.MovementType.VelocityTracking;

            xr.colliders.Clear();
            foreach (var col in layer.GetComponentsInChildren<Collider>())
                xr.colliders.Add(col);

            var rb = layer.GetComponent<Rigidbody>() ?? layer.gameObject.AddComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic = false;
        }
        ResetStack();
    }
}
