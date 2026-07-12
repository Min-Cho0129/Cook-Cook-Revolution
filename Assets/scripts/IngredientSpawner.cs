using System.Collections.Generic;
using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    [Header("Spawn Region")]
    public Vector3 spawnRegionSize = new Vector3(2f, 0f, 2f); // Y=0 keeps it flat
    public float spawnHeightOffset = 0.1f; // slight lift so items don't clip

    [Header("Spawning")]
    public GameObject ingredientPrefab;
    public int spawnAmount = 5;
    public float spawnInterval = 1f;

    float spawnTimer = 0;
    [SerializeField]
    List<GameObject> spawnedIngredients = new List<GameObject>();
    [SerializeField]

    List<GameObject> objectsInTrigger = new List<GameObject>();

    void Update()
    {
        //Debug.Log("countInstancesInTrigger(): " + countInstancesInTrigger());
        if (countInstancesInTrigger() < spawnAmount && spawnTimer >= spawnInterval)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject ingredient = Instantiate(ingredientPrefab, spawnPos, Quaternion.identity);
            spawnedIngredients.Add(ingredient);
            spawnTimer = 0;
        }

        if (spawnTimer < spawnInterval)
            spawnTimer += Time.deltaTime;
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 localOffset = new Vector3(
            Random.Range(-spawnRegionSize.x / 2f, spawnRegionSize.x / 2f),
            spawnHeightOffset,
            Random.Range(-spawnRegionSize.z / 2f, spawnRegionSize.z / 2f)
        );
        return transform.position + localOffset;
    }

    int countInstancesInTrigger()
    {
        int count = 0;
        foreach (GameObject ingredient in spawnedIngredients)
            if (objectsInTrigger.Contains(ingredient)) count++;
        return count;
    }

    void OnTriggerEnter(Collider other)
    {
        // Walk up to find the spawned root
        GameObject root = GetSpawnedRoot(other.gameObject);
        if (root != null && !objectsInTrigger.Contains(root))
            objectsInTrigger.Add(root);
    }

    void OnTriggerExit(Collider other)
    {
        GameObject root = GetSpawnedRoot(other.gameObject);
        if (root != null)
            objectsInTrigger.Remove(root);
    }

    GameObject GetSpawnedRoot(GameObject obj)
    {
        // Walk up the hierarchy until we find a spawnedIngredients entry
        Transform t = obj.transform;
        while (t != null)
        {
            if (spawnedIngredients.Contains(t.gameObject))
                return t.gameObject;
            t = t.parent;
        }
        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.25f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.up * spawnHeightOffset, spawnRegionSize + Vector3.up * 0.05f);

        Gizmos.color = new Color(0f, 1f, 0.4f, 0.9f);
        Gizmos.DrawWireCube(Vector3.up * spawnHeightOffset, spawnRegionSize + Vector3.up * 0.05f);
    }
}