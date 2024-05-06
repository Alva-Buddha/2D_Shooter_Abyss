using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This class inherits from the UIelement class and handles the display of the high score
/// </summary>
public class TimeDisplay : UIelement
{
    [Tooltip("The TMP UI to use for display")]
    public TextMeshProUGUI displayText = null; // Change Text to TextMeshProUGUI

    /// <summary>
    /// Description:
    /// Changes the high score display
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    public void DisplayTime()
    {
        if (displayText != null)
        {
            float time = GameManager.instance.totalTimePlayed;
            int hours = (int)(time / 3600);
            int minutes = (int)(time % 3600) / 60;
            //Debug.Log("hours "+ hours + " & minutes " + minutes);
            displayText.text = hours + " hours and " + minutes + " minutes";
        }
    }

    /// <summary>
    /// Description:
    /// Overrides the virtual function UpdateUI() of the UIelement class and uses the DisplayHighScore function to update
    /// Inputs:
    /// none
    /// Returns:
    /// void (no return)
    /// </summary>
    public override void UpdateUI()
    {
        // This calls the base update UI function from the UIelement class
        base.UpdateUI();

        // The remaining code is only called for this sub-class of UIelement and not others
        DisplayTime();
    }
}
