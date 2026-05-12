using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderIngredientsViewModel : ViewModel<FoodMaterial>
{
    protected OrderIngredientsModel orderIngredientsModel
    {
        get => model as OrderIngredientsModel;
        set => model = value;
    }

    public OrderIngredientsViewModel(OrderIngredientsModel orderIngredientsModel, int itemsPerPage) : base(orderIngredientsModel, itemsPerPage) { }
}
