﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles the health state of a game object.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Team Settings")]
    [Tooltip("The team associated with this damage")]
    public int teamId = 0;
    [Tooltip("Health bar")]
    public Slider healthBarSlider = null;

    [Header("Health Settings")]
    [Tooltip("The default health value")]
    public float defaultHealth = 1.0f;
    [Tooltip("The maximum health value")]
    public float maximumHealth = 1.0f;
    [Tooltip("The current in game health value")]
    public float currentHealth = 1.0f;
    [Tooltip("Invulnerability duration, in seconds, after taking damage")]
    public float invincibilityTime = 3f;
    [Tooltip("Whether or not this health is always invincible")]
    public bool isAlwaysInvincible = false;

    [Header("Lives settings")]
    [Tooltip("Whether or not to use lives")]
    public bool useLives = false;
    [Tooltip("Current number of lives this health has")]
    public int currentLives = 3;
    [Tooltip("The maximum number of lives this health can have")]
    public int maximumLives = 5;

    [Header("Damage and recovery settings")]
    [Tooltip("Activate constant health loss per second")]
    public bool constantDrain = false;
    [Tooltip("Health loss per second")]
    public float drainPerSec = 1.0f;

    private float updateTime = 1.0f;

    [Header("Message associated with health")]
    [Tooltip("Should a message be sent on 1st drain of health")]
    public bool firstDrainMessage = false;
    [TextArea]
    [Tooltip("Message to be sent on 1st drain of health")]
    public string firstDrainText = null;

    [Tooltip("Should a message be sent on 1st damage")]
    public bool firstDamageMessage = false;
    [TextArea]
    [Tooltip("Message to be sent on 1st drain of health")]
    public string firstDamageText = null;

    //flag to check if first drain has been happened
    private bool firstDrainFlag = false;

    //flag to check if first damage has been happened
    private bool firstDamage = false;

    [Tooltip("GameObject to display message")]
    public GameObject messageObject = null;

    private MessageManager messageManager;

    void Start()
    {
        SetRespawnPoint(transform.position);
        updateTime += Time.time;
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maximumHealth;
        }
    }

    void Update()
    {
        InvincibilityCheck();
        if (constantDrain && Time.time > updateTime)
        {
            TakeDamage(drainPerSec);
            //Send message on first drain
            if (firstDrainMessage && !firstDrainFlag)
            {
                if (messageObject != null)
                {
                    messageManager = messageObject.GetComponent<MessageManager>();
                    messageManager.AddMessage(firstDrainText);
                    firstDrainFlag = true;
                }
            }
            updateTime = 1 + Time.time;
        }
    }

    // The specific game time when the health can be damged again
    private float timeToBecomeDamagableAgain = 0;
    // Whether or not the health is invincible
    private bool isInvincableFromDamage = false;

    /// <summary>
    /// Description:
    /// Checks against the current time and the time when the health can be damaged again.
    /// Removes invicibility if the time frame has passed
    /// </summary>
    private void InvincibilityCheck()
    {
        if (timeToBecomeDamagableAgain <= Time.time)
        {
            isInvincableFromDamage = false;
        }
    }

    // The position that the health's gameobject will respawn at if lives are being used
    private Vector3 respawnPosition;
    /// <summary>
    /// Description:
    /// Changes the respawn position to a new position
    /// </summary>
    /// <param name="newRespawnPosition">The new position to respawn at</param>
    public void SetRespawnPoint(Vector3 newRespawnPosition)
    {
        respawnPosition = newRespawnPosition;
    }

    /// <summary>
    /// Description:
    /// Repositions the health's game object to the respawn position and resets the health to the default value
    /// </summary>
    void Respawn()
    {
        transform.position = respawnPosition;
        currentHealth = defaultHealth;
    }

    /// <summary>
    /// Description:
    /// Applies damage to the health unless the health is invincible.
    /// </summary>
    /// <param name="damageAmount">The amount of damage to take</param>
    public void TakeDamage(float damageAmount)
    {
        if (isInvincableFromDamage || isAlwaysInvincible)
        {
            return;
        }
        else
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, transform.rotation, null);
            }
            timeToBecomeDamagableAgain = Time.time + invincibilityTime;
            isInvincableFromDamage = true;
            currentHealth -= damageAmount;
            //Send message on first damage
            if (firstDamageMessage && !firstDamage)
            {
                if (!firstDrainMessage || firstDrainFlag)
                {
                    if (messageObject != null)
                    {
                        messageManager = messageObject.GetComponent<MessageManager>();
                        messageManager.AddMessage(firstDamageText);
                        firstDamage = true;
                    }
                }
            }
            if (healthBarSlider!=null)
            {
                healthBarSlider.value = currentHealth;
            }
            CheckDeath();
        }
    }

    /// <summary>
    /// Description:
    /// Applies healing to the health, capped out at the maximum health.
    /// </summary>
    /// <param name="healingAmount">How much healing to apply</param>
    public void ReceiveHealing(float healingAmount)
    {
        currentHealth += healingAmount;
        if (currentHealth > maximumHealth)
        {
            currentHealth = maximumHealth;
        }
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }
        CheckDeath();
    }

    [Header("Effects & Polish")]
    [Tooltip("The effect to create when this health dies")]
    public GameObject deathEffect;
    [Tooltip("The effect to create when this health is damaged")]
    public GameObject hitEffect;

    /// <summary>
    /// Description:
    /// Checks if the health is dead or not. If it is, true is returned, false otherwise.
    /// Calls Die() if the health is dead.
    /// <returns>Bool: true or false value representing if the health has died or not (true for dead)</returns>
    bool CheckDeath()
    {
        if (currentHealth <= 0)
        {
            Die();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Description:
    /// Handles the death of the health. If a death effect is set, it is created. If lives are being used, the health is respawned.
    /// If lives are not being used or the lives are 0 then the health's game object is destroyed.
    /// </summary>
    public void Die()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation, null);
        }

        if (useLives)
        {
            HandleDeathWithLives();
        }
        else
        {
            HandleDeathWithoutLives();
        }      
    }

    /// <summary>
    /// Description:
    /// Handles the death of the health when lives are being used
    /// </summary>
    void HandleDeathWithLives()
    {
        currentLives -= 1;
        if (currentLives > 0)
        {
            Respawn();
        }
        else
        {
            if (gameObject.tag == "Player" && GameManager.instance != null)
            {
                GameManager.instance.GameOver();
            }
            if (gameObject.GetComponent<OtherDark>() != null)
            {
                gameObject.GetComponent<OtherDark>().DoBeforeDestroy();
            }
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Description:
    /// Handles death when lives are not being used
    /// </summary>
    void HandleDeathWithoutLives()
    {
        if (gameObject.tag == "Player" && GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }
        if (gameObject.GetComponent<OtherDark>() != null)
        {
            gameObject.GetComponent<OtherDark>().DoBeforeDestroy();
        }
        Destroy(this.gameObject);
    }
}
