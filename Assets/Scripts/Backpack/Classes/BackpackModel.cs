using Statics.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackpackModel
{
    //背包数组
    private BagItem[] bagItems;
    //背包容量
    int capacity;
    //背包中非空物品的数量
    int count;

    /// <summary>
    /// 获取Count
    /// </summary>
    public int Count => count;
    /// <summary>
    /// 获取Capacity
    /// </summary>
    public int Capacity => capacity;
    /// <summary>
    /// 获取只读列表
    /// </summary>
    public BagItem[] BagItems => bagItems;


    public BackpackModel(int capacity)
    {
        bagItems = new BagItem[capacity];
        this.capacity = capacity;
        this.count = 0;
    }

    /// <summary>
    /// 增加一个BagItem进入背包
    /// </summary>
    /// <param name="bagItem"></param>
    /// <returns></returns>
    public bool AddItem(BagItem bagItem)
    { 
        if (count >= capacity) return false;

        for (int i = 0; i < capacity; i++)
        {
            BagItem item = bagItems[i];
            if (item == null)
            {
                bagItems[i] = bagItem;
                break;
            }
            else if (item.material.id == bagItem.material.id)
            {
                item.IncreaseNum(bagItem.num);
                break;
            }
        }

        count++;

        return true;
    }

    /// <summary>
    /// 在指定位置加物品入背包
    /// </summary>
    /// <param name="bagItem"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool AddItemAt(BagItem bagItem, int index)
    { 
        if (count >= capacity || index >= capacity || index < 0) return false;

        BagItem item = bagItems[index];
        if (item == null)
        {
            bagItems[index] = bagItem;
            count++;
        }
        else if (item.material.id == bagItem.material.id)
        {
            item.IncreaseNum(bagItem.num);
        }
        else //不可堆叠,增加失败
        { 
            return false; 
        }
        
        return true;
    }

    /// <summary>
    /// 在指定绝对位置移除quantity个物品
    /// </summary>
    /// <param name="index"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public bool RemoveItemAt(int index, int quantity = 1)
    {
        if (index >= capacity) return false;

        if (bagItems[index].num > quantity)
        {
            bagItems[index].num -= quantity;
        }
        else
        { 
            bagItems[index] = null;
        }
        
        count--;

        return true;
    }

    /// <summary>
    /// 获取指定绝对位置的物品
    /// </summary>
    public BagItem GetItemAt(int index)
    {
        if (index < 0 || index >= capacity) return null;
        return bagItems[index];
    }

    /// <summary>
    /// 交换两个绝对位置的物品
    /// </summary>
    /// <param name="indexA"></param>
    /// <param name="indexB"></param>
    /// <returns></returns>
    public bool SwapItem(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= capacity
         || indexB < 0 || indexB >= capacity) return false;

        var temp = bagItems[indexA];
        bagItems[indexA] = bagItems[indexB];
        bagItems[indexB] = temp;
        return true;
    }

    /// <summary>
    /// 获取start到end的物品，闭区间
    /// </summary>
    /// <param name="start">绝对开始位置</param>
    /// <param name="end">绝对结束位置</param>
    /// <returns></returns>
    public BagItem[] GetItemRange(int start, int end)
    {
        if (start >= capacity) return new BagItem[0];

        int actualCount = Mathf.Min(end - start + 1, capacity - start);
        if (actualCount <= 0) return new BagItem[0];

        BagItem[] result = new BagItem[actualCount];
        Array.Copy(bagItems, start, result, 0, actualCount);
        return result;
    }
}

