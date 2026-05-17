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
            {"step_001", new FoodMaterial("step_001", 32, "霓辉鳍鱼", "Images/Recipes/霓辉鳍鱼", "身体纤细流畅的鱼类，鱼鳍大而薄，像一层层会折光的彩膜。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_002", new FoodMaterial("step_002", 10, "明太子", "Images/Recipes/明太子", "撕开后有一串串半透明小卵囊，颜色很浅，像微微发亮的珠子。拾起后周围散出一圈圈潮水状光纹。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_003", new FoodMaterial("step_003", 7, "贝汤亮油", "Images/Recipes/贝汤亮油", "一种瓶装的，清亮、略带珠光的油脂液体。", StaticTemplates.Food.MaterialType.Drink)},
            {"step_005", new FoodMaterial("step_005", 35, "星环鳗鱼", "Images/Recipes/星环鳗鱼", "一种细长身形的鳗鱼，身体表面有一圈圈规律分布的环纹。环纹在暗处会略微发亮，像一节节光环套在身体上。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_006", new FoodMaterial("step_006", 17, "星沫囊泡", "Images/Recipes/星沫囊泡", "一簇簇附着在伞背后的半透明囊体，大小如鱼丸。整体偏浅蓝或浅紫色，表面有微光。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_007", new FoodMaterial("step_007", 20, "白潮果", "Images/Recipes/白潮果", "一种乳白色外壳的大小似鸡蛋的硬质果实，根深埋在盐滩之中。", StaticTemplates.Food.MaterialType.Vegetable)},
            {"step_008", new FoodMaterial("step_008", 28, "团叶果母", "Images/Recipes/团叶果母", "漂浮在水面上的团状果体结合物，外面拖着一层层圆润叶片。", StaticTemplates.Food.MaterialType.Vegetable)},
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
