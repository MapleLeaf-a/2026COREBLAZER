using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class DescriptionEntry
{
    private static Dictionary<string, Sprite> descriptionEntrySprites;

    //静态构造函数
    static DescriptionEntry()
    {
        descriptionEntrySprites = new Dictionary<string, Sprite>(){
            {"深空货运的", Resources.Load<Sprite>("Images/Recipes/Entry/1") },
            {"充满惊喜的", Resources.Load<Sprite>("Images/Recipes/Entry/2") },
            {"菌林秘酿的", Resources.Load<Sprite>("Images/Recipes/Entry/3") },
            {"本真清雅的", Resources.Load<Sprite>("Images/Recipes/Entry/4") },
            {"小火慢煨的", Resources.Load<Sprite>("Images/Recipes/Entry/5") },
            {"火焰炙烤的", Resources.Load<Sprite>("Images/Recipes/Entry/6") },
            {"雨后初摘的", Resources.Load<Sprite>("Images/Recipes/Entry/7") },
            {"海妖祝福的", Resources.Load<Sprite>("Images/Recipes/Entry/8") },
            {"潮汐孕育的", Resources.Load<Sprite>("Images/Recipes/Entry/9") },
            {"极致美味的", Resources.Load<Sprite>("Images/Recipes/Entry/10") },
            {"星光闪耀的", Resources.Load<Sprite>("Images/Recipes/Entry/11") },
            {"无人敢尝的", Resources.Load<Sprite>("Images/Recipes/Entry/12") },
            {"只剩一口的", Resources.Load<Sprite>("Images/Recipes/Entry/13") },
            {"夜潜捕捞的", Resources.Load<Sprite>("Images/Recipes/Entry/14") },
            {"直立行走的", Resources.Load<Sprite>("Images/Recipes/Entry/15") },
            {"哭着跑来的", Resources.Load<Sprite>("Images/Recipes/Entry/16") },
            {"被偷吃过的", Resources.Load<Sprite>("Images/Recipes/Entry/17") },
            {"风沙磨砺的", Resources.Load<Sprite>("Images/Recipes/Entry/18") },
            {"神秘改良的", Resources.Load<Sprite>("Images/Recipes/Entry/19") },
            {"林中迷路的", Resources.Load<Sprite>("Images/Recipes/Entry/20") },
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

    /// <summary>
    /// 根据图片查找词条名字
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns></returns>
    public static string GetName(Sprite sprite)
    {
        foreach (var kvp in descriptionEntrySprites)
        {
            if (kvp.Value == sprite)
                return kvp.Key;
        }
        return null;
    }

    /// <summary>
    /// 打乱顺序后取前N个(Fisher-Yates洗牌)
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static List<Sprite> GetRandomSprites(int num)
    {
        var sourceList = descriptionEntrySprites.Values.ToList();

        num = Mathf.Min(num, sourceList.Count);

        //创建副本并打乱
        List<Sprite> shuffled = new List<Sprite>(sourceList);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffled.Count);
            Sprite temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        //取前numE个
        return shuffled.GetRange(0, num);
    }
}
