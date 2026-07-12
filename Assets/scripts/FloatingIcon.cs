using UnityEngine;

public class FloatingIcon : MonoBehaviour
{
    [Header("Icon")]
    public Sprite icon;
    public float heightAboveObject = 0.3f;
    public float iconSize = 0.1f;

    [Header("Billboard")]
    public bool faceCamera = true;

    [Header("Bob")]
    public float bobHeight = 0.05f;
    public float bobSpeed = 2f;

    private GameObject iconObject;
    private SpriteRenderer spriteRenderer;
    private Transform cameraTransform;
    private float bobOffset;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        bobOffset = Random.Range(0f, Mathf.PI * 2f); // randomize phase so multiple icons don't sync

        iconObject = new GameObject("FloatingIcon");
        iconObject.transform.SetParent(transform);

        spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = icon;
        iconObject.transform.localScale = Vector3.one * iconSize;
    }

    void LateUpdate()
    {
        if (iconObject == null) return;

        float bob = Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
        iconObject.transform.position = transform.position + Vector3.up * (heightAboveObject + bob);
        iconObject.transform.localScale = Vector3.one * iconSize;

        if (!faceCamera || cameraTransform == null) return;

        iconObject.transform.rotation = Quaternion.LookRotation(
            cameraTransform.position - iconObject.transform.position
        );
    }

    public void SetIcon(Sprite newIcon)
    {
        icon = newIcon;
        if (spriteRenderer != null)
            spriteRenderer.sprite = newIcon;
    }

    public void SetVisible(bool visible)
    {
        if (iconObject != null)
            iconObject.SetActive(visible);
    }
}