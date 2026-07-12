using UnityEngine;

public class Egg : MonoBehaviour
{
    public float breakThreshold = 2.5f;
    public GameObject crackedPrefab;

    bool isCracked = false; 

    void OnCollisionEnter(Collision collision)
    {
        if (isCracked) return;

        if (collision.relativeVelocity.magnitude > breakThreshold)
        {
            Crack();
        }
    }

    void Crack()
    {
        if (isCracked) return; 
        isCracked = true;

        Instantiate(crackedPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}