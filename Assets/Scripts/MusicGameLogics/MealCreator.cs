using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using Test;
using UnityEngine;

public class MealCreator : MonoBehaviour
{
    //每个食谱的预设音符列表
    List<MealNotes> mealNotesList;

    //需要进行打击的谱子的音符列表
    public List<MealNotes> currentMealNotesList = new List<MealNotes>();

    //单例
    public static MealCreator mealCreatorInstance;

    void Awake()
    {
        mealNotesList = JsonTest.GetMealNotes();
        for (int i = 0; i < mealNotesList.Count; i++)
        {
            MealNotes mealNotes = mealNotesList[i];
            if (mealNotes.track1.Count != mealNotes.track2.Count || mealNotes.track1.Count != mealNotes.track3.Count || mealNotes.track1.Count != mealNotes.track4.Count)
            {
                throw new UnityException("MealNote各轨道的长度不同！");
            }
        }

        if (mealCreatorInstance == null)
        {
            mealCreatorInstance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public List<MealNotes> GetCurrentMealNotesList()
    { 
        return currentMealNotesList;
    }

    public void AddMealNotes(int mealIndex)
    {
        if (mealIndex > mealNotesList.Count)
        {
            throw new UnityException("超出了给定菜谱的index范围！");
        }
        else
        {
            currentMealNotesList.Add(mealNotesList[mealIndex]);
        }
    }

    public void DeleteMealNotes(int quantity)
    {
        currentMealNotesList.RemoveRange(0, quantity);
    }
}
