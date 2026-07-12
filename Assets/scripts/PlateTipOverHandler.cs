using UnityEngine;

public class PlateTipOverHandler : MonoBehaviour
{

    public float dotThreshold = 0.5f; // threshold for determining if the plate is tipped over
    private FoodStack stack;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stack = GetComponentInParent<FoodStack>();
    }

    // Update is called once per frame
    void Update()
    {
        if(stack == null) return;
        if(Vector3.Dot(transform.up, Vector3.up) < dotThreshold) {
            if(stack.layers.Count <= 1) return;

            print("PlateTipOverHandler: Plate tipped over, dismantling stack");
            stack.Dismantle();
        }
    }
}
