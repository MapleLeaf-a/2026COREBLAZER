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
}
