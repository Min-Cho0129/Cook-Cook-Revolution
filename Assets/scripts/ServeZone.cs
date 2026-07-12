using System.Collections;
using UnityEngine;

public class ServeZone : MonoBehaviour
{
    [Header("Feedback")]
    public float rejectDelay = 0.5f;
    public float rejectEjectForceMagnitude = 2f;

    public event System.Action<Recipe> OnCorrectServe;
    public event System.Action OnWrongServe;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    void OnTriggerEnter(Collider other)
    {
        var stack = other.GetComponentInParent<FoodStack>();
        if (other.CompareTag("Unmovable")) return;
        if (stack == null)
        {
            Destroy(other.transform.root.gameObject);
            return;
        }

        //print("ServeZone: Matching stack found for " + other.name);

        foreach (var recipe in RecipeManager.Instance.activeRecipes)
        {
            if (!stack.MatchesRecipe(recipe))
            {
                //print("ServeZone: Recipe " + recipe.recipeName + " does not match stack " + stack.name);
                continue;
            }

            //print("ServeZone: Matching recipe found for " + stack.name);
            RecipeManager.Instance.CompleteOrder(recipe);
            OnCorrectServe?.Invoke(recipe);
            Destroy(stack.gameObject);
            return;
        }

        OnWrongServe?.Invoke();
        audioSource.Play();
        //StartCoroutine(RejectPlate(stack.gameObject));
        Destroy(stack.gameObject);
    }

    IEnumerator RejectPlate(GameObject plate)
    {
        yield return new WaitForSeconds(rejectDelay);

        if (plate == null) yield break;

        // find children under nested attatch points, add rigidbody and disable colliders for each and eject in random directions
        FoodStack stack = plate.GetComponent<FoodStack>();
        if (stack != null)
        {
            foreach (var layer in stack.layers)
            {
                // skip plate itself
                if(layer.isBase) continue;

                // unparent each stack layer so they can be ejected separately
                layer.transform.SetParent(null);
                var rb = layer.GetComponent<Rigidbody>();
                if(rb == null)
                {
                    rb = layer.gameObject.AddComponent<Rigidbody>();
                }
                var colliders = layer.GetComponentsInChildren<Collider>();
                foreach (var col in colliders) col.enabled = false;
                rb.AddForce(new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f)) * rejectEjectForceMagnitude, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * rejectEjectForceMagnitude * 5, ForceMode.Impulse);

                // destroy ingredient after ejecting
                StartCoroutine(DestroyAfterDelay(layer.gameObject, 2f));
            }

            stack.layers.Clear();
            stack.AddLayer(stack.topLayer); // add back the plate as the only layer so it can be reused
        }
    }

    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null) Destroy(obj);
    }
}
