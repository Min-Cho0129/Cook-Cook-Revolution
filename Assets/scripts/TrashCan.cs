using UnityEngine;

public class TrashCan : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Unmovable")) return;
        if (other.CompareTag("Conveyor")) return;

        // check if FoodStack, ChoppableIngredient, StackableIngredient, or CookableIngredient is in the parents, if not destroy the object that entered
        if (other.GetComponentInParent<FoodStack>() || other.GetComponentInParent<ChoppableIngredient>() || other.GetComponentInParent<StackableIngredient>() || other.GetComponentInParent<CookableIngredient>())
        {
            Destroy(other.transform.root.gameObject);
            return;
        }
    }
}
