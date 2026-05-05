using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetRemainingCountsText : MonoBehaviour
{
    public TextMeshProUGUI countText;

    [Header("前缀文本")]
    public string preText;
    [Header("后缀文本")]
    public string sufText;

    public void SetText(int count)
    { 
        countText.text = preText + count + sufText;
    }
}
