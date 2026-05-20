using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Foods
{
    //ʳ��id -> ʳ�� ��ӳ��
    private static Dictionary<string, Food> foods;

    static Foods()
    { 
        foods = new Dictionary<string, Food>() {
            { "step_004", new Food(FoodRecipes.LookUpFoodRecipe("step_004"))},
            //{ "step_009", new Food(FoodRecipes.LookUpFoodRecipe("step_009"))},
            //{ "step_010", new Food(FoodRecipes.LookUpFoodRecipe("step_010"))},    
            { "step_011", new Food(FoodRecipes.LookUpFoodRecipe("step_011"))},    
        };
    }

    /// <summary>
    /// ʳ��id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Food GetFood(string id)
    { 
        return foods[id];
    }


    /// <summary>
    /// ʳ��id�����ز�Ʒ�Ĵ���,��Ӧ��ʳ������ -> ������
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Dictionary<int, string> GetFoodEntry(string id)
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
