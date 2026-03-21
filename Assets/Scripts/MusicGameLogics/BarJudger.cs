using StaticTemplates.MusicGame;
using System;
using System.Collections.Generic;
using Test;
using UnityEngine;

public class BarJudger : MonoBehaviour
{
    //音轨数组
    public Transform[] barList;     

    public GameObject notePrefab;      

    private float spawnInterval = 0.8f; // 每多少秒生成一个节奏块
    private float spawnHeightOffset = 10f; // 生成时比 Bar 高出多少

    //判定时间
    public float perfect = 1f; //<40ms
    public float good = 0.08f; //<80ms
    public float soso = 0.12f; //<120ms
    public float miss = 0.18f; //<180ms

    //音符列表
    public List<List<Note>> noteList;

    private float timer = 0f;

    //跳字文本text数组
    public PopUpText[] textMeshList;

    //需要打谱的音符表
    List<MealNotes> currentMealNotesList;
    //是否已经结束所有的打谱
    bool over;
    //最大能同时进行的谱子数量
    const int maxMealQuantity = 2;
    //遍历currentMealNotesList的index
    int indexOfMealNotesList;
    //遍历每个MealNotes的index
    int indexOfMealNotes;

    //需要打谱的音符表对应的mealNotesIndex的List
    List<int> mealNotesIndexList = new List<int>();

    //画布父物体
    public Canvas canvas;

    //分数列表
    public List<Score> mealScores;

    //单例
    public static BarJudger BarJudgerInstance { get; private set; }

