using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TodaysRecipeModel : RecipesModel
{
    public TodaysRecipeModel(int capacity) : base(capacity)
    { 
        
    }

    public bool Organize()
    {
        int j = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                var tmp = items[j];
                items[j] = items[i];
                items[i] = tmp;

                j++;
            }
        }

        return true;
    }
}
