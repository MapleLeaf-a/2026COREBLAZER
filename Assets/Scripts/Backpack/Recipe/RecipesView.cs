using JSONInterpreter.Tokens.Implement;
using Statics.Classes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class RecipesView : View<FoodRecipe, RecipesUIItem>
{
    public RecipesViewModel recipesViewModel
    {
        get => viewModel as RecipesViewModel;

        set => viewModel = value; //value是C#属性setter中的上下文关键字,代表赋值操作传入的值
    }

    public virtual void InitBackpackView(RecipesModel recipesModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        capacity = recipesModel.Capacity;
        this.recipesViewModel = new RecipesViewModel(recipesModel, itemsPerPage);
    }

    protected override void OnViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(recipesViewModel.CurrentPageItems):      //VM中此页物品刷新时
                RefreshItems(); //V刷新物品列表
                break;
            case nameof(recipesViewModel.SelectedItem): //VM中选中的物品刷新时
                RefreshDetail();                         //V刷新详情面板
                RefreshItems();
                break;
            case nameof(recipesViewModel.CurrentPageNumber):
                break;
        }
    }

    protected virtual void RefreshItems()
    {
        FoodRecipe[] items = recipesViewModel.CurrentPageItems;
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                bool isSelected = (recipesViewModel.SelectedItem == items[i]);
                slots[i].SetUp(items[i], recipesViewModel.GetSprite(items[i].spritePath), i, isSelected);
            }
            else //不包含在items里的清除槽位的显示效果
            {
                slots[i].Clear();
            }
        }
    }

    protected abstract void RefreshDetail();


    public override void RefreshUI()
    {
        RefreshItems();
        RefreshDetail();
    }
}
