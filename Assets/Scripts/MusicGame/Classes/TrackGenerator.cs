using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator
{
    List<List<int>> fishs = new List<List<int>>()
    {
        new List<int>() { 1, 1, 0, 0 }, //刺球蟹 
        new List<int>() { 1, 0, 1, 0 }, //团叶果母
        new List<int>() { 1, 0, 0, 1 }, //霓辉鳍鱼
        new List<int>() { 0, 1, 1, 0 }, //玻壳海胆
        new List<int>() { 0, 1, 0, 1 }, //星环鳗鱼
        new List<int>() { 0, 0, 1, 1 }, //泡泡糖海龙
    };

    public static List<string> index2id = new List<string>() { "step_011", "step_008", "step_001", "step_009", "step_005", "step_010" };

    public string AddOneLine(List<List<int>> notesPres)
    {
        //准备加入预设轨道的鱼对应的轨道的按键情况
        int fishIdx = Random.Range(0, fishs.Count);
        List<int> l = fishs[fishIdx];
        for (int i = 0; i < 4; i++)
        {
            notesPres[i].Add(l[i]);
        }
        return index2id[fishIdx];
    }
}
