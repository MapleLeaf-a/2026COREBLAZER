using StaticTemplates.MusicGame;
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

    //预计营业额
    protected int respectedTurnover = 0;

    /// <summary>
    /// 预计营业额
    /// </summary>
    public int RespectedTurnover => respectedTurnover;

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

    public int CaculateRespectedTurnover()
    {
        respectedTurnover = todaysRecipeModel.CaculateRespectedTurnover();
        OnPropertyChanged(nameof(RespectedTurnover));
        return respectedTurnover;
    }

    public bool RemoveRecipeAndReturnIngredients(int index)
    {
        FoodRecipe foodRecipe = GetItemAt(index);
        if (foodRecipe != null)
        {
            foreach (var item in foodRecipe.ingredients)
            {
                FoodMaterial foodMaterial = FoodMaterials.LookUpFoodMaterial(item.Key);
                TestBackpack.instance.FreezerBackpackView.backpackViewModel.AddItem(new Statics.Classes.BagItem(foodMaterial, item.Value));
            }
            return RemoveItemAt(index);
        }
        else
        {
            Debug.Log("拖拽位为空！");
            return false;
        }
    }

   
}
