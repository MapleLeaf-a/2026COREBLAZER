using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator
{
    List<List<int>> fishs = new List<List<int>>()
    {
        new List<int>() { 1, 1, 0, 0 },
        new List<int>() { 1, 0, 1, 0 },
        new List<int>() { 1, 0, 0, 1 },
        new List<int>() { 0, 1, 1, 0 },
        new List<int>() { 0, 1, 0, 1 },
        new List<int>() { 0, 0, 1, 1 },
    };

    public void AddOneLine(List<List<int>> notesPres)
    {
        //准备加入预设轨道的鱼对应的轨道的按键情况
        List<int> l = fishs[Random.Range(0, fishs.Count)];
        for (int i = 0; i < 4; i++)
        {
            notesPres[i].Add(l[i]);
        }
    }
}
