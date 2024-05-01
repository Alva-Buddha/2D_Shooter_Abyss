using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A class which controls OtherDark behaviour
/// </summary>
public class OtherLight : MonoBehaviour
{
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


    [Header("Avoiding Settings")]
    [Tooltip("The transform of the object that this OtherDark should avoid.")]
    public Transform avoidTarget = null;
    [Tooltip("The target distance OtherDark seek to maintain from the avoidTarget")]
    public float avoidTargetRange = 3.0f;

    [Header("Flocking Settings")]
    [Tooltip("Enable or disable flocking behavior.")]
    public bool useFlocking = true;
    [Tooltip("The radius to detect neighboring boids.")]
    public float neighborRadius = 3.0f;
    [Tooltip("The radius to maintain distance from neighboring boids for separation.")]
    public float avoidOtherRadius = 2.0f;
    [Tooltip("Weight for alignment movement")]
    public float alignmentWeight = 2.0f;
    [Tooltip("Weight for separation movement")]
    public float seperationWeight = 1.0f;
    [Tooltip("Weight for cohesion movement")]
    public float cohesionWeight = 0.5f;


    private List<Transform> neighbors;
    public LayerMask neighborLayerMask; // Define which layer boids are on for neighbor detection
    public float neighborUpdateInterval = 0.2f; // How often to update neighbors list in seconds
    private float nextUpdateTime = 0.1f;
    private Vector3 flockVectorLocal = Vector3.zero;

    /// <summary>
    /// Enum to help wih different movement modes
    /// </summary>
    public enum MovementModes { NoMovement, avoidTarget };

    [Tooltip("The way this OtherDark will move\n" +
        "NoMovement: This OtherDark will not move.\n" +
        "avoidTarget: This OtherDark will Avoid the assigned target.\n" +
        "Scroll: This OtherDark will move in one horizontal direction only.")]
    public MovementModes movementMode = MovementModes.avoidTarget;

    /// <summary>
    /// Standard Unity function called once before the first call to Update
    /// </summary>
    private void Start()
    {
        neighbors = new List<Transform>();
        nextUpdateTime = Time.time + neighborUpdateInterval;
    }

    /// <summary>
    /// Default update method
    /// </summary>
    private void Update()
    {
        // Draw the global forward direction from the object's position
        //Vector3 globalForward = transform.TransformDirection(Vector3.forward) * 5; 
        //Debug.DrawLine(transform.position, transform.position + globalForward, Color.blue);
    }

    /// <summary>
    /// Standard Unity function called after update every frame
    /// </summary>
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
        
