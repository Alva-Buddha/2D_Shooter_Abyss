using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attraction : MonoBehaviour
{
    [Header("attracting Settings")]
    [Tooltip("Layer mask to define which layers to check for attractance.")]
    public LayerMask attractLayers;
    [Tooltip("The target distance OtherDark seek to maintain from the attractTarget")]
    public float attractRadius = 20.0f;
    [Tooltip("The random offset angle at which to attract")]
    public float attractAngle = 45.0f;
    [Tooltip("The extent of attractance")]
    public float attractWeight = 1.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 GetAttractVector()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attractRadius, attractLayers);
        Vector3 attractVector = Vector3.zero;
        foreach (Collider hit in hits)
        {
            if (hit.transform != transform) // attract self
            {
                Vector3 directionToTarget = transform.position - hit.transform.position;
                float distance = directionToTarget.magnitude;

                // More weight is given to objects closer to this object
                float weight = Mathf.Clamp01(attractRadius - distance / attractRadius) * attractWeight;

                //Debug.DrawRay(transform.position, directionToTarget, Color.blue);

                // Add to the attractance vector, inversely proportional to the distance
                attractVector -= directionToTarget.normalized * weight;
            }
        }

        return attractVector;
    }
}
