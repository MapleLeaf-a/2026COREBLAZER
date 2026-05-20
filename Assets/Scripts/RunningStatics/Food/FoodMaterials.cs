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
            {"step_009", new FoodMaterial("step_009", 28, "玻壳海胆", "Images/Recipes/玻壳海胆", "外壳为半透明玻璃球壳的海胆状生物，能直接透过其看见内部柔软的海胆芯。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_010", new FoodMaterial("step_010", 30, "泡泡糖海龙", "Images/Recipes/泡泡糖海龙", "身体细长卷曲的海龙状生物。表层质感半透明、带弹性，颜色通体明亮多彩，局部还会鼓起小泡室。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_012", new FoodMaterial("step_012", 30, "刺球蟹", "Images/Recipes/刺球蟹", "刺球状的生物，样貌与螃蟹有些相似，口感却完全不同。第一个尝试将这种生物当作食材的是谁呢？", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_013", new FoodMaterial("step_013", 28, "极光贝", "Images/Recipes/极光贝", "表面带有虹彩光泽的贝类，像一层极光涂在外壳。受热后的贝肉也会泛出柔和的虹彩。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_014", new FoodMaterial("step_014", 23, "淘砂根", "Images/Recipes/淘砂根", "埋在盐滩下的块根类食材，挖出来时表面裹着湿盐和细砂。整体为长条的浅色根块，切开后内部如磨砂，带细小晶粒。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_015", new FoodMaterial("step_015", 5, "砂盐", "Images/Recipes/砂盐", "带些许珍珠光泽的浅色食盐。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_016", new FoodMaterial("step_016", 8, "花蜜酱", "Images/Recipes/花蜜酱", "浅琥珀色的甜蜜酱料，质地柔亮黏润。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_017", new FoodMaterial("step_017", 18, "裂壳芽菜", "Images/Recipes/裂壳芽菜", "深埋在盐滩之中的芽菜，形似包着一层矿质外壳的嫩笋。表面有不规则灰白壳片。", StaticTemplates.Food.MaterialType.Seafood)},
            {"step_018", new FoodMaterial("step_018", 27, "盐蜕", "Images/Recipes/盐蜕", "外层包着坚硬盐壳的贝类。剥开以后里面是柔软、紧实、湿润的贝肉。", StaticTemplates.Food.MaterialType.Seafood)},
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
