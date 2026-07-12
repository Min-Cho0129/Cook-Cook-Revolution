using UnityEngine;

[CreateAssetMenu(fileName = "TicketIngredient", menuName = "VR Cooking/Ticket Ingredient")]
public class TicketIngredient : ScriptableObject
{
    // StackableIngredient id used to match with the ingredient
    public string ingredientID;
    // icon to display on ticket for this ingredient
    public Sprite ingredientIcon;
    // icon to indicate if ingredient needs to be prepared specially (cooked, chopped, etc.)
    public Sprite extraIcon;
}
