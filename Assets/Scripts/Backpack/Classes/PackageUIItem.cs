using Statics.Classes;
using TMPro;
using UnityEngine;

public class PackageUIItem : GenericSlot<BagItem>
{
    ////物品图像
    //public Image itemImage;
    //物品数量文本
    public TextMeshProUGUI quantityText;
    ////选中显示
    //public Image selectedImage;


    //private BagItem currentItem;
    //private int slotIndex;

    void Awake()
    {
        itemImage.enabled = false;
        quantityText.enabled = false;
        selectedImage.enabled = false;
    }

    /// <summary>
    /// 设置槽位显示
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    /// <param name="isSelected"></param>
    public override void SetUp(BagItem item, Sprite image, int index, bool isSelected)
    {
        currentItem = item;
        slotIndex = index;

        if (item != null)
        {
            itemImage.sprite = image;
            itemImage.enabled = true;
            itemImage.SetNativeSize();
        }

        if (item.num > 1) //物品数量大于1显示数量文本
        {
            quantityText.text = item.num.ToString();
            quantityText.enabled = true;
        }
        else
        {
            quantityText.enabled = false;
        }

        SetSelected(isSelected);
    }

    /// <summary>
    /// 清除槽位各显示效果
    /// </summary>
    public override void Clear()
    {
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }

        quantityText.enabled = false;

        currentItem = null;

        SetSelected(false);
    }

    ///// <summary>
    ///// 设置选中效果
    ///// </summary>
    ///// <param name="isSelected"></param>
    //public void SetSelected(bool isSelected)
    //{
    //    if (selectedImage != null)
    //    {
    //        if (isSelected)
    //        {
    //            selectedImage.enabled = true;
    //        }
    //        else
    //        {
    //            selectedImage.enabled = false; 
    //        }
    //    }
    //}
}
