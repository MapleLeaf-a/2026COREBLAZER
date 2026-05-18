using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezerView : BackpackView
{
    public FreezerViewModel freezerViewModel
    { 
        get => viewModel as FreezerViewModel;
        set => viewModel = value;
    }

    public override void InitBackpackView(BackpackModel backpackModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        capacity = backpackModel.Capacity;
        this.backpackViewModel = new FreezerViewModel(backpackModel, itemsPerPage);
    }


    protected override void BindOtherButtons() { }

    

    /// <summary>
    /// 检测当前背包是否拥有足够数量的recipe所需的食材，若有则移除
    /// </summary>
    /// <param name="foodRecipe"></param>
    /// <returns></returns>
    public bool CheckFoodRecipeAble(FoodRecipe foodRecipe)
    {
        return freezerViewModel.CheckFoodRecipeAble(foodRecipe);
    }
}
