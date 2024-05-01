using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocking : MonoBehaviour
{
    [Header("Flocking Settings")]
    [Tooltip("The radius to detect neighboring boids.")]
    public float neighborRadius = 3.0f;
    [Tooltip("The radius to maintain distance from neighboring boids for separation.")]
    public float avoidOtherRadius = 2.0f;
    [Tooltip("Weight for alignment movement")]
    public float alignmentWeight = 2.0f;
    [Tooltip("Weight for separation movement")]
    public float separationWeight = 1.0f;
    [Tooltip("Weight for cohesion movement")]
    public float cohesionWeight = 0.5f;
    [Tooltip("Layers examined for Neighbors")]
    public LayerMask neighborLayers; 
    [Tooltip("How often the flock vector updates")]
    public float UpdateInterval = 0.2f; 

    private List<Transform> neighbors;
    private float nextUpdateTime;
    private Vector3 flockVector = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        neighbors = new List<Transform>();
        nextUpdateTime = Time.time + UpdateInterval;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Calculates the movement vector to execute flocking based on input parameters
    /// </summary>
    /// <returns>Returns a Vector3 representing the combined movement vector for an agent based on the flocking behavior.</returns>
    public Vector3 GetFlockVector()
    {
        //Debug.Log("Flock vector requested");
        if (nextUpdateTime < Time.time) 
        {
            UpdateNeighbors();
            //Debug.Log("neigbor count" + neighbors.Count);

            Vector3 alignment = Alignment();
            Vector3 separation = Separation(avoidOtherRadius);
            Vector3 cohesion = Cohesion();

            // Combine the behaviors with possibly different weights
            flockVector = (alignment * alignmentWeight) + (cohesion * cohesionWeight) + (separation * separationWeight);
            //Debug.Log("Flock vector updated");
            //Debug.Log("Flock x:" + flockVector.x + " and y:" + flockVector.y);
            nextUpdateTime = Time.time + UpdateInterval;
        }
        //Debug.Log("Flock vector returned");
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
    /// Calculates a vector to keep distance from nearby boids for separation.
    /// </summary>
    /// <param name="avoidOtherRadiusLocal">The radius within which the boid should start moving away to avoid crowding. 
    /// Any neighbor within this radius will contribute to the repulsion force.</param>
    /// <returns>A Vector3 representing the direction and magnitude of the movement needed for a boid to maintain separation from its neighbors. 
    /// This vector points away from neighbors that are too close within the specified radius.</returns>
    private Vector3 Separation(float avoidOtherRadiusLocal)
    {
        Vector3 separationVector = Vector3.zero;
        foreach (Transform neighbor in neighbors)
        {
            float proximity = (neighbor.position - transform.position).magnitude;
            if (proximity < avoidOtherRadiusLocal)
            {
                separationVector -= (neighbor.position - transform.position) / Mathf.Max(proximity * proximity, 0.0001f);
            }
        }

        return separationVector;
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
    /// Updates the list of nearby boids to be considered for flocking.
    /// </summary>
    public void UpdateNeighbors()
    {
        neighbors = new List<Transform>();

        Collider[] colliders = Physics.OverlapSphere(transform.position, neighborRadius, neighborLayers);
        //Debug.Log("neighbor count:" + colliders.Length);
        foreach (Collider collider in colliders)
        {
            if (collider.transform != transform) // Avoid adding self
            {
                neighbors.Add(collider.transform);
            }
        }
    }
}
