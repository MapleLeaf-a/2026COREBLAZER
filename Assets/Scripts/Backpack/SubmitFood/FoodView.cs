using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodView : View<Food, FoodUIItem>
{
    public FoodViewModel foodViewModel
    { 
        get => viewModel as FoodViewModel;
        set => viewModel = value;
    }

    public virtual void InitBackpackView(FoodModel foodModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        capacity = foodModel.Capacity;
        this.foodViewModel = new FoodViewModel(foodModel, itemsPerPage);
    }



    public override void RefreshUI()
    {
        Food[] items = foodViewModel.CurrentPageItems;
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                bool isSelected = (foodViewModel.SelectedItem == items[i]);
                slots[i].SetUp(items[i], foodViewModel.GetSprite(items[i].foodRecipe.spritePath), i, isSelected);
            }
            else //不包含在items里的清除槽位的显示效果
            {
                slots[i].Clear();
            }
        }
    }

    protected override void BindOtherButtons()
    {
        
    }

    //拖拽相关(由Drag Handler调用,这种设计使得视图和拖拽逻辑分离)
    private int draggingIndex = -1;
    public override void OnDragStart(int index)
    {
        draggingIndex = index;  //记录开始拖拽的槽位

        sourceView = this; //记录源头是自己

        Debug.Log($"拖拽开始: 索引={index}, 源背包={name}");
    }

    public override void OnDragEnd()
    {
        draggingIndex = -1; //拖拽结束清空记录

        Debug.Log("拖拽结束");
    }

    public override void OnDrop(int targetIndex)
    {
        if (DragState<Food, FoodUIItem>.FromIndex == -1)
        {
            return; //如果没有正在拖拽的物品,返回
        }

        //不可移动到此背包

        DragState<Food, FoodUIItem>.Reset();  //清空拖拽状态
    }
}
