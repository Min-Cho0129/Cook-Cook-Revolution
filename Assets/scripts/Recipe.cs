using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "VR Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public TicketIngredient[] requiredLayers;
    public int scoreValue = 100;
    public float timeLimit = 60f;
}