    void Awake()
    {
        noteList = new List<List<Note>>();
        for (int i = 0; i < barList.Length; i++)
        {
            noteList.Add(new List<Note>()); // 为每个轨道创建一个空的 Note 列表
        }

        if (BarJudgerInstance == null)
        {
            BarJudgerInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
       
    }

    void Update()
    {
        //当前未有需要进行打谱的列表
        if (currentMealNotesList == null)
        {
            
        }
        else
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;

                if (indexOfMealNotesList < currentMealNotesList.Count)
                {
                    //indexOfMealNotesList + i，每次遍历的谱子的index
                    for (int i = 0; i < maxMealQuantity; i++)
                    {
                        int index = indexOfMealNotesList + i;
                        if (index >= currentMealNotesList.Count)
                        {
                            over = true;
                            break;
                        }
                        else
                        {
                            MealNotes mealNotes = currentMealNotesList[index];
                            if (indexOfMealNotes < mealNotes.track1.Count)
                            {
                                if (mealNotes.track1[indexOfMealNotes] == 1)
                                {
                                    SpawnNote(0, index);
                                    mealScores[index].AddNoteCount();
                                }
                                if (mealNotes.track2[indexOfMealNotes] == 1)
                                {
                                    SpawnNote(1, index);
                                    mealScores[index].AddNoteCount();
                                }
                                if (mealNotes.track3[indexOfMealNotes] == 1)
                                {
                                    SpawnNote(2, index);
                                    mealScores[index].AddNoteCount();
                                }
                                if (mealNotes.track4[indexOfMealNotes] == 1)
                                {
                                    SpawnNote(3, index);
                                    mealScores[index].AddNoteCount();
                                }
                            }
                            else
                            {
                                if (mealScores[index].GetFinalRate() == 0 && NoNotesOfMealXPending(index))
                                {
                                    mealScores[index].ComputeFinalRate();
                                    Debug.Log("菜品" + mealNotesIndexList[index] + "的最终perfect率为：" + mealScores[index].GetFinalRate());
                                }
                            }
                        }
                    }

                    indexOfMealNotes++;
                    if (NoNotesPending())
                    {
                        indexOfMealNotesList += maxMealQuantity;
                        indexOfMealNotes = 0;
                    }
                }
                else
                {
                    over = true;
                }

                if (over && NoNotesPending())
                {
                    InitState();
                    Debug.Log("当前谱子已全部结束");
                    canvas.gameObject.SetActive(false);
                    InputManager.InputManagerInstance.SetContext(InputManager.InputContext.CHARACTER);
                    return;
                }
            }

            //
            if (InputManager.InputManagerInstance.GetKeyDown("bar1"))
            {
                JudgeNoteAt(0);
            }
            if (InputManager.InputManagerInstance.GetKeyDown("bar2"))
            {
                JudgeNoteAt(1);
            }
            if (InputManager.InputManagerInstance.GetKeyDown("bar3"))
            {
                JudgeNoteAt(2);
            }
            if (InputManager.InputManagerInstance.GetKeyDown("bar4"))
            {
                JudgeNoteAt(3);
            }
        }
    }

    void SpawnNote(int barIndex, int mealIndex)
    {
        Transform bar = barList[barIndex];

        //在该轨道上方生成节奏块
        Vector3 spawnPos = bar.position + new Vector3(0, spawnHeightOffset, 0);

        Color color;
        switch (mealNotesIndexList[mealIndex])
        { 
            case 0:
                color = Color.black;
                break;
            case 1:
                color = Color.white;
                break;
            case 2:
                color = Color.blue;
                break;
            case 3:
                color = Color.green;
                break;
            default:
                color = Color.red;
                break;
        }
        notePrefab.GetComponent<SpriteRenderer>().color = color;
        
        GameObject noteObj = Instantiate(notePrefab, spawnPos, notePrefab.transform.rotation, canvas.transform);

        //音符速度
        float noteSpeed = 5f;

        Note note = noteObj.GetComponent<Note>();
        note.speed = noteSpeed;
        note.barIndex = barIndex;
        note.mealIndex = mealIndex;

        noteList[barIndex].Add(note); //入队
    }

    public Note GetNote(int barIndex)
    {
        //if (noteList[barIndex].Count != 0)
        //{
        //    return noteList[barIndex][0];
        //}
        //else
        //{
        //    Debug.LogWarning("此路已无音符！");
        //    return null;
        //}
        return GetNote(barIndex, 0);
    }

    public Note GetNote(int barIndex, int noteIndex)
    {
        if (noteList[barIndex].Count > noteIndex)
        {
            return noteList[barIndex][noteIndex];
        }
        else
        {
            //Debug.LogWarning("在"+ barIndex +"获取" + noteIndex +"超出了范围！");
            return null;
        }
    }

    public void JudgeNoteAt(int barIndex)
    {
        int noteIndex = 0;
        Note note = GetNote(barIndex, noteIndex);
        while (note != null && noteIndex < noteList[barIndex].Count && note.JudgeTime())
        {
            note = GetNote(barIndex, noteIndex);
        }
    }

    //在mealIndex号菜谱没有未判定的音符
    private bool NoNotesOfMealXPending(int mealIndex)
    {
        for (int i = 0; i < noteList.Count; i++)
        {
            for (int j = 0; j < noteList[i].Count; j++)
            { 
                if (noteList[i][j].mealIndex == mealIndex) return false;
            }
        }
        return true;
    }

    //整个谱中没有未判定的音符
    private bool NoNotesPending()
    {
        for (int i = 0; i < noteList.Count; i++)
        {
            if (noteList[i].Count != 0) return false;
        }
        return true;
    }

    //重置状态
    void InitState()
    {
        currentMealNotesList = null; 
        mealScores = new List<Score>();
        over = false;
        indexOfMealNotesList = 0;
    }

    public void CreateMealNotesList(List<int> indexsOfMealNotes)
    {
        canvas.gameObject.SetActive(true);

        MealCreator mealCreatorInstance = new MealCreator();
        for (int i = 0; i < indexsOfMealNotes.Count; i++)
        {
            if (i >= MealCreator.mealNotesList.Count)
            {
                throw new UnityException("给定的index超出了预设菜谱列表的长度");
            }
            mealCreatorInstance.AddMealNotes(indexsOfMealNotes[i]);
        }
        mealNotesIndexList = indexsOfMealNotes;
        currentMealNotesList = mealCreatorInstance.GetCurrentMealNotesList();

        mealScores = new List<Score>();
        for (int i = 0; i < currentMealNotesList.Count; i++)
        {
            mealScores.Add(new Score(i));
        }
    }


    public void ShowText(int barIndex, string message, Color color, float duration = 0f)
    {
        textMeshList[barIndex].ShowText(message, color, duration);
    }
}