using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which controls OtherDark behaviour
/// </summary>
public class OtherDark : MonoBehaviour
{
    private Flocking Flocking;
    private Avoidance Avoidance;
    private SpaceLimits SpaceLimits;

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

    /// <summary>
    /// Standard Unity function called once before the first call to Update
    /// </summary>
    private void Start()
    {
        Flocking = GetComponent<Flocking>();
        Avoidance = GetComponent<Avoidance>();
        SpaceLimits = GetComponent<SpaceLimits>();
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
        Vector3 avoidDirection = Vector3.zero;

        // Check if the target is in range, then move
        if (useAvoidance && Avoidance != null)
        {
            avoidDirection += Avoidance.GetAvoidanceVector();
            //Debug.Log("Avoid Target x:" + moveDirection.x + " and y:" + moveDirection.y);
        }
        if (useSpaceLimits && SpaceLimits != null)
        {
            moveDirection += SpaceLimits.GetBounceVector();
        }
        if (useFlocking && Flocking != null)
        {
            moveDirection += Flocking.GetFlockVector();
        }
        moveDirection += ((inertiaFactor * this.transform.forward) + RandomMove());
        //Debug.Log("Final (incl. inertia) x:" + moveDirection.x + " and y:" + moveDirection.y);
        Vector3 movement = (moveSpeedBase * Time.deltaTime * moveDirection.normalized)
            + (moveSpeedBase * Time.deltaTime * avoidDirection.normalized);
        movement.z = 0;
        transform.position += movement;
        RotateTowardsMovement(movement, degreeMax);
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
