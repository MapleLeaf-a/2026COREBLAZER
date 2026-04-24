using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TodaysRecipeUIItem : RecipesUIItem
{
    //²ËÆ·Ãû³Æ
    public TextMeshProUGUI recipeNameText;

    public void SetUp(FoodRecipe item, int index)
    {
        currentItem = item;
        slotIndex = index;

        if (item != null && recipeNameText != null)
        {
            recipeNameText.text = item.name;
        }
    }

    public override void Clear()
    {
        if (recipeNameText != null)
        {
            recipeNameText.text = "";
        }

        currentItem = null;
    }
}
