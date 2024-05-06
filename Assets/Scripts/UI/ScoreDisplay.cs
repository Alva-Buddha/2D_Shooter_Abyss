using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This class inherits for the UIelement class and handles updating the score display
/// </summary>
public class ScoreDisplay : UIelement
{
    [Tooltip("The TMP UI to use for display")]
    public TextMeshProUGUI displayText = null; // Change Text to TextMeshProUGUI

    /// <summary>
    /// Description:
    /// Updates the score display
    /// </summary>
    public void DisplayScore()
    {
        if (displayText != null)
        {
            displayText.text = GameManager.score.ToString();
        }
    }

    /// <summary>
    /// Description:
    /// Overides the virtual UpdateUI function and uses the DisplayScore to update the score display
    /// </summary>
    public override void UpdateUI()
    {
        // This calls the base update UI function from the UIelement class
        base.UpdateUI();

        // The remaining code is only called for this sub-class of UIelement and not others
        DisplayScore();
    }
}
