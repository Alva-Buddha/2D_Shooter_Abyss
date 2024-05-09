using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hunter : MonoBehaviour
{
    private Flocking Flocking;
    private Avoidance Avoidance;
    private SpaceLimits SpaceLimits;
    private Attraction Attraction;

    [Header("Settings")]
    [Tooltip("The min speed at which the OtherDark turns")]
    public float degreeMax = 90.0f;
    [Tooltip("The max speed at which the OtherDark moves.")]
    public float moveSpeedBase = 6.0f;
    [Tooltip("The score value for defeating this OtherDark")]
    public int scoreValue = 5;
    [Tooltip("Weight for velocity inertia commponent")]
    public float inertiaFactor = 1f;
    [Tooltip("Weight for random velocity commponent")]
    public float randomFactor = 0.1f;
    [Tooltip("Prefab to replace when destroyed")]
    public GameObject convertPrefab = null;


    [Header("Behavior Settings")]
    [Tooltip("Enable or disable avoidance behavior.")]
    public bool useAvoidance = true;
    [Tooltip("Enable or disable flocking behavior.")]
    public bool useFlocking = true;
    [Tooltip("Enable or disable space limits to redirect object")]
    public bool useSpaceLimits = true;
    [Tooltip("Enable or disable attraction behavior.")]
    public bool useAttract = true;

    [Header("Damage Heal Settings")]
    [Tooltip("Should damage to specific enemies heal?")]
    public bool HealonDamage = true;
    [Tooltip("Amount of damage to be healed")]
    public float HealAmount = 5.0f;
    [Tooltip("Player gameobject for healing")]
    public GameObject PlayerObject = null;


    [Header("Update settings")]
    [Tooltip("Other bar")]
    public Slider otherBar = null;


    // Start is called before the first frame update
    void Start()
    {
        Flocking = GetComponent<Flocking>();
        Avoidance = GetComponent<Avoidance>();
        SpaceLimits = GetComponent<SpaceLimits>();
        Attraction = GetComponent<Attraction>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        MoveBehaviour();
    }

    /// <summary>
    /// Handles moving in accordance with the OtherDark's set behaviour
    /// </summary>
    private void MoveBehaviour()
    {
        Vector3 moveDirection = Vector3.zero;
        Vector3 avoidDirection = Vector3.zero;
        Vector3 angleAvoidDirection = Vector3.zero;

        // Check if the target is in range, then move
        if (useAvoidance && Avoidance != null)
        {
            avoidDirection += Avoidance.GetAvoidanceVector();
            angleAvoidDirection += Avoidance.GetAngleAvoidanceVector(avoidDirection);
            //Debug.Log("Avoid Target x:" + avoidDirection.x + " and y:" + avoidDirection.y);
        }
        if (useSpaceLimits && SpaceLimits != null)
        {
            moveDirection += SpaceLimits.GetBounceVector();
        }
        if (useFlocking && Flocking != null)
        {
            moveDirection += Flocking.GetFlockVector();
        }
        if (useAttract && Attraction != null)
        {
            moveDirection += Attraction.GetAttractVector();
            //Debug.Log("Attract Target x:" + attractDirection.x + " and y:" + attractDirection.y);
        }

        moveDirection += ((inertiaFactor * this.transform.forward) + RandomMove());
        //Debug.Log("Final (incl. inertia) x:" + moveDirection.x + " and y:" + moveDirection.y);
        Vector3 movement = (moveSpeedBase * Time.deltaTime * moveDirection.normalized)
            + (moveSpeedBase * Time.deltaTime * avoidDirection.normalized)
            + (moveSpeedBase * Time.deltaTime * angleAvoidDirection.normalized);
        movement.z = 0;
        transform.position += movement;
        RotateTowardsMovement(movement, degreeMax);
    }

    void OnDestroy()
    {
        ObjectSpawner.SpawnedObjects.Remove(this.gameObject);
    }

    /// <summary>
    /// This is meant to be called before destroying the gameobject associated with this script
    /// It can not be replaced with OnDestroy() because of Unity's inability to distiguish between unloading a scene
    /// and destroying the gameobject from the Destroy function
    /// </summary>
    public void DoBeforeDestroy()
    {
        //Debug.Log("instantiating otherlight");
        GameObject OtherLightObject = Instantiate(convertPrefab, this.transform.position, this.transform.rotation, null);
        OtherLightObject.transform.SetParent(this.transform.parent, true);
        OtherLight OtherLight = OtherLightObject.GetComponent<OtherLight>();
        if (otherBar != null)
        {
            otherBar.value += 1;
        }
        AddToScore();
        IncrementEnemiesDefeated();
        //Debug.Log("OtherDark being destroyed");
        if (HealonDamage)
        {
            PlayerObject.GetComponent<Health>().ReceiveHealing(HealAmount);
        }
    }

    /// <summary>
    /// Adds to the game manager's score the score associated with this OtherDark if one exists
    /// </summary>
    private void AddToScore()
    {
        if (GameManager.instance != null && !GameManager.instance.gameIsOver)
        {
            GameManager.AddScore(scoreValue);
        }
    }

    /// <summary>
    /// Description: Increments the game manager's number of defeated enemies
    /// </summary>
    private void IncrementEnemiesDefeated()
    {
        if (GameManager.instance != null && !GameManager.instance.gameIsOver)
        {
            GameManager.instance.IncrementEnemiesDefeated();
        }
    }

    /// <summary>
    /// Rotates the OtherDark to instantly face the direction it is moving.
    /// </summary>
    /// <param name="moveDirection">The direction of movement.</param>
    private void RotateTowardsMovement(Vector3 moveDirectionR, float degreeMax)
    {
        if (moveDirectionR != Vector3.zero) // Prevent rotation towards zero vector
        {
            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirectionR);
            transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, degreeMax * Time.deltaTime);
        }
    }

    private Vector3 RandomMove()
    {
        Vector3 randomVector = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
        return randomFactor * randomVector;
    }
}
