using Statics.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackpackModel
{
    //背包数组
    private List<BagItem> bagItems;
    //背包容量
    int capacity;

    /// <summary>
    /// 获取Count
    /// </summary>
    public int Count => bagItems.Count;
    /// <summary>
    /// 获取Capacity
    /// </summary>
    public int Capacity => capacity;
    /// <summary>
    /// 获取只读列表
    /// </summary>
    public IReadOnlyList<BagItem> BagItems => bagItems.AsReadOnly();


    public BackpackModel(int capacity)
    {
        bagItems = new List<BagItem>(capacity);
        this.capacity = capacity;
    }

    /// <summary>
    /// 增加一个BagItem进入背包
    /// </summary>
    /// <param name="bagItem"></param>
    /// <returns></returns>
    public bool AddItem(BagItem bagItem)
    { 
        if (Count >= capacity) return false;

        foreach (var item in bagItems)
        {
            if (item.material.id == bagItem.material.id)
            {
                item.IncreaseNum(bagItem.num);
                return true;
            }
        }

        bagItems.Add(bagItem);
        return true;
    }

    /// <summary>
    /// 在指定位置移除quantity个物品
    /// </summary>
    /// <param name="index"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public bool RemoveItemAt(int index, int quantity = 1)
    {
        if (index >= Count) return false;

        if (bagItems[index].num > quantity)
        {
            bagItems[index].num -= quantity;
        }
        else
        { 
            bagItems.RemoveAt(index);
        }
        return true;
    }

    /// <summary>
    /// 获取指定位置的物品
    /// </summary>
    public BagItem GetItemAt(int index)
    {
        if (index < 0 || index >= Count) return null;
        return bagItems[index];
    }

    /// <summary>
    /// 排序
    /// </summary>
    public void Sort(System.Comparison<BagItem> comparison)
    {
        bagItems.Sort(comparison);
    }

    /// <summary>
    /// 清空背包
    /// </summary>
    public void Clear()
    {
        bagItems.Clear();
    }

    /// <summary>
    /// 获取start到end的物品，闭区间
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public List<BagItem> GetItemRange(int start, int end)
    {
        if (start >= bagItems.Count) return new List<BagItem>();

        int actualCount = Mathf.Min(end - start + 1, bagItems.Count - start); //实际有BagItem的Count数
        if (actualCount <= 0) return new List<BagItem>();

        return bagItems.GetRange(start, actualCount);
    }
}

