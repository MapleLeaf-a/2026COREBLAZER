using StaticTemplates.MusicGame;
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

    //每个食谱的预设音符列表
    List<MealNotes> mealNotesList;

    //遍历MealNotes的index
    int indexOfMealNotes;

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

        mealNotesList = JsonTest.GetMealNotes();
        for (int i = 0; i < mealNotesList.Count; i++)
        {
            MealNotes mealNotes = mealNotesList[i];
            if (mealNotes.track1.Count != mealNotes.track2.Count || mealNotes.track1.Count != mealNotes.track3.Count || mealNotes.track1.Count != mealNotes.track4.Count)
            {
                throw new UnityException("MealNote各轨道的长度不同！");
            }
        }

        mealScores = new List<Score>(mealNotesList.Count);
        for (int i = 0; i < mealNotesList.Count; i++)
        {
            mealScores.Add(new Score(i));
        }
    }

    void Start()
    {
       
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            for (int i = 0; i < mealNotesList.Count; i++)
            {
                MealNotes mealNotes = mealNotesList[i];
                if (indexOfMealNotes < mealNotes.track1.Count)
                {
                    if (mealNotes.track1[indexOfMealNotes] == 1)
                    {
                        SpawnNote(0, i);
                        mealScores[i].AddNoteCount();
                    }
                    if (mealNotes.track2[indexOfMealNotes] == 1)
                    {
                        SpawnNote(1, i);
                        mealScores[i].AddNoteCount();
                    }
                    if (mealNotes.track3[indexOfMealNotes] == 1)
                    {
                        SpawnNote(2, i);
                        mealScores[i].AddNoteCount();
                    }
                    if (mealNotes.track4[indexOfMealNotes] == 1)
                    {
                        SpawnNote(3, i);
                        mealScores[i].AddNoteCount();
                    }
                }
                else 
                {
                    if (mealScores[i].GetFinalRate() == 0 && NoNotesOfMealXPending(i))
                    {
                        mealScores[i].ComputeFinalRate();
                        Debug.Log("菜品" + i + "的最终perfect率为：" + mealScores[i].GetFinalRate());
                    }
                }
            }
            indexOfMealNotes++;
        }

        //
        if (Input.GetKeyDown(KeyCode.A))
        {
            JudgeNoteAt(0);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            JudgeNoteAt(1);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            JudgeNoteAt(2);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            JudgeNoteAt(3);
        }
    }

    void SpawnNote(int barIndex, int mealIndex)
    {
        Transform bar = barList[barIndex];

        //在该轨道上方生成节奏块
        Vector3 spawnPos = bar.position + new Vector3(0, spawnHeightOffset, 0);
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
            Debug.LogWarning("在"+ barIndex +"获取" + noteIndex +"超出了范围！");
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

    public void ShowText(int barIndex, string message, Color color, float duration = 0f)
    {
        textMeshList[barIndex].ShowText(message, color, duration);
    }
}