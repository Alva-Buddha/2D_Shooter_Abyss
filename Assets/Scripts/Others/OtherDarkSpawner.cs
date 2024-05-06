using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    [Tooltip("Other bar")]
    public Slider otherBar = null;

    [Header("Spawn Position & Orientation")]
    [Tooltip("The distance within which enemies can spawn in the X direction")]
    [Min(0)]
    public float spawnRangeXmax = 10.0f;
    public float spawnRangeXmin = 5.0f;
    [Tooltip("The distance within which enemies can spawn in the Y direction")]
    [Min(0)]
    public float spawnRangeYmax = 10.0f;
    public float spawnRangeYmin = 5.0f;
    [Tooltip("Random angle range of spawns (0 = all spawns orient towards player)")]
    public float angleRange = 45.0f;

    [Header("Spawn Variables")]
    [Tooltip("The maximum number of enemies that can be spawned from this spawner")]
    public int maxSpawn = 20;
    [Tooltip("Ignores the max spawn limit if true")]
    public bool spawnInfinite = true;

    // The number of enemies that have been spawned
    public int currentlySpawned = 0;

    [Tooltip("The time delay between spawning enemies")]
    public float spawnDelay = 2.5f;

    // The most recent spawn time
    private float lastSpawnTime = Mathf.NegativeInfinity;

    [Tooltip("The object to make OtherDarks child objects of.")]
    public Transform OtherDarkHolder = null;

    private void Start()
    {
        if (otherBar != null)
        {
            otherBar.maxValue = maxSpawn;
        }
    }

    private void Update()
    {
        CheckSpawnTimer();
    }

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
    /// Spawn and set up an instance of the OtherDark prefab
    /// Inputs: Vector3 spawnLocation
    /// Returns: void (no return)
    /// </summary>
    /// <param name="spawnLocation">The location to spawn an enmy at</param>
    private void SpawnOtherDark(Vector3 spawnLocation)
    {
        // Make sure the prefab is valid
        if (OtherDarkPrefab != null)
        {
            Vector3 lookatorigin = -spawnLocation;

            // Create the OtherDark gameobject
            GameObject OtherDarkGameObject = Instantiate(OtherDarkPrefab, spawnLocation, 
                Quaternion.LookRotation(lookatorigin)*
                Quaternion.Euler(angleRange * Random.Range(-1f, 1f), 
                angleRange * Random.Range(-1f, 1f), 
                angleRange * Random.Range(-1f, 1f)), null);

            if (OtherDarkHolder != null)
            {
                OtherDarkGameObject.transform.SetParent(OtherDarkHolder);
            }

            OtherDarkGameObject.GetComponent<OtherDark>().PlayerObject = GameObject.FindWithTag("Player");
            OtherDarkGameObject.GetComponent<OtherDark>().otherBar = otherBar;

            // Incremment the spawn count
            currentlySpawned++;
            lastSpawnTime = Time.timeSinceLevelLoad;
        }
    }

    /// <summary>
    /// Returns a generated spawn location for an OtherDark
    /// Inputs: none
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
