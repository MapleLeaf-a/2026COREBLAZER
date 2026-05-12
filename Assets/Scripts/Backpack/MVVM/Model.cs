using Statics.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Model<T> where T : class
{
    //物品数组
    protected T[] items;
    //物品数组容量
    protected int capacity;

    /// <summary>
    /// 获取Capacity
    /// </summary>
    public int Capacity => capacity;

    /// <summary>
    /// 获取非null物品数量
    /// </summary>
    public int Count
    {
        get
        {
            int count = 0;
            for (int i = 0; i < capacity; i++) 
            {
                if (items[i] != null) count++;
            }

            return count;
        }
    }

    /// <summary>
    /// 获取只读列表
    /// </summary>
    public ReadOnlyCollection<T> Items => Array.AsReadOnly(items);

    public Model(int capacity)
    {
        items = new T[capacity];
        this.capacity = capacity;
    }

    /// <summary>
    /// 增加一个item进入物品数组
    /// </summary>
    public virtual bool AddItem(T item)
    {
        int i;
        for (i = 0; i < capacity; i++)
        {
            if (items[i] == null)
            { 
                items[i] = item;
                return true;
            }
        }

        return false;    
    }

    /// <summary>
    /// 在指定位置加物品入物品数组
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual bool AddItemAt(T item, int index)
    {
        if (index < 0 || index >= capacity) return false;

        if (items[index] == null)
        {
            items[index] = item;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 在指定位置移除物品
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual bool RemoveItemAt(int index)
    {
        if (index < 0 || index >= capacity) return false;

        items[index] = null;

        return true;
    }

    /// <summary>
    /// 获取指定位置的物品
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual T GetItemAt(int index)
    {
        if (index < 0 || index >= capacity) return null;
        return items[index];
    }

    /// <summary>
    /// 交换两个绝对位置的物品
    /// </summary>
    /// <param name="indexA"></param>
    /// <param name="indexB"></param>
    /// <returns></returns>
    public virtual bool SwapItem(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= capacity
         || indexB < 0 || indexB >= capacity) return false;

        var temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;
        return true;
    }

    /// <summary>
    /// 获取start到end的物品，闭区间
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public virtual T[] GetItemRange(int start, int end)
    {
        if (start >= capacity) return new T[0];

        int actualCount = Mathf.Min(end - start + 1, capacity - start);
        if (actualCount <= 0) return new T[0];

        T[] result = new T[actualCount];
        Array.Copy(items, start, result, 0, actualCount);
        return result;
    }

    public virtual void Clear()
    {
        Array.Fill(items, null);
    }
}
