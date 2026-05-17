using ObjectPool.Interface;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientUIItem : MonoBehaviour
{
    [Header("UI组件")]
    public Image iconImage;
    public TextMeshProUGUI XText;
    public TextMeshProUGUI quantityText;
    

    void Awake()
    {
        if (iconImage != null)
            iconImage.enabled = false;
        if (quantityText != null)
            quantityText.enabled = false;
        if (XText != null)
            XText.enabled = false;
    }

    /// <summary>
    /// 设置槽位显示
    /// </summary>
    /// <param name="image"></param>
    /// <param name="quantity"></param>
    public void SetUp(Sprite image, int quantity)
    {
        if (iconImage != null)
        { 
            iconImage.sprite = image;
            iconImage.enabled = true;
            iconImage.SetNativeSize();
            iconImage.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        }

        if (quantityText != null)
        { 
            quantityText.text = quantity.ToString();
            quantityText.enabled = true;
        }

        if (XText != null)
        {
            XText.enabled = true;
        }
    }

    /// <summary>
    /// 清除槽位各显示效果
    /// </summary>
    public virtual void Clear()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.enabled = false;
        }

        if (XText != null)
        { 
            XText.enabled= false;
        }
    }
}
