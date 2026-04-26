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
}
