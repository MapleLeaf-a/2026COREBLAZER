using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DescriptionEntry
{
    private static Dictionary<string, Sprite> descriptionEntrySprites;

    //静态构造函数
    static DescriptionEntry()
    {
        descriptionEntrySprites = new Dictionary<string, Sprite>(){
            {"美味的", Resources.Load<Sprite>("Images/Recipes/Entry/fresh") },

        };
    }

    public static Sprite GetSprite(string name)
    {
        if (descriptionEntrySprites.ContainsKey(name))
        {
            return descriptionEntrySprites[name];
        }
        return null;
    }
}
