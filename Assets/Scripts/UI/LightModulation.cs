using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightModulation : MonoBehaviour
{
    [Header("Light Sources")]
    [Tooltip("Directional light attached to camera object")]
    public Light directionalSource = null;
    [Tooltip("Point source at player object")]
    public Light pointSource = null;

    private Health HealthCheck;
    private GameObject playerObject;
    private float directionalIntensity = 1.0f;
    private float pointIntensity = 1.0f;
    private float maxHealth = 1.0f;
    private float maxScore = 1.0f;

    [Tooltip("GameManager reference for pulling data")]
    public GameManager gameManager = null;

    // Start is called before the first frame update
    void Start()
    {
        HealthCheck = this.GetComponent<Health>();
        directionalIntensity = directionalSource.intensity;
        pointIntensity = pointSource.intensity;
        if (HealthCheck != null)
        {
            maxHealth = HealthCheck.maximumHealth;
        }
        if (gameManager != null)
        {
            maxScore = gameManager.GetComponent<GameManager>().enemiesToDefeat; 
        }
    }

    // Update is called once per frame
    void Update()
    {
        directionalSource.intensity = directionalIntensity * (HealthCheck.currentHealth/maxHealth);
        pointSource.intensity = pointIntensity * (GameManager.score/maxScore);
    }
}
