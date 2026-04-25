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
    /// 菜品ID,唯一标识菜品种类
    /// </summary>
    public string id;

    /// <summary>
    /// 菜品名称
    /// </summary>
    public string name;

    /// <summary>
    /// 需要的食材原料的id和数量
    /// </summary>
    public Dictionary<string, int> ingredients;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="basePrice"></param>
    /// <param name="id">食谱id</param>
    /// <param name="name"></param>
    /// <param name="spritePath"></param>
    /// <param name="ingredients">string:食材原料id, int:数量</param>
    public FoodRecipe(int basePrice, string id, string name, string spritePath, Dictionary<string, int> ingredients)
    { 
        this.basePrice = basePrice;
        this.spritePath = spritePath;
        this.id = id;
        this.name = name;
        this.ingredients = ingredients;
    }
}
