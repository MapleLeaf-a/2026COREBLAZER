using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UAVBackpackView : BackpackView
{
    [Header("")]
    [Tooltip("丢弃按钮")]
    public Button dropButton;

    [Tooltip("转移全部按钮")]
    public Button transferAllButton;

    [Tooltip("转移此种物品按钮")]
    public Button transferThisButton;

    [Tooltip("冰箱View")]
    public FreezerView freezerView;

    protected override void BindOtherButtons()
    {
        dropButton.onClick.AddListener(OnDropItemClick);

        transferAllButton.onClick.AddListener(OnTransferAllClick);
        transferThisButton.onClick.AddListener(OnTransferSelectedClick);
    }

    public override void InitBackpackView(BackpackModel backpackModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        capacity = backpackModel.Capacity;
        this.backpackViewModel = new UAVBackpackViewModel(backpackModel, itemsPerPage);  //向上造型(UAVbackpackVM -> BackpackVM)
    }

    void OnDropItemClick()
    {
        var selected = backpackViewModel.SelectedItem;
        if (selected == null) return;

        (backpackViewModel as UAVBackpackViewModel)?.DropItem(backpackViewModel.SelectedIndex);

        //RefreshUI();
    }

    void OnTransferAllClick()
    {
        if (freezerView == null) return;

        (backpackViewModel as UAVBackpackViewModel)?.TransferAllTo(freezerView.backpackViewModel);

        //RefreshUI();
        //freezerView.RefreshUI();
    }

    void OnTransferSelectedClick()
    {
        if (freezerView == null) return;

        (backpackViewModel as UAVBackpackViewModel)?.TransferSelectedTo(freezerView.backpackViewModel);
    }
}
