using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;

    public int minCount = 10;
    public int maxCount = 20;

    public int count;

    public float spawnRangeX = 10f;
    public float spawnRangeZ = 10f;

    public bool useTerrainHeight = true;
    public float yOffset = 0f;

    void Awake()
    {
        count = Random.Range(minCount, maxCount + 1);

        ZombieAI.deathCount = 0;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomPosition();
            Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
        }
    }

    Vector3 GetRandomPosition()
    {
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);

        Vector3 position = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ + 5
        );
        if (useTerrainHeight && Terrain.activeTerrain != null)
        {
            position.y = Terrain.activeTerrain.SampleHeight(position)
                         + Terrain.activeTerrain.transform.position.y
                         + yOffset;
        }

        return position;
    }
}