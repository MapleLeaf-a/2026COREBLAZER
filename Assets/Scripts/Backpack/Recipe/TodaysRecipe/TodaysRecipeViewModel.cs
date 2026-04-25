using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TodaysRecipeViewModel : RecipesViewModel
{
    protected TodaysRecipeModel todaysRecipeModel
    { 
        get => recipesModel as TodaysRecipeModel;
        set => recipesModel = value;
    }

    public TodaysRecipeViewModel(TodaysRecipeModel todaysRecipesModel, int itemsPerPage) : base(todaysRecipesModel, itemsPerPage)
    {

    }

    public void Organize()
    {
        if (todaysRecipeModel.Organize())
        {
            OnPropertyChanged(nameof(CurrentPageItems));
        }
    }
}
