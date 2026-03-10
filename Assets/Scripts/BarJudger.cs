using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BarJudger : MonoBehaviour
{
    //音轨数组
    public Transform[] barList;     

    public GameObject notePrefab;      

    private float spawnInterval = 0.8f; // 每多少秒生成一个节奏块
    private float spawnHeightOffset = 10f; // 生成时比 Bar 高出多少

    //音符速度
    private float noteSpeed = 5f;

    //判定时间
    public float perfect = 0.04f; //<40ms
    public float good = 0.08f; //<80ms
    public float soso = 0.12f; //<120ms
    public float miss = 0.18f; //<180ms


    public List<List<Note>> noteList;

    private float timer = 0f;

    //跳字文本text数组
    public PopUpText[] textMeshList;

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


    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnRandomNote();
        }

        //
        if (Input.GetKeyDown(KeyCode.A))
        {
            Note note = GetNote(0);
            if (note != null)
                note.JudgeTime();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Note note = GetNote(1);
            if (note != null)
                note.JudgeTime();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            Note note = GetNote(2);
            if (note != null)
                note.JudgeTime();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Note note = GetNote(3);
            if (note != null)
                note.JudgeTime();
        }
    }

    void SpawnRandomNote()
    {
        //随机选择一条轨道
        int index = Random.Range(0, barList.Length);
        Transform bar = barList[index];

        //在该轨道上方生成节奏块
        Vector3 spawnPos = bar.position + new Vector3(0, spawnHeightOffset, 0);
        GameObject noteObj = Instantiate(notePrefab, spawnPos, notePrefab.transform.rotation);

        Note note = noteObj.GetComponent<Note>();
        note.speed = noteSpeed;
        note.barIndex = index;

        noteList[index].Add(note); //入队
    }

    public Note GetNote(int barIndex)
    {
        if (noteList[barIndex].Count != 0)
        {
            return noteList[barIndex][0];
        }
        else
        {
            Debug.LogWarning("此路已无音符！");
            return null;
        }
    }

    public void ShowText(int barIndex, string message, Color color, float duration = 0.1f)
    {
        textMeshList[barIndex].ShowText(message, color, duration);
    }
}