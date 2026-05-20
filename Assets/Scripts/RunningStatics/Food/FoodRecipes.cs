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
			{"step_004", new Dictionary<string, int>(){ { "step_001", 2 }, { "step_002", 2 }, { "step_003", 1 } }  },
            //{"step_009", new Dictionary<string, int>(){ { "step_006", 1 }, { "step_007", 1 }, { "step_008", 1 } } },
            //{"step_010", new Dictionary<string, int>(){ { "step_005", 2 }, } },
            {"step_011", new Dictionary<string, int>() }
		};

		foodRecipes = new Dictionary<string, FoodRecipe>() {
			{"step_004", new FoodRecipe(150, "step_004", "轻炙霓辉握", "Images/Recipes/轻炙霓辉握", ingredients["step_004"])},
            //{"step_009", new FoodRecipe(130, "step_009", "浮灯一盏", "Images/Recipes/seaweed", ingredients["step_009"])},
            //{"step_010", new FoodRecipe(140, "step_010", "星环蒲烧串", "Images/Recipes/Soup", ingredients["step_010"])},
            {"step_011", new FoodRecipe(10, "step_011", "绿茶", "Images/Recipes/绿茶", ingredients["step_011"])},
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
