using UnityEngine;

public class HotPanSizzle : MonoBehaviour
{
    public AudioSource sizzleAudio;
    public float pitchMin = 0.95f;
    public float pitchMax = 1.03f;

    bool isOnHotPan;

    void Awake()
    {
        if (sizzleAudio == null)
            sizzleAudio = GetComponentInChildren<AudioSource>();

        if (sizzleAudio != null)
            sizzleAudio.playOnAwake = false;
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
        else if (sizzleAudio.isPlaying)
        {
            sizzleAudio.Stop();
        }
    }

    public void SetOnHotPan(bool value)
    {
        isOnHotPan = value;
    }

    void OnDisable()
    {
        isOnHotPan = false;

        if (sizzleAudio != null && sizzleAudio.isPlaying)
            sizzleAudio.Stop();
    }
}
