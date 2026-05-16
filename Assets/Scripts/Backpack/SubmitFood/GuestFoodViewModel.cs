using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestFoodViewModel : ViewModel<Food>
{
    protected GuestFoodModel guestFoodModel
    { 
        get => model as GuestFoodModel;
        set => model = value;
    }

    public GuestFoodViewModel(Model<Food> model, int itemsPerPage) : base(model, itemsPerPage)
    {

    }
}
