using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which controls OtherDark behaviour
/// </summary>
public class OtherDark : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The speed at which the OtherDark moves.")]
    public float moveSpeed = 5.0f;
    [Tooltip("The score value for defeating this OtherDark")]
    public int scoreValue = 5;

    [Header("Avoiding Settings")]
    [Tooltip("The transform of the object that this OtherDark should Avoid.")]
    public Transform avoidTarget = null;
    [Tooltip("The distance at which the OtherDark begins Avoiding the Avoid target.")]
    public float avoidTargetRange = 5.0f;

    [Header("Flocking Settings")]
    [Tooltip("Enable or disable flocking behavior.")]
    public bool useFlocking = true;
    [Tooltip("The radius to detect neighboring boids.")]
    public float neighborRadius = 10.0f;
    [Tooltip("The radius to maintain distance from neighboring boids for separation.")]
    public float avoidOtherRadius = 1.0f;

    private List<Transform> neighbors;
    public LayerMask neighborLayerMask; // Define which layer boids are on for neighbor detection
    private float updateInterval = 0.1f; // How often to update neighbors list in seconds
    private float nextUpdateTime = 0;

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
        nextUpdateTime = Time.time + updateInterval;
    }

    /// <summary>
    /// Standard Unity function called after update every frame
    /// </summary>
    private void LateUpdate()
    {
        HandleBehaviour();
    }

    /// <summary>
    /// Handles moving in accordance with the OtherDark's set behaviour
    /// </summary>
    private void HandleBehaviour()
    {
        // Check if the target is in range, then move
        if (avoidTarget != null && (avoidTarget.position - transform.position).magnitude < avoidTargetRange)
        {
            avoidTargetMove();
        }
        if (useFlocking)
        {
            if (Time.time > nextUpdateTime)
            {
                UpdateNeighbors();
                nextUpdateTime = Time.time + updateInterval;
            }
            Flock();
        }
    }

    /// <summary>
    /// Combines the flocking behaviors to determine the enemy's next move.
    /// </summary>
    private void Flock()
    {
        CleanUpNeighbors();
        
        Vector3 alignment = Alignment();
        Vector3 cohesion = Cohesion();
        Vector3 separation = Separation();

        // Combine the behaviors with possibly different weights
        Vector3 moveDirection = alignment + cohesion + separation;
        Vector3 movement = moveDirection * Time.deltaTime * moveSpeed; 
        transform.position += movement;
        RotateTowardsMovement(movement);
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
        alignVector /= neighbors.Count;

        return alignVector.normalized;
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
        cohesionVector /= neighbors.Count;
        cohesionVector -= transform.position;

        return cohesionVector.normalized;
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
            if ((neighbor.position - transform.position).magnitude < avoidOtherRadius)
            {
                separationVector -= (neighbor.position - transform.position);
            }
        }

        return separationVector.normalized;
    }

    /// <summary>
    /// Updates the list of nearby boids to be considered for flocking.
    /// </summary>
    private void UpdateNeighbors()
    {
        neighbors.Clear(); // Clear the existing list
        Collider[] colliders = Physics.OverlapSphere(transform.position, neighborRadius, neighborLayerMask);
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
    /// Moves the OtherDark
    /// </summary>
    private void avoidTargetMove()
    {
        // Determine correct movement
        Vector3 movement = GetAvoidTargetMovement();

        // Move the OtherDark
        transform.position = transform.position + movement;
    }

    /// <summary>
    /// The direction and magnitude of the OtherDark's desired movement in Avoid mode
    /// </summary>
    /// <returns>Vector3: The movement to be used in Avoid movement mode.</returns>
    private Vector3 GetAvoidTargetMovement()
    {
        Vector3 moveDirection = -(avoidTarget.position - transform.position).normalized;
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
        RotateTowardsMovement(movement);
        return movement;
    }

    /// <summary>
    /// Rotates the OtherDark to instantly face the direction it is moving.
    /// </summary>
    /// <param name="moveDirection">The direction of movement.</param>
    private void RotateTowardsMovement(Vector3 moveDirection)
    {
        if (moveDirection != Vector3.zero) // Prevent rotation towards zero vector
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
}
