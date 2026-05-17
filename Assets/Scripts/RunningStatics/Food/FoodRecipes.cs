using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FoodRecipes
{
    //Ê³Æ×id -> FoodRecipe
    private static Dictionary<string, FoodRecipe> foodRecipes;

    //Ê³Æ×id -> ¸÷Ê³²ÄÊýÁ¿
    private static Dictionary<string, Dictionary<string, int>> ingredients;

    static FoodRecipes()
    { 
        ingredients = new Dictionary<string, Dictionary<string, int>>() {
            {"step_004", new Dictionary<string, int>(){ { "step_001", 2 }, { "step_002", 2 }, { "step_003", 1 } }  },
            {"step_009", new Dictionary<string, int>(){ { "step_006", 1 }, { "step_007", 1 }, { "step_008", 1 } } },
            {"step_010", new Dictionary<string, int>(){ { "step_005", 2 }, } },
            {"step_011", new Dictionary<string, int>() }
        };

        foodRecipes = new Dictionary<string, FoodRecipe>() {
            {"step_004", new FoodRecipe(150, "step_004", "ÇáÖËÄÞ»ÔÎÕ", "Images/Recipes/Sushi", ingredients["step_004"])},
            {"step_009", new FoodRecipe(130, "step_009", "¸¡µÆÒ»Õµ", "Images/Recipes/seaweed", ingredients["step_009"])},
            {"step_010", new FoodRecipe(140, "step_010", "ÐÇ»·ÆÑÉÕ´®", "Images/Recipes/Soup", ingredients["step_010"])},
            {"step_011", new FoodRecipe(10, "step_011", "ÂÌ²è", "Images/Recipes/Water", ingredients["step_011"])},
        };
    }

    /// <summary>
    /// Ê³Æ×id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static FoodRecipe LookUpFoodRecipe(string id)
    { 
        return foodRecipes[id];
    }

    /// <summary>
    /// Ê³Æ×id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Dictionary<string, int> LookUpIngredients(string id)
    { 
        return ingredients[id];
    }
}
