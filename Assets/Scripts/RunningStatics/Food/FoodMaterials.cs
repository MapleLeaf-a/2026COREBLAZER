using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StaticTemplates.MusicGame;

public static class FoodMaterials
{
    //食材id -> FoodMaterial
    private static Dictionary<string, FoodMaterial> allMaterials;

    static FoodMaterials()
    { 
        allMaterials = new Dictionary<string, FoodMaterial>() {
            {"101", new FoodMaterial("101", "KaZuHa", "Images/Recipes/kazuha", "枫原万叶,流浪的武士", StaticTemplates.Food.MaterialType.Drink) },
            {"102", new FoodMaterial("102", "Fish", "Images/Recipes/fish", "只是咸鱼", StaticTemplates.Food.MaterialType.Seafood) },
            {"103", new FoodMaterial("103", "Seaweed", "Images/Recipes/seaweed", "海草海草", StaticTemplates.Food.MaterialType.Seafood) },

        };
    }

    /// <summary>
    /// 给定食材id,返回食材
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static FoodMaterial LookUpFoodMaterial(string id)
    {
        return allMaterials[id];
    }
}
