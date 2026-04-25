using Statics.Classes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ViewModel<T> : INotifyPropertyChanged where T : class
{
    public Model<T> model;

    //当前页数
    protected int currentPage = 0;
    //总页数
    protected int totalPages;
    //每页含有的元素数量
    protected int itemsPerPage;
    //当前页选中的物品在当前页的index
    protected int selectecIndex = -1;

    ////string到Sprite的映射,用于读取每个item的图片
    //protected Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    public ViewModel(Model<T> model, int itemsPerPage)
    { 
        this.model = model;
        this.itemsPerPage = itemsPerPage;
        this.totalPages = model.Capacity / itemsPerPage;

        InitDictionarySprites();
    }

    /// <summary>
    /// 获取当前页的所有物品
    /// </summary>
    /// <returns></returns>
    public T[] CurrentPageItems
    {
        get
        {
            int start = currentPage * itemsPerPage;
            int end = (currentPage + 1) * itemsPerPage - 1;

            return model.GetItemRange(start, end);
        }
    }

    /// <summary>
    /// 当前选中的物品
    /// </summary>
    public T SelectedItem
    {
        get
        {
            T[] currenPageItems = CurrentPageItems;
            if (selectecIndex >= 0 && selectecIndex < currenPageItems.Length)
            { return currenPageItems[selectecIndex]; }
            return null;
        }
    }

    /// <summary>
    /// 当前选中的物品在当前页的索引
    /// </summary>
    public int SelectedIndex => selectecIndex;

    /// <summary>
    /// 当前页的编号(从一开始)
    /// </summary>
    public int CurrentPageNumber => currentPage + 1;

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => totalPages;

    /// <summary>
    /// 添加物品
    /// </summary>
    public virtual void AddItem(T bagItem)
    {
        if (model.AddItem(bagItem)) //若添加成功
        {
            OnPropertyChanged(nameof(CurrentPageItems));
        }
    }

    /// <summary>
    /// 在指定绝对位置添加物品
    /// </summary>
    /// <param name="bagItem"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual void AddItemAt(T bagItem, int index)
    {
        if (model.AddItemAt(bagItem, index))
        {
            OnPropertyChanged(nameof(CurrentPageItems));
        }
    }

    /// <summary>
    /// 删除当前页index的物品
    /// </summary>
    /// <param name="itemIndexInCurrentPage"></param>
    /// <param name="quantity"></param>
    public virtual bool RemoveItemAt(int itemIndexInCurrentPage)
    {
        int indexInBackpack = currentPage * totalPages + itemIndexInCurrentPage;
        if (model.RemoveItemAt(indexInBackpack)) //若删除成功
        {
            OnPropertyChanged(nameof(CurrentPageItems));
            OnPropertyChanged(nameof(SelectedItem));
            return true;
        }
        return false;
    }

    /// <summary>
    /// 选择物品
    /// </summary>
    public virtual T SelectItem(int index)
    {
        if (selectecIndex != index)
        {
            selectecIndex = index;

            OnPropertyChanged(nameof(SelectedItem));
        }

        return SelectedItem;
    }

    /// <summary>
    /// 获取当前页指定index的物品
    /// </summary>
    /// <returns></returns>
    public virtual T GetItemAt(int index)
    {
        return model.GetItemAt(itemsPerPage * currentPage + index);
    }

    /// <summary>
    /// 尝试移动物品(同一页内)
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns>是否成功</returns>
    public virtual bool TryMoveItem(int from, int to)
    {
        T fromItem = GetItemAt(from);
        T toItem = GetItemAt(to);

        if (fromItem == null) return false;

        model.SwapItem(currentPage * itemsPerPage + from, currentPage * itemsPerPage + to);

        OnPropertyChanged(nameof(CurrentPageItems));

        return true;
    }

    /// <summary>
    /// 尝试向另一背包移动物品
    /// </summary>
    /// <param name="anotherBackpack"></param>
    /// <param name="fromInCurrent">本页index</param>
    /// <param name="toInTarget">目标背包的那页的index</param>
    /// <returns></returns>
    public virtual bool TryTransferTo(ViewModel<T> anotherBackpack, int fromInCurrent, int toInTarget)
    {
        if (anotherBackpack == null) return false;

        T fromItem = GetItemAt(fromInCurrent);
        T toItem = anotherBackpack.GetItemAt(toInTarget);

        if (fromItem == null) return false;


        if (toItem == null)
        {
            anotherBackpack.AddItemAt(fromItem, toInTarget);
        }
        else //非空认为不可移动
        {
            return false;
        }

        RemoveItemAt(fromInCurrent); //对前两种情况的原背包的更新

        return true;
    }

    public virtual void RefreshAll()
    {
        OnPropertyChanged(nameof(CurrentPageItems));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(CurrentPageNumber));
        OnPropertyChanged(nameof(SelectedItem));
    }

    //实现接口,MVVM的核心接口,在属性变化后通知UI更新
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) //[CallerMemberName] 编译器特性,编译时自动获取并填充调用者的属性名
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 初始化字典的图片
    /// </summary>
    protected virtual void InitDictionarySprites()
    { 
        
    }

    //给sprites增加键值对
    protected void AddPairToSprites(string path)
    {
        SpriteStatic.AddPairToSprites(path);
    }

    public Sprite GetSprite(string path)
    {
        return SpriteStatic.GetSprite(path);
    }
}