        // Check if the target is in range, then move
        if (avoidTarget != null)
        {
            moveDirection += GetAvoidTargetVector();
            //Debug.Log("Avoid Target x:" + moveDirection.x + " and y:" + moveDirection.y);
        }
        if (useFlocking)
        {
            if (Time.time > nextUpdateTime)
            {
                UpdateNeighbors();
                nextUpdateTime = Time.time + neighborUpdateInterval;
                flockVectorLocal = GetFlockVector();
            }
            moveDirection += flockVectorLocal;
        }
        //Debug.Log("Flock x:" + flockVectorlocaL.x + " and y:" + moveDirection.y);
        moveDirection += ((inertiaFactor * this.transform.forward)+RandomMove());
        //Debug.Log("Final (incl. inertia) x:" + moveDirection.x + " and y:" + moveDirection.y);
        Vector3 movement = (moveSpeedBase * Time.deltaTime * moveDirection.normalized);
        movement.z = 0;
        transform.position += movement;
        RotateTowardsMovement(movement, degreeMax);
    }

    /// <summary>
    /// Combines the flocking behaviors to determine the enemy's next move.
    /// </summary>
    private Vector3 GetFlockVector()
    {
        CleanUpNeighbors();
        
        Vector3 alignment = Alignment();
        Vector3 cohesion = Cohesion();
        Vector3 separation = Separation();

        // Combine the behaviors with possibly different weights
        Vector3 flockVector = (alignment * alignmentWeight) + (cohesion * cohesionWeight) + (separation * seperationWeight);
        return flockVector;
    }

    /// <summary>
    /// Calculates the average direction of nearby boids for alignment.
    /// </summary>
    /// <returns>Vector3 representing the average heading of nearby boids.</returns>
    private Vector3 Alignment()
    {
        Vector3 alignVector = Vector3.zero;
        foreach (Transform neighbor in neighbors)
        {
            alignVector += neighbor.forward;  // Assuming forward is the moving direction
        }
        if (neighbors.Count > 0)
        {
            alignVector /= neighbors.Count;
        }
        return alignVector;
    }

    /// <summary>
    /// Calculates the center of mass of nearby boids for cohesion.
    /// </summary>
    /// <returns>Vector3 pointing towards the center of mass.</returns>
    private Vector3 Cohesion()
    {
        Vector3 cohesionVector = Vector3.zero;
        foreach (Transform neighbor in neighbors)
        {
            cohesionVector += neighbor.position;
        }
        if (neighbors.Count > 0)
        {
            cohesionVector /= neighbors.Count;
            cohesionVector -= transform.position;
        }
        return cohesionVector;
    }

    /// <summary>
    /// Calculates a vector to keep distance from nearby boids for separation.
    /// </summary>
    /// <returns>Vector3 to move away from nearby boids.</returns>
    private Vector3 Separation()
    {
        Vector3 separationVector = Vector3.zero;
        foreach (Transform neighbor in neighbors)
        {
            float proximity = (neighbor.position - transform.position).magnitude;
            if (proximity < avoidOtherRadius)
            {
                separationVector -= (neighbor.position - transform.position)/Mathf.Max(proximity*proximity,0.0001f);
            }
        }

        return separationVector;
    }

    /// <summary>
    /// Updates the list of nearby boids to be considered for flocking.
    /// </summary>
    private void UpdateNeighbors()
    {
        neighbors.Clear(); // Clear the existing list
        Collider[] colliders = Physics.OverlapSphere(transform.position, neighborRadius, neighborLayerMask);
        //Debug.Log("neighbor count:" + colliders.Length);
        foreach (Collider collider in colliders)
        {
            if (collider.transform != transform) // Avoid adding self
            {
                neighbors.Add(collider.transform);
            }
        }
    }

    /// <summary>
    /// Clears any null or destroyed objects from the neighbors list.
    /// </summary>
    private void CleanUpNeighbors()
    {
        neighbors.RemoveAll(item => item == null || item.gameObject == null);
    }


    /// <summary>
    /// This is meant to be called before destroying the gameobject associated with this script
    /// It can not be replaced with OnDestroy() because of Unity's inability to distiguish between unloading a scene
    /// and destroying the gameobject from the Destroy function
    /// </summary>
    public void DoBeforeDestroy()
    {
        //Debug.Log("instantiating otherlight");
        //GameObject OtherLightObject = Instantiate(convertPrefab, this.transform.position, this.transform.rotation, null);
        //OtherLightObject.transform.SetParent(this.transform.parent, true);
        //OtherLightObject.OtherDark.avoidTarget = avoidTarget;
        AddToScore();
        IncrementEnemiesDefeated();
        //Debug.Log("OtherDark being destroyed");
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
    /// The direction and magnitude of the OtherDark's desired movement in Avoid mode
    /// </summary>
    /// <returns>Vector3: The movement to be used in Avoid movement mode.</returns>
    private Vector3 GetAvoidTargetVector()
    {
        float avoidScale = ((avoidTarget.position - transform.position).magnitude - avoidTargetRange);
        if (avoidScale > 0)
        {
            avoidScale = 0.1f;
        }
        Vector3 avoidVector = (avoidTarget.position - transform.position).normalized * avoidScale;
        return avoidVector;
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
