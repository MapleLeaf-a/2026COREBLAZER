using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TotalPriceCaculator : MonoBehaviour
{
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI totalPriceText;

    public void Caculate()
    {
        totalPriceText.text = (int.Parse(priceText.text) * int.Parse(countText.text)).ToString();
    }
}
