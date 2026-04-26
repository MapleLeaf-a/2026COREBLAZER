using Statics.Classes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TodaysRecipeView : RecipesView
{
    [Header("预期营业额的Text组件")]
    public TextMeshProUGUI respectedTurnoverText;

    public TodaysRecipeViewModel todaysRecipeViewModel
    {
        get => viewModel as TodaysRecipeViewModel;

        set => viewModel = value; //value是C#属性setter中的上下文关键字,代表赋值操作传入的值
    }

    public virtual void InitBackpackView(TodaysRecipeModel recipesModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        capacity = recipesModel.Capacity;
        this.recipesViewModel = new TodaysRecipeViewModel(recipesModel, itemsPerPage);
    }
    protected override void BindOtherButtons()
    {

    }

    protected override void OnViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(todaysRecipeViewModel.CurrentPageItems):      //VM中此页物品刷新时
                RefreshItems(); //V刷新物品列表
                RefreshRespectedTurnoverText();
                break;
            case nameof(todaysRecipeViewModel.SelectedItem): //VM中选中的物品刷新时
                RefreshDetail();                         //V刷新详情面板
                RefreshItems();
                break;
            case nameof(todaysRecipeViewModel.CurrentPageNumber):
                break;
            case nameof(todaysRecipeViewModel.RespectedTurnover):
                RefreshRespectedTurnoverText();
                break;
        }
    }

    protected override void RefreshDetail()
    {
        
    }

    protected override void RefreshItems()
    {
        FoodRecipe[] items = recipesViewModel.CurrentPageItems;
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                bool isSelected = (recipesViewModel.SelectedItem == items[i]);
                (slots[i] as TodaysRecipeUIItem).SetUp(items[i], i);
            }
            else //不包含在items里的清除槽位的显示效果
            {
                slots[i].Clear();  //(slots[i] as TodaysRecipeUIItem).Clear(); //多态,应该是等价的
            }
        }
    }

    protected void RefreshRespectedTurnoverText()
    { 
        respectedTurnoverText.text = todaysRecipeViewModel.RespectedTurnover.ToString();
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
        if (DragState<FoodRecipe, RecipesUIItem>.FromIndex == -1)
        {
            return; //如果没有正在拖拽的物品,返回
        }

        if (DragState<FoodRecipe, RecipesUIItem>.SourceView == this) //若是同一个背包
        {
            //不是拖到同一个槽位
            if (draggingIndex != targetIndex)
            {
                //尝试移动物品
                bool success = recipesViewModel.TryMoveItem(draggingIndex, targetIndex);

                if (success)
                {
                    //整理当前背包
                    todaysRecipeViewModel.Organize();

                    RefreshUI(); //刷新界面
                }
            }
        }
        else //不是同一个背包
        {
            //尝试移动,注意是从源背包到目前背包
            var v = DragState<FoodRecipe, RecipesUIItem>.SourceView;
            bool success = (v as RecipesView).recipesViewModel.TryTransferTo(recipesViewModel, DragState<FoodRecipe, RecipesUIItem>.FromIndex, targetIndex);

            if (success)
            {
                //整理当前背包
                todaysRecipeViewModel.Organize();
                //计算预计营业额
                todaysRecipeViewModel.CaculateRespectedTurnover();

                //刷新两个背包的页面
                DragState<FoodRecipe, RecipesUIItem>.SourceView.RefreshUI();
                RefreshUI();
            }
        }

        DragState<FoodRecipe, RecipesUIItem>.Reset();  //清空拖拽状态
    }
}
