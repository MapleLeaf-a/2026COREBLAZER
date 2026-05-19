using Statics.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAVBackpackViewModel : BackpackViewModel
{
    public UAVBackpackViewModel(BackpackModel backpackModel, int itemsPerPage) : base(backpackModel, itemsPerPage) //用base关键字调用父类构造函数
    { }

    /// <summary>
    /// 丢弃一格中的所有物品
    /// </summary>
    /// <param name="itemIndexInCurrentPage"></param>
    public void DropItem(int itemIndexInCurrentPage)
    {
        RemoveItemAt(itemIndexInCurrentPage, GetItemAt(itemIndexInCurrentPage).num);
        //RefreshAll(); //不用是因为BackpackViewModel.RemoveItemAt里面已经有通知更新了
    }

    /// <summary>
    /// 转移所有物品到另一个背包
    /// </summary>
    /// <param name="target"></param>
    public void TransferAllTo(BackpackViewModel target)
    {
        if (backpack.TransferAllTo(target.backpack))
        {
            RefreshAll();
            target.RefreshAll();
        }
    }

    /// <summary>
    /// 转移选中的物品到另一个背包（自动找空位）
    /// </summary>
    /// <param name="target">目标背包</param>
    public void TransferSelectedTo(BackpackViewModel target)
    {
        if (target == null) return;

        //获取当前选中的物品
        BagItem selectedItem = SelectedItem;
        if (selectedItem == null)
        {
            return;
        }

        //获取选中物品在当前页的索引
        int fromIndex = SelectedIndex;
        if (fromIndex < 0) return;

        //在目标背包中找一个空位
        int targetIndex = target.FindFirstEmptySlot();

        if (targetIndex == -1)
        {
            return;
        }

        //执行转移
        bool success = TryTransferTo(target, fromIndex, targetIndex);

        if (success)
        {
            //刷新两个背包的 UI
            RefreshAll();
            target.RefreshAll();
        }
    }
}
