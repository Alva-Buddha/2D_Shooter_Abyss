using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A class which spawns enemies in an area around it.
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    [Header("GameObject References")]
    [Tooltip("The SpawnObject prefab to use when spawning enemies")]
    public GameObject SpawnObjectPrefab = null;
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

    [Tooltip("The object to make SpawnObjects child objects of.")]
    public Transform SpawnObjectHolder = null;

    //Variables to add message for first spawn
    [Header("Message associated with 1st spawn")]
    [Tooltip("Should a message be sent on 1st spawn")]
    public bool firstSpawnMessage = true;
    [TextArea]
    [Tooltip("Message to be sent on 1st spawn")]
    public string firstSpawnMessageText = null;

    //flag to check if first spawn has been happened
    private bool firstSpawnFlag = false;

    //GameObject to display message
    public GameObject messageObject = null;

    //List of all spawned objects
    public List<GameObject> SpawnedObjects = new List<GameObject>();


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
        // If it is time for an SpawnObject to be spawned
        if (Time.timeSinceLevelLoad > lastSpawnTime + spawnDelay && (SpawnedObjects.Count < maxSpawn || spawnInfinite))
        {
            // Determine spawn location
            Vector3 spawnLocation = GetSpawnLocation();

            // Spawn an SpawnObject
            SpawnSpawnObject(spawnLocation);

            //Debug.Log(SpawnedObjects.Count + " objects spawned.");
        }
    }

    /// <summary>
    /// Spawn and set up an instance of the SpawnObject prefab
    /// Inputs: Vector3 spawnLocation
    /// Returns: void (no return)
    /// </summary>
    /// <param name="spawnLocation">The location to spawn an enmy at</param>
    private void SpawnSpawnObject(Vector3 spawnLocation)
    {
        // Make sure the prefab is valid
        if (SpawnObjectPrefab != null)
        {
            Vector3 lookatorigin = -spawnLocation;

            // Create the SpawnObject gameobject
            GameObject SpawnObjectGameObject = Instantiate(SpawnObjectPrefab, spawnLocation, 
                Quaternion.LookRotation(lookatorigin)*
                Quaternion.Euler(angleRange * Random.Range(-1f, 1f), 
                angleRange * Random.Range(-1f, 1f), 
                angleRange * Random.Range(-1f, 1f)), null);

            SpawnedObjects.Add(SpawnObjectGameObject);

            if (SpawnObjectHolder != null)
            {
                SpawnObjectGameObject.transform.SetParent(SpawnObjectHolder);
            }
            
            if(SpawnObjectGameObject.GetComponent<OtherDark>() != null)
            {
                SpawnObjectGameObject.GetComponent<OtherDark>().PlayerObject = GameObject.FindWithTag("Player");
                SpawnObjectGameObject.GetComponent<OtherDark>().otherBar = otherBar;
                SpawnObjectGameObject.GetComponent<OtherDark>().Spawner = this;
            }
            if (SpawnObjectGameObject.GetComponent<Hunter>() != null)
            {
                SpawnObjectGameObject.GetComponent<Hunter>().PlayerObject = GameObject.FindWithTag("Player");
                SpawnObjectGameObject.GetComponent<Hunter>().otherBar = otherBar;
                SpawnObjectGameObject.GetComponent<Hunter>().Spawner = this;    
            }

            // Incremment the spawn count
            currentlySpawned++;
            lastSpawnTime = Time.timeSinceLevelLoad;

            // Send message on first spawn
            if (firstSpawnMessage && !firstSpawnFlag)
            {
                if (messageObject != null)
                {
                    MessageManager messageManager = messageObject.GetComponent<MessageManager>();
                    messageManager.AddMessage(firstSpawnMessageText);
                    firstSpawnFlag = true;
                }
            }
        }
    }

    /// <summary>
    /// Returns a generated spawn location for an SpawnObject
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
