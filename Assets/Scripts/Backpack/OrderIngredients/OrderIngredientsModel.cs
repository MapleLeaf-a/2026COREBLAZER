using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderIngredientsModel : Model<FoodMaterial>
{
    public OrderIngredientsModel(int capacity) : base(capacity) { }
}
