using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avoidance : MonoBehaviour
{
    [Header("Avoiding Settings")]
    [Tooltip("Layer mask to define which layers to check for avoidance.")]
    public LayerMask avoidLayers;
    [Tooltip("The target distance OtherDark seek to maintain from the avoidTarget")]
    public float avoidRadius = 12.0f;
    [Tooltip("The angle at which to avoid")]
    public float avoidAngle = 45.0f;
    [Tooltip("The extent of avoidance")]
    public float avoidWeight = 1.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 GetAvoidanceVector()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, avoidRadius, avoidLayers);
        Vector3 avoidanceVector = Vector3.zero;
        foreach (Collider hit in hits)
        {
            if (hit.transform != transform) // Avoid self
            {
                Vector3 directionToTarget = transform.position - hit.transform.position;
                float distance = directionToTarget.magnitude;

                // More weight is given to objects closer to this object
                float weight = Mathf.Clamp01((avoidRadius - distance) / avoidRadius) * avoidWeight;

                //Debug.DrawRay(transform.position, directionToTarget, Color.blue);

                // Add to the avoidance vector, inversely proportional to the distance
                avoidanceVector += directionToTarget.normalized * weight;
            }
        }

        return avoidanceVector;
    }

    public Vector3 GetAngleAvoidanceVector(Vector3 avoidanceVectorLocal)
    {
        // Rotate the direction to target by avoidance angle to either left or right
        Vector3 angleAvoidanceVector = Quaternion.Euler(0, 0, avoidAngle * Random.Range(-1f, 1f)) * avoidanceVectorLocal;
        //Debug.DrawRay(transform.position, rotatedDirection, Color.yellow);
        return angleAvoidanceVector;
    }
}
