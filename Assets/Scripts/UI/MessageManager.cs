using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class MessageManager : MonoBehaviour
{
    [Header("Message details")]

    //UIObject to change panel size containing TMP object for message
    [Tooltip("UIObject for message panel")]
    public GameObject messagePanel;
    //UIObject to change text of message
    [Tooltip("UIObject for message to be displayed")]
    public Text messageText; 
    
    [Tooltip("Duration of display")]
    public float messageDuration = 5f;
    
    private float messageTimer = 0f;


    // Start is called before the first frame update
    void Start()
    {
        messageText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
            {
                messageText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Shows the message sent as a part of the message input
    /// </summary>
    /// <param name="message">Text to be displayed</param>
    public void ShowMessage(string message)
    {
        //Get height required for message given text box width
        messageText.text = message;
        float height = messageText.preferredHeight;

        //change panel height to fit message
        RectTransform rt = messagePanel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + messageText.preferredHeight);

        //Set the message text to the message input
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        messageTimer = messageDuration;
    }
}
