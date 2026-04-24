using Statics.Classes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllRecipesView : RecipesView
{
    public AllRecipesViewModel allRecipesViewModel
    {
        get => viewModel as AllRecipesViewModel;

        set => viewModel = value; //value是C#属性setter中的上下文关键字,代表赋值操作传入的值
    }

    public virtual void InitBackpackView(AllRecipesModel recipesModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        capacity = recipesModel.Capacity;
        this.recipesViewModel = new AllRecipesViewModel(recipesModel, itemsPerPage);
    }

    [Header("详情面板")]
    [Tooltip("名称")]
    public TextMeshProUGUI itemNameText;
    [Tooltip("价格")]
    public TextMeshProUGUI itemPriceText;
    [Tooltip("图标")]
    public Image itemIconImage;
    [Tooltip("价格图标")]
    public Image priceIconImage;

    protected override void BindOtherButtons()
    {
        
    }

    protected override void RefreshDetail()
    {
        FoodRecipe item = allRecipesViewModel.SelectedItem;
        if (item != null)
        {
            itemIconImage.enabled = true;
            itemNameText.enabled = true;
            itemPriceText.enabled = true;
            priceIconImage.enabled = true;

            itemIconImage.sprite = allRecipesViewModel.GetSprite(item.spritePath);
            itemNameText.text = item.name;
            itemPriceText.text = item.basePrice.ToString();
        }
        else
        {
            itemIconImage.enabled = false;
            itemNameText.enabled = false;
            itemPriceText.enabled = false;
            priceIconImage.enabled = false;
        }
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
            //全部食谱同一背包内不可移动
        }
        else //不是同一个背包
        {
            //尝试移动,注意是从源背包到目前背包
            var v = DragState<FoodRecipe, RecipesUIItem>.SourceView;
            bool success = (v as RecipesView).recipesViewModel.RemoveItemAt(DragState<FoodRecipe, RecipesUIItem>.FromIndex);

            if (success)
            {
                //刷新两个背包的页面
                DragState<FoodRecipe, RecipesUIItem>.SourceView.RefreshUI();
                RefreshUI();
            }
        }

        DragState<FoodRecipe, RecipesUIItem>.Reset();  //清空拖拽状态
    }
}
