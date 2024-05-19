using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Text;

public class MessageManager : MonoBehaviour
{
    [Header("Message details")]

    [Tooltip("UIObject for message panel")]
    public GameObject messagePanel;

    [Tooltip("TMP object for message to be displayed")]
    public TextMeshProUGUI messageText;

    [Tooltip("Duration of display")]
    public float messageDuration = 5f;

    // Default panel
    private Vector2 defaultSize;
    private string defaultText;
    private float defaultTextHeight;

    // Queue to store messages
    private Queue<string> messages = new Queue<string>();

    //Queue to store message times
    private Queue<float> messageTimes = new Queue<float>();

    // Reference to the RectTransform component
    private RectTransform rt;

    void Start()
    {
        rt = messagePanel.GetComponent<RectTransform>();
        defaultSize = rt.sizeDelta; // Save the default panel size
        defaultText = messageText.text; // Save the default text
        defaultTextHeight = messageText.preferredHeight;
    }

    void Update()
    {
        //Check message time vs current time and message duration and remove message if necessary
        if (messageTimes.Count > 0 && Time.time - messageTimes.Peek() > messageDuration)
        {
            RemoveMessage();
        }
    }

    // Add a message to the queue
    public void AddMessage(string message)
    {
        messages.Enqueue(message);
        messageTimes.Enqueue(Time.time);
        UpdateMessageText();
        UpdatePanelSize();
    }


    // Update the message text
    private void UpdateMessageText()
    {
        if (messages.Count > 0)
        {
            //Join current queue to default text
            StringBuilder sb = new StringBuilder();
            sb.Append(defaultText);
            foreach (string message in messages)
            {
                sb.Append("\n\n");
                sb.Append(message);
            }
            messageText.text = sb.ToString();
        }
        else
        {
            messageText.text = defaultText;
        }
    }

    // Update the panel size to fit the message
    private void UpdatePanelSize()
    {
        Canvas.ForceUpdateCanvases(); // Update the canvas to get the correct preferred height
        float paddingFactor = 0.1f;

        float previousHeight = rt.sizeDelta.y;
        rt.sizeDelta = new Vector2(defaultSize.x, defaultSize.y + (messageText.preferredHeight - defaultTextHeight)*(1f+paddingFactor));

        // Calculate the change in height
        float deltaHeight = rt.sizeDelta.y - previousHeight;

        // Move the messageText down/up based on the change in height
        messageText.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, deltaHeight*paddingFactor*0.5f);

        //Debug.Log("update panel size called with preferred height = " + messageText.preferredHeight);
    }

    // Remove the message from the queue
    public void RemoveMessage()
    {
        if (messages.Count > 0)
        {
            messages.Dequeue();
            messageTimes.Dequeue();
            UpdateMessageText();
            UpdatePanelSize();
        }
    }
}
