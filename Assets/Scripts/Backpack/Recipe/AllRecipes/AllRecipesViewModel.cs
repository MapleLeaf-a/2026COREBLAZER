using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllRecipesViewModel : RecipesViewModel
{
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
            anotherBackpack.AddItemAt(fromItem, toInTarget);
        }
        else //非空认为不可移动
        {
            return false;
        }

        return true;
    }

    
}
