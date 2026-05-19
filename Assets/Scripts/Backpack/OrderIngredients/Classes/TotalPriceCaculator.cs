using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TotalPriceCaculator : MonoBehaviour
{
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI totalPriceText;

    /// <summary>
    /// ·µ»ØÊýÁ¿
    /// </summary>
    /// <returns></returns>
    public int Caculate()
    {
        int count = int.Parse(countText.text);
        totalPriceText.text = (int.Parse(priceText.text) * count).ToString();
        return count;
    }
}
