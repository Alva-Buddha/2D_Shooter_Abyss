using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using static UnityEngine.UI.Image;

public class SpaceLimits : MonoBehaviour
{
    [Header("Avoiding Settings")]
    [Tooltip("The transform of the object that defines the centre of the space")]
    private Transform originObject;
    [Tooltip("The radius beyond which the the redirection vector should apply")]
    public float freeRadius = 20.0f;
    [Tooltip("The extent of avoidance - set to a high number")]
    public float bounceWeight = 10.0f;

    private Vector3 bounceVector = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        originObject = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// The direction and magnitude of the OtherDark's desired movement in Avoid mode
    /// </summary>
    /// <returns>Vector3: The movement to be used in Avoid movement mode.</returns>
    public Vector3 GetBounceVector()
    {
        if (originObject == null)
        {
            return Vector3.zero;
        }
        Vector3 directionToOrigin = originObject.position - transform.position;
        float distance = directionToOrigin.magnitude;

        // More weight is given to objects closer to this object
        float weight = Mathf.Clamp01((distance - freeRadius) / freeRadius) * bounceWeight;

        // Add to the avoidance vector, inversely proportional to the distance
        bounceVector = directionToOrigin.normalized * weight;
        return bounceVector;
    }
}
