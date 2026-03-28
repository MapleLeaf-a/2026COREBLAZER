using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetCurrentPerfectRateText : MonoBehaviour
{
    public TextMeshProUGUI currentPerfectRateText;

    void OnEnable()
    {
        currentPerfectRateText.text = Mathf.Round(ScoreManager.ScoreManagerInstance.score.GetCurrentRate() * 100).ToString() + "%";
    }
}
