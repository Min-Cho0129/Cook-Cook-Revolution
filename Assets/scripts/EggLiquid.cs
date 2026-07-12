using UnityEngine;
using System.Collections;

public class EggLiquid : MonoBehaviour
{
    public float spreadSpeed = 2f;
    public float maxScale = 1.8f;

    public AudioSource sizzleAudio;
    public float pitchMin = 0.95f;
    public float pitchMax = 1.03f;

    Vector3 originalScale;
    Rigidbody rb;

    bool isOnHotPan = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;

        StartCoroutine(Spread());
    }

    void Update()
    {
        if (sizzleAudio == null) return;

        if (isOnHotPan)
        {
            if (!sizzleAudio.isPlaying)
            {
                sizzleAudio.pitch = Random.Range(pitchMin, pitchMax);
                sizzleAudio.Play();
            }
        }
        else
        {
            if (sizzleAudio.isPlaying)
            {
                sizzleAudio.Stop();
            }
        }
    }

    public void SetOnHotPan(bool value)
    {
        isOnHotPan = value;
    }

    IEnumerator Spread()
    {
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * spreadSpeed;

            float smoothT = Mathf.SmoothStep(0, 1, t);

            Vector3 scale = Vector3.Lerp(originalScale, originalScale * maxScale, smoothT);

            transform.localScale = new Vector3(scale.x, originalScale.y, scale.z);

            yield return null;
        }

        /*
        // Attach the egg to the pan.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        */
    }
}
