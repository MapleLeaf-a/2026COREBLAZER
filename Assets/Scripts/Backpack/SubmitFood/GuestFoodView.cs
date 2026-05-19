using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestFoodView : View<Food, FoodUIItem>
{

    public Canvas SubmitFoodCanvas;

    public GuestFoodViewModel guestFoodViewModel
    { 
        get => viewModel as GuestFoodViewModel;
        set => viewModel = value;
    }

    public virtual void InitBackpackView(GuestFoodModel guestFoodModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        capacity = guestFoodModel.Capacity;
        this.guestFoodViewModel = new GuestFoodViewModel(guestFoodModel, itemsPerPage);
    }

    public override void RefreshUI()
    {
        Food[] items = guestFoodViewModel.CurrentPageItems;
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                bool isSelected = (guestFoodViewModel.SelectedItem == items[i]);
                slots[i].SetUp(items[i], guestFoodViewModel.GetSprite(items[i].foodRecipe.spritePath), i, isSelected);
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
        
    }

    public override void OnDragEnd()
    {
        
    } 

    public override void OnDrop(int targetIndex)
    {
        if (DragState<Food, FoodUIItem>.FromIndex == -1)
        {
            return; //如果没有正在拖拽的物品,返回
        }

        if (DragState<Food, FoodUIItem>.SourceView == this) //若是同一个背包
        {
            //同一背包内不可移动
        }
        else if (DragState<Food, FoodUIItem>.SourceView as FoodView != null) 
        {
            //尝试移动,注意是从源背包到目前背包
            var v = DragState<Food, FoodUIItem>.SourceView as FoodView;
            int fromIndex = DragState<Food, FoodUIItem>.FromIndex;
            guestFoodViewModel.AddItemAt(v.foodViewModel.GetItemAt(fromIndex), targetIndex);
            Food food = v.foodViewModel.GetItemAt(fromIndex);
            MoneyManager.IncreaseMoney(food.foodRecipe.basePrice);
            bool success = v.foodViewModel.RemoveItemAt(fromIndex);

            if (success)
            {
                //刷新两个背包的页面
                DragState<Food, FoodUIItem>.SourceView.RefreshUI();
                RefreshUI();

                if (v.foodViewModel.Count == 0)
                {
                    SubmitFoodCanvas.gameObject.SetActive(false);
                }
            }
        }

        DragState<Food, FoodUIItem>.Reset();  //清空拖拽状态
    }
}
