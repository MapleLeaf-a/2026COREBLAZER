using Statics.Classes;
using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezerViewModel : BackpackViewModel
{
    public FreezerViewModel(BackpackModel backpackModel, int itemsPerPage) : base(backpackModel, itemsPerPage)
    {

    }

    public bool CheckFoodRecipeAble(FoodRecipe foodRecipe)
    {
        backpack.Organize();
        foreach (var kvp in foodRecipe.ingredients)
        {
            BagItem bagItem = backpack.GetBagItemWithId(kvp.Key);
            if (bagItem == null || bagItem.num < kvp.Value)
            {
                return false;
            }
        }
        foreach (var kvp in foodRecipe.ingredients)
        {
            BagItem bagItem = backpack.GetBagItemWithId(kvp.Key);
            backpack.RemoveItem(bagItem, kvp.Value);
        }
        return true;
    }
}
