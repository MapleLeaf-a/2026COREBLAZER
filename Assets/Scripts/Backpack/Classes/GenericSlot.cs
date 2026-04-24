using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用槽位脚本
/// </summary>
/// <typeparam name="T">物品种类</typeparam>
public class GenericSlot<T> : MonoBehaviour where T : class
{
    //物品图像
    public Image itemImage;
    //选中显示
    public Image selectedImage;


    protected T currentItem;
    protected int slotIndex;

    void Awake()
    {
        if (itemImage != null) 
            itemImage.enabled = false;
        if (selectedImage != null)
            selectedImage.enabled = false;
    }

    /// <summary>
    /// 设置槽位显示
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    /// <param name="isSelected"></param>
    public virtual void SetUp(T item, Sprite image, int index, bool isSelected)
    {
        currentItem = item;
        slotIndex = index;

        if (item != null && itemImage != null)
        {
            itemImage.sprite = image;
            itemImage.enabled = true;
        }

        SetSelected(isSelected);
    }

    /// <summary>
    /// 清除槽位各显示效果
    /// </summary>
    public virtual void Clear()
    {
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }

        currentItem = null;

        SetSelected(false);
    }

    /// <summary>
    /// 设置选中效果
    /// </summary>
    /// <param name="isSelected"></param>
    public virtual void SetSelected(bool isSelected)
    {
        if (selectedImage != null)
        {
            if (isSelected)
            {
                selectedImage.enabled = true;
            }
            else
            {
                selectedImage.enabled = false;
            }
        }
    }
}
