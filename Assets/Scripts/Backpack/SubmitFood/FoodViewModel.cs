using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodViewModel : ViewModel<Food>
{
    protected FoodModel foodModel
    { 
        get => model as FoodModel;
        set => model = value;
    }

    public FoodViewModel(FoodModel foodModel, int itemsPerPage) : base(foodModel, itemsPerPage)
    { 
        
    }

    public void UpdateFoods(FoodRecipe[] foodRecipes)
    {
        foodModel.Clear();

        foreach (var item in foodRecipes)
        {
            if (item != null)
            {
                foodModel.AddItem(Foods.GetFood(item.id));
            }
        }
    }
}
