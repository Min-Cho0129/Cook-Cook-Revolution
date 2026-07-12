using TMPro;
using UnityEngine;

public class PhysicalTimer : MonoBehaviour
{

    public float monospaceWidth = 0.6f;
    private static float currentTimeInSeconds = 300f; // default to 5 minutes
    private TextMeshProUGUI tmp;
    private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tmp = GetComponentInChildren<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
        GameManager.Instance.OnGameEnded += OnGameOver;
        GameManager.Instance.OnGameStarted += OnGameStart;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTimeInSeconds < 0)
            currentTimeInSeconds = 0;

        int minutes = Mathf.FloorToInt(currentTimeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(currentTimeInSeconds % 60f);

        string colon = Time.time % 1f < 0.5f ? ":" : " ";
        tmp.text = string.Format("<mspace={0}em>{1:00}{2}{3:00}</mspace>", monospaceWidth, minutes, colon, seconds);
    }

    public static void SetTime(float timeInSeconds)
    {
        currentTimeInSeconds = timeInSeconds;
    }

    void OnGameStart()
    {
        // reset timer display
        currentTimeInSeconds = GameManager.Instance.gameDurationInSeconds;
        audioSource.Stop();
    }

    void OnGameOver()
    {
        // play sound
        audioSource.Play();
        // flash current time
        tmp.text = string.Format("<mspace={0}em>00:00</mspace>", monospaceWidth);
    }
}
