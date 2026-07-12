using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Order
{
    public Recipe recipe;
    public float timeRemaining;
    public float timeLimit;
}

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance;

    public List<Order> activeOrders = new();
    public List<Recipe> recipePool;
    public int maxActiveOrders = 3;

    // keep activeRecipes as a convenience property for MatchesRecipe lookups
    public List<Recipe> activeRecipes
    {
        get
        {
            var list = new List<Recipe>();
            foreach (var o in activeOrders) list.Add(o.recipe);
            return list;
        }
    }

    public event System.Action<Order> OnOrderAdded;
    public event System.Action<Order> OnOrderCompleted;
    public event System.Action<Order> OnOrderExpired;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

    void Update()
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            activeOrders[i].timeRemaining -= Time.deltaTime;
            if (activeOrders[i].timeRemaining <= 0f)
                ExpireOrder(activeOrders[i]);
        }
    }

    public void SpawnOrder()
    {
        if (activeOrders.Count >= maxActiveOrders) return;
        if (recipePool == null || recipePool.Count == 0) return;

        var recipe = recipePool[Random.Range(0, recipePool.Count)];
        var order = new Order { recipe = recipe, timeLimit = recipe.timeLimit, timeRemaining = recipe.timeLimit };
        activeOrders.Add(order);
        OnOrderAdded?.Invoke(order);
    }

    public void CompleteOrder(Recipe recipe)
    {
        // find the order with the soonest expiry that matches this recipe
        Order soonest = null;
        foreach (var o in activeOrders)
        {
            if (o.recipe != recipe) continue;
            if (soonest == null || o.timeRemaining < soonest.timeRemaining)
                soonest = o;
        }
        if (soonest == null) return;
        activeOrders.Remove(soonest);
        OnOrderCompleted?.Invoke(soonest);
    }

    void ExpireOrder(Order order)
    {
        if (!activeOrders.Contains(order)) return;
        activeOrders.Remove(order);
        OnOrderExpired?.Invoke(order);
    }
}