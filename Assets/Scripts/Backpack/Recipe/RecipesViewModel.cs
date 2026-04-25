using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipesViewModel : ViewModel<FoodRecipe>
{
    protected RecipesModel recipesModel
    { 
        get => model as RecipesModel;
        set => model = value;
    }

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
