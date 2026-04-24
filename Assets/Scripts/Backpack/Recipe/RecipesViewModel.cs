using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipesViewModel : ViewModel<FoodRecipe>
{
    public RecipesViewModel(RecipesModel allRecipesModel, int itemsPerPage) : base(allRecipesModel, itemsPerPage) 
    { 
        
    }

    protected override void InitDictionarySprites()
    {
        foreach (var item in model.Items)
        {
            if (item == null) continue;
            AddPairToSprites(item.spritePath);
        }
    }


}
