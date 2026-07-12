using UnityEngine;

public class Knife : MonoBehaviour
{
    public float chopVelocityThreshold = 1.0f;
    public string cuttingBoardTag = "Cutting Board";
    public AudioSource chopAudioSource;
    public AudioClip chopSound;
    public float chopSoundVolume = 1f;
    public float chopSoundPitchMin = 0.95f;
    public float chopSoundPitchMax = 1.05f;
    public float minTimeBetweenChopSounds = 0.05f;
    Collider knifeCollider;
    Rigidbody rb;
    Vector3 previousPosition;
    float lastChopSoundTime = float.NegativeInfinity;
    [SerializeField]
    Vector3 velocity;
    [SerializeField]
    GameObject currentIngredient;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        knifeCollider = GetComponent<Collider>();
        if (chopAudioSource == null)
            chopAudioSource = GetComponent<AudioSource>();

        if (chopAudioSource == null)
            chopAudioSource = gameObject.AddComponent<AudioSource>();

        chopAudioSource.playOnAwake = false;
        chopAudioSource.spatialBlend = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        velocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;

        //print("Knife velocity: " + velocity.magnitude);

        // turn off collision if knife is fast enough
        if(velocity.magnitude >= chopVelocityThreshold)
        {
            knifeCollider.isTrigger = true;
        }
        else
        {
            // dont enable collision until knife exits ingredient
            if(currentIngredient == null)
            {
                knifeCollider.isTrigger = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(cuttingBoardTag))
        {
            if(currentIngredient != null)
            {
                ChoppableIngredient choppable = currentIngredient.GetComponentInParent<ChoppableIngredient>();
                if (choppable == null) return;

                PlayChopSound();

                // moved on to next stage if true
                if (choppable.Chop())
                {
                    currentIngredient = null;
                }
            }
        }
        else
        {
            if(other.gameObject.GetComponentInParent<ChoppableIngredient>() != null)
            {
                currentIngredient = other.gameObject;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject == currentIngredient)
        {
            currentIngredient = null;
        }
    }

    void PlayChopSound()
    {
        if (chopAudioSource == null || chopSound == null) return;
        if (Time.time - lastChopSoundTime < minTimeBetweenChopSounds) return;

        chopAudioSource.pitch = Random.Range(chopSoundPitchMin, chopSoundPitchMax);
        chopAudioSource.PlayOneShot(chopSound, chopSoundVolume);
        lastChopSoundTime = Time.time;
    }
}
