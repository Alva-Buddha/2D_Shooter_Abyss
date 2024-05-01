using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocking : MonoBehaviour
{
    private List<Transform> neighborsLocal;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Calculates the movement vector to execute flocking based on input parameters
    /// </summary>
    /// <param name="alignmentWeight">Weight to apply to the alignment vector, influencing how much agents align with each other's heading.</param>
    /// <param name="separationWeight">Weight to apply to the separation vector, influencing how much agents avoid crowding others.</param>
    /// <param name="cohesionWeight">Weight to apply to the cohesion vector, influencing how much agents move towards the average position of neighbors.</param>
    /// <param name="avoidOtherRadius">Radius within which an agent will begin to move away from others to avoid crowding.</param>
    /// <param name="neighborsInput">List of Transform references representing the neighbors each agent considers for flocking.</param>
    /// <returns>Returns a Vector3 representing the combined movement vector for an agent based on the flocking behavior.</returns>
    public Vector3 GetFlockVector(float alignmentWeight, float separationWeight, float cohesionWeight, float avoidOtherRadius, 
        List<Transform> neighborsInput)
    {
        neighborsLocal = neighborsInput;
        CleanUpNeighbors();

        Vector3 alignment = Alignment();
        Vector3 separation = Separation(avoidOtherRadius);
        Vector3 cohesion = Cohesion();

        // Combine the behaviors with possibly different weights
        Vector3 flockVector = (alignment * alignmentWeight) + (cohesion * cohesionWeight) + (separation * separationWeight);
        Debug.Log("Flock vector returned");
        return flockVector;
    }

    /// <summary>
    /// Calculates the average direction of nearby boids for alignment.
    /// </summary>
    /// <returns>Vector3 representing the average heading of nearby boids.</returns>
    private Vector3 Alignment()
    {
        Vector3 alignVector = Vector3.zero;
        foreach (Transform neighbor in neighborsLocal)
        {
            alignVector += neighbor.forward;  // Assuming forward is the moving direction
        }
        if (neighborsLocal.Count > 0)
        {
            alignVector /= neighborsLocal.Count;
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
        foreach (Transform neighbor in neighborsLocal)
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
        foreach (Transform neighbor in neighborsLocal)
        {
            cohesionVector += neighbor.position;
        }
        if (neighborsLocal.Count > 0)
        {
            cohesionVector /= neighborsLocal.Count;
            cohesionVector -= transform.position;
        }
        return cohesionVector;
    }

    /// <summary>
    /// Updates the list of nearby boids to be considered for flocking.
    /// </summary>
    public List<Transform> UpdateNeighbors(float neighborRadius, LayerMask neighborLayerMask)
    {
        List<Transform> neighbors;
        neighbors = new List<Transform>();

        Collider[] colliders = Physics.OverlapSphere(transform.position, neighborRadius, neighborLayerMask);
        Debug.Log("neighbor count:" + colliders.Length);
        foreach (Collider collider in colliders)
        {
            if (collider.transform != transform) // Avoid adding self
            {
                neighbors.Add(collider.transform);
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Clears any null or destroyed objects from the neighbors list.
    /// </summary>
    private void CleanUpNeighbors()
    {
        neighborsLocal.RemoveAll(item => item == null || item.gameObject == null);
    }
}
