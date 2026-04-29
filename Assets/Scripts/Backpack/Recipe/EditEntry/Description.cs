using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Description : MonoBehaviour
{
    [Header("描述")]
    [Tooltip("词条")]
    public Image entry;
    [Tooltip("原材料的名字文本组件")]
    public TextMeshProUGUI foodMaterialNameText;

    public void SetUp(Sprite entry, string foodMaterialNameText)
    {
        this.entry.sprite = entry;
        this.foodMaterialNameText.text = foodMaterialNameText;
    }

    public void SetUp(Sprite entry)
    {
        this.entry.sprite = entry;
    }

    public void Clear()
    { 
        this.entry.sprite = null;
    }
}
