using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Foods
{
    //食谱id -> 食物 的映射
    private static Dictionary<string, Food> foods;

    static Foods()
    { 
        foods = new Dictionary<string, Food>() {
            { "001", new Food(FoodRecipes.LookUpFoodRecipe("001"))},
            { "002", new Food(FoodRecipes.LookUpFoodRecipe("002"))},
            { "003", new Food(FoodRecipes.LookUpFoodRecipe("003"))},    
        };
    }

    /// <summary>
    /// 食谱id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Food GetFood(string id)
    { 
        return foods[id];
    }


    /// <summary>
    /// 食谱id，返回菜品的词条,对应的食材索引 -> 词条索引
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Dictionary<int, int> GetFoodEntry(string id)
    {
        return foods[id].entry;
    }

    public static void SetFoodStar(string id, int star)
    {
        foods[id].star = star;
    }

    public static int GetFoodStar(string id)
    {
        return foods[id].star;
    }
}
