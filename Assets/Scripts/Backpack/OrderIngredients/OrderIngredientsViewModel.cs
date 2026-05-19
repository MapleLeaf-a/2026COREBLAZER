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

    //打折的物品的索引
    private int discountIndex = -1;

    public int DiscountIndex => discountIndex;

    public void GenerateDiscountIndex()
    {
        discountIndex = Random.Range(0, Count);
        OnPropertyChanged(nameof(DiscountIndex));
    }
}
