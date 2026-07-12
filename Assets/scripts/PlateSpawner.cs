using System.Collections.Generic;
using UnityEngine;

public class PlateSpawner : MonoBehaviour
{
    public GameObject platePrefab;
    public GameObject spawnedPlate;
    public float spawnInterval = 5f;
    private float spawnTimer = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // spawn every spawnInterval if no plate inside trigger
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            if (spawnedPlate == null)
            {
                spawnTimer = 0f;
                spawnedPlate = Instantiate(platePrefab, transform.position, Quaternion.identity);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        
    }

    void OnTriggerExit(Collider other)
    {
        if (spawnedPlate == null || other == null) return;

        Transform plateTransform = spawnedPlate.transform;
        if (other.gameObject == spawnedPlate || other.transform == plateTransform || other.transform.IsChildOf(plateTransform))
        {
            spawnedPlate = null;
        }
    }
}
