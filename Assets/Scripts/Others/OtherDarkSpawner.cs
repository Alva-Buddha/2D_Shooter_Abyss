using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which spawns enemies in an area around it.
/// </summary>
public class OtherDarkSpawner : MonoBehaviour
{
    [Header("GameObject References")]
    [Tooltip("The OtherDark prefab to use when spawning enemies")]
    public GameObject OtherDarkPrefab = null;
    [Tooltip("The target of the spwaned enemies")]
    public Transform target = null;

    [Header("Spawn Position")]
    [Tooltip("The distance within which enemies can spawn in the X direction")]
    [Min(0)]
    public float spawnRangeXmax = 10.0f;
    public float spawnRangeXmin = 5.0f;
    [Tooltip("The distance within which enemies can spawn in the Y direction")]
    [Min(0)]
    public float spawnRangeYmax = 10.0f;
    public float spawnRangeYmin = 5.0f;

    [Header("Spawn Variables")]
    [Tooltip("The maximum number of enemies that can be spawned from this spawner")]
    public int maxSpawn = 20;
    [Tooltip("Ignores the max spawn limit if true")]
    public bool spawnInfinite = true;

    // The number of enemies that have been spawned
    private int currentlySpawned = 0;

    [Tooltip("The time delay between spawning enemies")]
    public float spawnDelay = 2.5f;

    // The most recent spawn time
    private float lastSpawnTime = Mathf.NegativeInfinity;

    [Tooltip("The object to make projectiles child objects of.")]
    public Transform projectileHolder = null;

    /// <summary>
    /// Description:
    /// Standard Unity function called every frame
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void Update()
    {
        CheckSpawnTimer();
    }

    /// <summary>
    /// Description:
    /// Checks if it is time to spawn an OtherDark
    /// Spawns an OtherDark if it is time
    /// Inputs: 
    /// none
    /// Returns: 
    /// void (no return)
    /// </summary>
    private void CheckSpawnTimer()
    {
        // If it is time for an OtherDark to be spawned
        if (Time.timeSinceLevelLoad > lastSpawnTime + spawnDelay && (currentlySpawned < maxSpawn || spawnInfinite))
        {
            // Determine spawn location
            Vector3 spawnLocation = GetSpawnLocation();

            // Spawn an OtherDark
            SpawnOtherDark(spawnLocation);
        }
    }

    /// <summary>
    /// Description:
    /// Spawn and set up an instance of the OtherDark prefab
    /// Inputs: 
    /// Vector3 spawnLocation
    /// Returns: 
    /// void (no return)
    /// </summary>
    /// <param name="spawnLocation">The location to spawn an enmy at</param>
    private void SpawnOtherDark(Vector3 spawnLocation)
    {
        // Make sure the prefab is valid
        if (OtherDarkPrefab != null)
        {
            // Create the OtherDark gameobject
            GameObject OtherDarkGameObject = Instantiate(OtherDarkPrefab, spawnLocation, OtherDarkPrefab.transform.rotation, null);
            OtherDark OtherDark = OtherDarkGameObject.GetComponent<OtherDark>();

            // Setup the OtherDark if necessary
            if (OtherDark != null)
            {
                OtherDark.followTarget = target;
            }
  

            // Incremment the spawn count
            currentlySpawned++;
            lastSpawnTime = Time.timeSinceLevelLoad;
        }
    }

    /// <summary>
    /// Description:
    /// Returns a generated spawn location for an OtherDark
    /// Inputs: 
    /// none
    /// Returns: 
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The spawn location as determined by the function</returns>
    protected virtual Vector3 GetSpawnLocation()
    {
        // Get random coordinates
        int signX = Random.value < 0.5 ? -1 : 1;
        int signY = Random.value < 0.5 ? -1 : 1;
        float x = signX * Random.Range(spawnRangeXmin, spawnRangeXmax);
        float y = signY * Random.Range(spawnRangeYmin, spawnRangeYmax);
        // Return the coordinates as a vector
        return new Vector3(transform.position.x + x, transform.position.y + y, 0);
    }
}
