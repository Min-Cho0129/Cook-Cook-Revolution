using TMPro;
using UnityEngine;

public class ChoppableIngredient : MonoBehaviour
{
    public int chopCountRequired = 3;
    public GameObject nextStagePrefab;
    

    [SerializeField] int chopCount = 0;

    [SerializeField] TextMeshPro chopCountText;

    public void Start()
    {
        chopCountText = new GameObject("ChopCountText").AddComponent<TextMeshPro>();
        chopCountText.transform.SetParent(transform);
        chopCountText.transform.localPosition = new Vector3(0, 1, 0);
        chopCountText.alignment = TextAlignmentOptions.Center;
        chopCountText.fontSize = 0.5f;
    }

    void Update()
    {
        if (chopCountText == null) return;
        chopCountText.transform.position = transform.position + (0.15f * Vector3.up);
        chopCountText.transform.forward = Camera.main.transform.forward;
        if (nextStagePrefab != null)
        {
            chopCountText.text = chopCount.ToString() + "/" + chopCountRequired.ToString();
        }
        else
        {
            chopCountText.text = "";
        }
    }

    public bool Chop()
    {
        // play chop particle system if there is one attached to this ingredient
        ParticleSystem particles = GetComponentInChildren<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
        }

        if (nextStagePrefab == null)
        {
            Debug.Log("Ingredient " + gameObject.name + " is fully chopped!");
            return false;
        }

        chopCount++;

        Debug.Log("Chopped " + gameObject.name);

        if (chopCount < chopCountRequired)
            return false;

        Instantiate(nextStagePrefab, transform.position, transform.rotation);
        Destroy(gameObject);
        return true;
    }
}