using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderTicket : MonoBehaviour
{
    public float currentExpireTimer;
    public GameObject listItemPrefab;
    public GameObject orderInfoPrefab;
    public Recipe recipe;
    public Order order;

    private Vector3 initialPosition;
    private Vector3 initialScale;
    [SerializeField]
    private float ticketLerpSpeed = 0.065f;
    private bool isExpiring = false;
    public AudioSource audioSource;
    public AudioClip expireSound;
    public AudioClip completeSound;
    public AudioClip enterSound;

    private TextMeshProUGUI timerText;

    //public List<Material> ticketMaterials;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }
    void Awake()
    {
        initialPosition = transform.localPosition;
        transform.position -= new Vector3(0f, 0.5f, 0f);

        if (enterSound != null)
        {
            audioSource.clip = enterSound;
            audioSource.Play();
        }
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(!isExpiring) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, ticketLerpSpeed * Time.deltaTime * 60f);
            transform.localScale = Vector3.Lerp(transform.localScale, initialScale, ticketLerpSpeed * Time.deltaTime * 60f);
        }

        // format as 12.3s
        if(timerText != null)
            timerText.text = order.timeRemaining.ToString("F1") + "s";
    }

    public void setOrder(Order order)
    {
        this.order = order;
        SetRecipe(order.recipe);
    }

    public void SetRecipe(Recipe newRecipe)
    {
        Debug.Log($"OrderTicket: SetRecipe called for {newRecipe.recipeName}");

        recipe = newRecipe;
        currentExpireTimer = recipe.timeLimit;

        Canvas ticketCanvas = GetComponentInChildren<Canvas>();
        if (ticketCanvas != null)
        {
            VerticalLayoutGroup itemList = ticketCanvas.GetComponentInChildren<VerticalLayoutGroup>();
            VerticalLayoutGroup layout = itemList;
            layout.spacing = 12f;
            layout.padding.top = 10;
            layout.padding.bottom = 10;
            if (itemList != null && listItemPrefab != null)
            {
                foreach(var ingredient in recipe.requiredLayers.Reverse())
                {
                    GameObject item = Instantiate(listItemPrefab, itemList.transform);
                    TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                    Image icon = item.transform.Find("Icon").GetComponent<Image>();
                    Image extraIcon = item.transform.Find("Extra Icon").GetComponent<Image>();
                    
                    if (text != null)
                    {
                        text.text = ingredient.ingredientID; // TODO: replace with a nicer display name
                    }
                    if (icon != null && ingredient.ingredientIcon != null)
                    {
                        icon.sprite = ingredient.ingredientIcon;
                    }
                    if (extraIcon != null && ingredient.extraIcon != null)
                    {
                        extraIcon.sprite = ingredient.extraIcon;
                    }
                }
            }

            // instantiate order info prefab and set the timer text reference
            if (orderInfoPrefab != null)
            {
                GameObject info = Instantiate(orderInfoPrefab, itemList.transform);
                timerText = info.transform.Find("Expiry").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI valueText = info.transform.Find("Value").GetComponent<TextMeshProUGUI>();

                if(valueText != null)
                {
                    // display score value 100 as $10.00
                    valueText.text = "$" + (recipe.scoreValue / 10f).ToString("F2");
                }
            }
        }
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = Vector3.zero;
        initialScale = scale;
    }

    public void ExpireTicket()
    {
        if (expireSound != null)
        {
            audioSource.clip = expireSound;
            audioSource.Play();
        }
        isExpiring = true;
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        // tip the ticket forward
        rb.AddTorque(Vector3.right * 10f, ForceMode.Impulse);
        StartCoroutine(DestroyAfterDelay(2f));
    }

    public void CompleteTicket()
    {
        if (completeSound != null)
        {
            audioSource.clip = completeSound;
            audioSource.Play();
        }
        // play some kind of celebration effect here
        isExpiring = true;
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        // tip the ticket forward
        rb.AddTorque(Vector3.right * 10f, ForceMode.Impulse);
        StartCoroutine(DestroyAfterDelay(2f));
    }
}
