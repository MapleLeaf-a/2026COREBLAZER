using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food
{
    /// <summary>
    /// 菜品星级
    /// </summary>
    public int star;

    /// <summary>
    /// 菜品对应食谱
    /// </summary>
    public FoodRecipe foodRecipe;

    /// <summary>
    /// 菜品的词条, 对应的食材索引 -> 词条名
    /// </summary>
    public Dictionary<int, string> entry;

    public Food(FoodRecipe foodRecipe)
    { 
        this.foodRecipe = foodRecipe;
        entry = new Dictionary<int, string>();
        for (int i = 0; i < foodRecipe.ingredients.Count; i++)
        {
            entry.Add(i, "");
        }
    }
}
