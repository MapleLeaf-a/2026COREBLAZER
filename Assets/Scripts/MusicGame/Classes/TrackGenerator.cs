using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator
{
    List<List<bool>> fishs = new List<List<bool>>()
    {
        new List<bool>() { true, true, false, false },
        new List<bool>() { true, false, true, false },
        new List<bool>() { true, false, false, true },
        new List<bool>() { false, true, true, false },
        new List<bool>() { false, true, false, true },
        new List<bool>() { false, false, true, true },
    };

    public void AddOneLine(List<List<bool>> notesPres)
    {
        //准备加入预设轨道的鱼对应的轨道的按键情况
        List<bool> l = fishs[Random.Range(0, fishs.Count)];
        for (int i = 0; i < 4; i++)
        {
            notesPres[i].Add(l[i]);
        }
    }
}
