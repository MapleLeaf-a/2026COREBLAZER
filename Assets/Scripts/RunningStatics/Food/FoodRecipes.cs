using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FoodRecipes
{
    //食谱id -> FoodRecipe
    private static Dictionary<string, FoodRecipe> foodRecipes;

    //食谱id -> 各食材数量
    private static Dictionary<string, Dictionary<string, int>> ingredients;

    static FoodRecipes()
    { 
        ingredients = new Dictionary<string, Dictionary<string, int>>() {
            {"001", new Dictionary<string, int>(){ { "101", 3 } }  },
            {"002", new Dictionary<string, int>(){ { "101", 1 }, { "102", 1 }, { "103", 1 } } },
            {"003", new Dictionary<string, int>() }
        };

        foodRecipes = new Dictionary<string, FoodRecipe>() {
            {"001", new FoodRecipe(60, "001", "轻炙霓辉握", "Images/Recipes/Sushi", ingredients["001"])},
            {"002", new FoodRecipe(20, "002", "汤", "Images/Recipes/Soup", ingredients["002"])},
            {"003", new FoodRecipe(0, "003", "水", "Images/Recipes/Water", ingredients["003"])},
        };
    }

    /// <summary>
    /// 食谱id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static FoodRecipe LookUpFoodRecipe(string id)
    { 
        return foodRecipes[id];
    }

    /// <summary>
    /// 食谱id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Dictionary<string, int> LookUpIngredients(string id)
    { 
        return ingredients[id];
    }
}
