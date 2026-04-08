using Statics.Classes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BackpackViewModel : INotifyPropertyChanged
{
    private BackpackModel backpack;

    //当前页数
    private int currentPage = 0;
    //总页数
    private int totalPages;
    //每页含有的元素数量
    private int itemsPerPage;
    //当前页选中的物品在当前页的index
    private int selectecIndex = -1;

    //string到Sprite的映射,用于读取每个BagItem的图片
    private Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();


    public BackpackViewModel(BackpackModel backpackModel, int itemsPerPage)
    {
        this.backpack = backpackModel;
        this.itemsPerPage = itemsPerPage;
        this.totalPages = backpackModel.Capacity / itemsPerPage;

        //初始化读取BagItem的图片
        foreach (var bagItem in backpackModel.BagItems)
        {
            if (bagItem == null) continue;
            AddPairToSprites(bagItem.SpritePath);
        }
    }

    /// <summary>
    /// 获取当前页的所有物品
    /// </summary>
    /// <returns></returns>
    public BagItem[] CurrentPageItems
    {
        get
        {
            int start = currentPage * itemsPerPage;
            int end = (currentPage + 1) * itemsPerPage - 1;

            return backpack.GetItemRange(start, end);
        }
    }

    /// <summary>
    /// 获取当前页指定index的物品
    /// </summary>
    /// <returns></returns>
    public BagItem GetItemAt(int index)
    { 
        BagItem[] bagItems = CurrentPageItems;
        if (0 <= index && index < bagItems.Length)
        { 
            return bagItems[index];
        }
        return null;
    }

    /// <summary>
    /// 当前选中的物品
    /// </summary>
    public BagItem SelectedItem
    {
        get
        {
            BagItem[] currenPageItems = CurrentPageItems;
            if (selectecIndex >= 0 && selectecIndex < currenPageItems.Length)
            { return currenPageItems[selectecIndex]; }
            return null;
        }
    }

    /// <summary>
    /// 当前页的编号(从一开始)
    /// </summary>
    public int CurrentPageNumber => currentPage + 1;

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => totalPages;

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevPage => currentPage > 0;
    
    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => currentPage < totalPages - 1;

    /// <summary>
    /// 总物品数量
    /// </summary>
    public int TotalCount => backpack.Count;



    /// <summary>
    /// 添加物品
    /// </summary>
    public void AddItem(BagItem bagItem)
    {
        if (backpack.AddItem(bagItem)) //若添加成功
        {
            AddPairToSprites(bagItem.SpritePath);

            OnPropertyChanged(nameof(CurrentPageItems));
            OnPropertyChanged(nameof(TotalCount));
        }
    }

    /// <summary>
    /// 删除物品
    /// </summary>
    public void DeleteItem(int itemIndexInCurrentPage, int quantity = 1)
    {
        int indexInBackpack = currentPage * totalPages + itemIndexInCurrentPage;
        if (backpack.RemoveItemAt(indexInBackpack, quantity)) //若删除成功
        {
            OnPropertyChanged(nameof(CurrentPageItems));
            OnPropertyChanged(nameof(TotalCount));
        }
    }

    /// <summary>
    /// 选择物品
    /// </summary>
    public BagItem SelectItem(int index)
    {
        if (selectecIndex != index)
        {
            selectecIndex = index;

            OnPropertyChanged(nameof(SelectedItem));
        }

        return SelectedItem;
    }

    /// <summary>
    /// 下一页
    /// </summary>
    public void NextPage()
    {
        if (HasNextPage)
        {
            currentPage++;
            selectecIndex = -1;

            Debug.Log("下一页，currentPage：" + currentPage + " HasNextPage：" + HasNextPage + " HasPrevPage：" + HasPrevPage);

            OnPropertyChanged(nameof(CurrentPageItems));
            OnPropertyChanged(nameof(HasPrevPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(CurrentPageNumber));
            OnPropertyChanged(nameof(SelectedItem));
        }
    }

    /// <summary>
    /// 上一页
    /// </summary>
    public void PrevPage()
    {
        if (HasPrevPage)
        { 
            currentPage--;
            selectecIndex = -1;

            Debug.Log("上一页，currentPage：" + currentPage + " HasNextPage：" + HasNextPage + " HasPrevPage：" + HasPrevPage);

            OnPropertyChanged(nameof(CurrentPageItems));
            OnPropertyChanged(nameof(HasPrevPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(CurrentPageNumber));
            OnPropertyChanged(nameof(SelectedItem));
        }
    }

    /// <summary>
    /// 跳转到指定页
    /// </summary>
    public void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < totalPages && pageIndex != currentPage)
        {
            currentPage = pageIndex;
            selectecIndex = -1;

            OnPropertyChanged(nameof(CurrentPageItems));
            OnPropertyChanged(nameof(HasPrevPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(CurrentPageNumber));
            OnPropertyChanged(nameof(SelectedItem));
        }
    }



    /// <summary>
    /// 尝试移动物品(同一页内),返回是否成功
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public bool TryMoveItem(int from, int to)
    {
        BagItem fromItem = GetItemAt(from);
        BagItem toItem = GetItemAt(to);

        if (fromItem == null) return false;

        if (toItem != null && fromItem.ID == toItem.ID) //目标处有物品且可堆叠
        {
            toItem.IncreaseNum(fromItem.num);
            DeleteItem(from, fromItem.num);
        }
        else
        {
            backpack.SwapItem(currentPage * itemsPerPage + from, currentPage * itemsPerPage + to);
        }

        OnPropertyChanged(nameof(CurrentPageItems));
    
        return true;
    }

    private void RefreshAll()
    {
        OnPropertyChanged(nameof(CurrentPageItems));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasPrevPage));
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(CurrentPageNumber));
        OnPropertyChanged(nameof(SelectedItem));
    }


    //实现接口,MVVM的核心接口,在属性变化后通知UI更新
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) //[CallerMemberName] 编译器特性,编译时自动获取并填充调用者的属性名
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    //给sprites增加键值对
    private void AddPairToSprites(string path)
    {
        if (!sprites.ContainsKey(path))
        {
            sprites[path] = Resources.Load<Sprite>(path);
        }
    }

    public Sprite GetSprite(string path)
    {
        if (sprites.ContainsKey(path))
        {
            return sprites[path];
        }
        else 
        {
            throw new UnityException("图片路径不存在！");
        }
    }
}
