using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// A class which controlls player aiming and shooting
/// </summary>
public class ShootingController : MonoBehaviour
{
    private Health HealthImpact;

    [Header("GameObject/Component References")]
    [Tooltip("The projectile to be fired.")]
    public GameObject projectilePrefab = null;
    [Tooltip("The transform in the heirarchy which holds projectiles if any")]
    public Transform projectileHolder = null;

    [Header("Input")]
    [Tooltip("Whether this shooting controller is controled by the player")]
    public bool isPlayerControlled = false;

    [Header("Firing Settings")]
    [Tooltip("The minimum time between projectiles being fired.")]
    public float fireRate = 0.05f;

    [Tooltip("The maximum diference between the direction the" +
        " shooting controller is facing and the direction projectiles are launched.")]
    public float projectileSpread = 1.0f;

    [Tooltip("Distance from object for launch of projectile")]
    public float projectileDistance = 1.0f;

    // The last time this component was fired
    private float lastFired = Mathf.NegativeInfinity;

    [Header("Effects")]
    [Tooltip("The effect to create when this fires")]
    public GameObject fireEffect;
    [Tooltip("Whether shooting drains health")]
    public bool fireDamageCheck = false;
    [Tooltip("Health drain from shooting")]
    public float fireDamage = 1.0f;


    //The input manager which manages player input
    private InputManager inputManager = null;

    [Header("Message associated with 1st projectile")]
    [Tooltip("Should a message be sent on 1st projectile")]
    public bool firstProjectileMessage = true;

    [TextArea]
    [Tooltip("Message to be sent on 1st projectile")]
    public string firstProjectileMessageText = null;    

    //flag to check if first projectile has been fired
    private bool firstProjectileFired = false;

    [Tooltip("GameObject to display message")]
    public GameObject messageObject = null;

    private MessageManager messageManager;

    /// <summary>
    /// Description:
    /// Standard unity function that runs every frame
    /// </summary>
    private void Update()
    {
        ProcessInput();
    }

    /// <summary>
    /// Description:
    /// Standard unity function that runs when the script starts
    /// </summary>
    private void Start()
    {
        SetupInput();
        HealthImpact = GetComponent<Health>();
        if (messageObject != null)
        {
            messageManager = messageObject.GetComponent<MessageManager>();
        }
    }

    /// <summary>
    /// Description:
    /// Attempts to set up input if this script is player controlled and input is not already correctly set up 
    /// </summary>
    void SetupInput()
    {
        if (isPlayerControlled)
        {
            if (inputManager == null)
            {
                inputManager = InputManager.instance;
            }
            if (inputManager == null)
            {
                Debug.LogError("Player Shooting Controller can not find an InputManager in the scene, there needs to be one in the " +
                    "scene for it to run");
            }
        }
    }

    /// <summary>
    /// Description:
    /// Reads input from the input manager
    /// </summary>
    void ProcessInput()
    {
        if (isPlayerControlled)
        {
            if (inputManager.firePressed || inputManager.fireHeld)
            {
                if(!EventSystem.current.IsPointerOverGameObject())
                {
                    Fire();
                }
            }
        }   
    }

    /// <summary>
    /// Description:
    /// Fires a projectile if possible
    /// </summary>
    public void Fire()
    {
        // If the cooldown is over fire a projectile
        if ((Time.timeSinceLevelLoad - lastFired) > fireRate)
        {
            // Launches a projectile
            SpawnProjectile();

            if (fireEffect != null)
            {
                Instantiate(fireEffect, transform.position, transform.rotation, null);
            }
            if (fireDamageCheck && HealthImpact != null)
            {
                HealthImpact.TakeDamage(fireDamage);
            }

            // Restart the cooldown
            lastFired = Time.timeSinceLevelLoad;
        }
    }

    /// <summary>
    /// Description:
    /// Spawns a projectile and sets it up
    /// </summary>
    public void SpawnProjectile()
    {
        // Check that the prefab is valid
        if (projectilePrefab != null)
        {
            // Create the projectile
            GameObject projectileGameObject = Instantiate(projectilePrefab, transform.position + transform.up*projectileDistance, transform.rotation, null);

            // Account for spread
            Vector3 rotationEulerAngles = projectileGameObject.transform.rotation.eulerAngles;
            rotationEulerAngles.z += Random.Range(-projectileSpread, projectileSpread);
            projectileGameObject.transform.rotation = Quaternion.Euler(rotationEulerAngles);

            // Keep the heirarchy organized
            if (projectileHolder != null)
            {
                projectileGameObject.transform.SetParent(projectileHolder);
            }

            //Check if first projectile has been fired
            if (!firstProjectileFired && firstProjectileMessage)
            {
                firstProjectileFired = true;
                if(messageManager != null)
                {
                    messageManager.AddMessage(firstProjectileMessageText);
                }    
            }
        }
    }
}
