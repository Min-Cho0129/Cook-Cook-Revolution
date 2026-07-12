using System;
using UnityEngine;

public class BGMPlayerManager : MonoBehaviour
{
    public float maxMusicSpeed = 1.5f;
    AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GameManager.Instance.OnGameStarted += OnGameStarted;
        GameManager.Instance.OnGameEnded += OnGameEnded;
    }

    // Update is called once per frame
    void Update()
    {
        // make the pitch increase from 1 to maxMusicSpeed after the remaining time hits 10 seconds
        // otherwise pitch is 1
        float timeRemaining = GameManager.Instance.GetTimeRemaining();
        if (timeRemaining < 10f)
        {
            float t = 1f - (timeRemaining / 10f);
            audioSource.pitch = Mathf.Lerp(1f, maxMusicSpeed, t);
        }
        else
        {
            audioSource.pitch = 1f;
        }
    }

    private void OnGameStarted()
    {
        // Play background music
        audioSource.Play();
    }

    private void OnGameEnded()
    {
        // Stop background music
        audioSource.Stop();
    }
}
