using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Profiling.HierarchyFrameDataView;

public class UAVBackpackView : BackpackView<UAVBackpackViewModel>
{
    [Header("")]
    [Tooltip("丢弃按钮")]
    public Button dropButton;

    [Tooltip("转移全部按钮")]
    public Button transferAllButton;

    [Tooltip("冰箱View")]
    public BackpackView<BackpackViewModel> freezerView;

    protected override void BindOtherButtons()
    {
        dropButton.onClick.AddListener(OnDropItemClick);

        transferAllButton.onClick.AddListener(OnTransferAllClick);
    }

    protected override UAVBackpackViewModel CreateViewModel(BackpackModel model, int itemsPerPage)
    {
        return new UAVBackpackViewModel(model, itemsPerPage);
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
}
