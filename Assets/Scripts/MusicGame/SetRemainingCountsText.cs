using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetRemainingCountsText : MonoBehaviour
{
    public TextMeshProUGUI countText;

    public void SetText(int count)
    { 
        countText.text = "剩余获取鱼的次数：" + count;
    }
}
