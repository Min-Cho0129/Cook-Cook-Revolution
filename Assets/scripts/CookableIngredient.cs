using TMPro;
using UnityEngine;

public class CookableIngredient : MonoBehaviour
{
    public float cookTime;
    private float randomizedCookTime;
    public GameObject nextStagePrefab;
    public bool nextStageIsBurnt = false;
    
    [SerializeField] float currentCookTime = 0;

    [SerializeField] TextMeshPro cookTimeText;
    public bool showCookTime = false;

    public void Start()
    {
        randomizedCookTime = Random.Range(cookTime * 0.8f, cookTime * 1.2f);

        if (showCookTime)
        {
            cookTimeText = new GameObject("CookTimeText").AddComponent<TextMeshPro>();
            cookTimeText.transform.SetParent(transform);
            cookTimeText.transform.localPosition = new Vector3(0, 1, 0);
            cookTimeText.alignment = TextAlignmentOptions.Center;
            cookTimeText.fontSize = 0.5f;
        }
    }

    void Update()
    {
        if(!showCookTime) return;

        cookTimeText.transform.position = transform.position + (0.15f * Vector3.up);
        cookTimeText.transform.forward = Camera.main.transform.forward;

        if(nextStageIsBurnt == false)
            cookTimeText.text = currentCookTime.ToString("F1") + "s/" + cookTime.ToString("F1") + "s";
        else
            cookTimeText.text = "Done!";
    }

    public bool Cook(float cookTimeDelta)
    {
        currentCookTime += cookTimeDelta;

        if (nextStagePrefab == null)
        {
            Debug.Log("Ingredient " + gameObject.name + " is fully cooked!");
            return false;
        }

        if (currentCookTime < cookTime)
            return false;
        
        Debug.Log("Cooked " + gameObject.name);

        Instantiate(nextStagePrefab, transform.position, transform.rotation);
        Destroy(gameObject);
        return true;
    }
}