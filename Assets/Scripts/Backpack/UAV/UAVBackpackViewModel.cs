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
}
