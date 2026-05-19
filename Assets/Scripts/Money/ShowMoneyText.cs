using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowMoneyText : MonoBehaviour
{
    [Header("钱的文本组件")]
    public TextMeshProUGUI text;
    
    void Start()
    {
        text.text = MoneyManager.Money.ToString();
        
        //订阅事件
        MoneyManager.OnMoneyChanged += UpdateMoneyText;
    }

    void UpdateMoneyText(int newMoney)
    {
        text.text = newMoney.ToString();
    }

    void OnDestroy()
    {
        //取消订阅，防止内存泄漏
        MoneyManager.OnMoneyChanged -= UpdateMoneyText;
    }
}
