using System.Collections;
using UnityEngine;

public class EggCracked : MonoBehaviour
{
    public GameObject eggLiquidPrefab;
    public Transform spawnPoint;

    void Start()
    {
        Instantiate(eggLiquidPrefab, spawnPoint.position, Quaternion.identity);
        StartCoroutine(DestroyAfterDelay());
    }

    // destroy eggshell after 3 seconds to clean up the scene
    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}