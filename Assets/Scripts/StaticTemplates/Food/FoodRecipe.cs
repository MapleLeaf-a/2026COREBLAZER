using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 食谱
/// </summary>
public class FoodRecipe
{
    /// <summary>
    /// 基础价格
    /// </summary>
    public int basePrice;

    /// <summary>
    /// 菜品的图片路径
    /// </summary>
    public string spritePath;

    /// <summary>
    /// 菜品名称
    /// </summary>
    public string name;

    /// <summary>
    /// 需要的食材原料的id
    /// </summary>
    public List<string> ingredients;

    public FoodRecipe(int basePrice, string name, string spritePath, List<string> ingredients)
    { 
        this.basePrice = basePrice;
        this.spritePath = spritePath;
        this.name = name;
        this.ingredients = ingredients;
    }
}
