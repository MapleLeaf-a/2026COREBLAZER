using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipType
{
    ShowCanvas, //显示原料不足的画布
}

public class AllRecipesViewModel : RecipesViewModel
{

    public event System.Action<TipType> OnShowTip;

    protected void ShowTip(TipType message)
    {
        OnShowTip?.Invoke(message);
    }

    protected AllRecipesModel allRecipesModel
    { 
        get => recipesModel as AllRecipesModel;
        set => recipesModel = value;
    }

    public AllRecipesViewModel(AllRecipesModel allRecipesModel, int itemsPerPage) : base(allRecipesModel, itemsPerPage)
    {

    }

    public override bool TryTransferTo(ViewModel<FoodRecipe> anotherBackpack, int fromInCurrent, int toInTarget)
    {
        if (anotherBackpack == null) return false;

        FoodRecipe fromItem = GetItemAt(fromInCurrent);
        FoodRecipe toItem = anotherBackpack.GetItemAt(toInTarget);

        if (fromItem == null) return false;


        if (toItem == null)
        {
            if (TestBackpack.instance.FreezerBackpackView.CheckFoodRecipeAble(fromItem))
            {
                anotherBackpack.AddItemAt(fromItem, toInTarget);
            }
            else
            {
                OnShowTip(TipType.ShowCanvas);
                return false;
            }
        }
        else //非空认为不可移动
        {
            return false;
        }

        return true;
    }

    
}
