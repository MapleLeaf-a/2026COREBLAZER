using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BarJudger : MonoBehaviour
{
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


    private List<List<GameObject>> noteList;

    private float timer = 0f;

    //单例
    public static BarJudger BarJudgerInstance { get; private set; }

    void Awake()
    {
        noteList = new List<List<GameObject>>(barList.Length);
        for (int i = 0; i < barList.Length; i++)
        {
            noteList.Add(new List<GameObject>()); // 为每个轨道创建一个空的 Note 列表
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
            JudgeTime(0);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            JudgeTime(1);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            JudgeTime(2);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            JudgeTime(3);
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

        noteList[index].Add(noteObj); //入队
    }

    //判断时机
    public void JudgeTime(int barIndex)
    {
        Transform bar = barList[barIndex];
        float baseY = bar.position.y;

        if (noteList[barIndex].Count != 0)
        {
            GameObject noteObj = noteList[barIndex][0];
            Note note = noteObj.GetComponent<Note>();
            float y = note.transform.position.y;

            if (baseY + note.speed * miss < y)  //还未到判定区
            {
                return;
            }
            else if (baseY - note.speed * miss > y) //已过判定区
            {
                noteList[barIndex].RemoveAt(0);
                Destroy(noteObj);
                while (noteList[barIndex].Count != 0) //如果还有剩下的块
                {
                    noteObj = noteList[barIndex][0];
                    note = noteObj.GetComponent<Note>();
                    y = note.transform.position.y;
                    if (baseY - note.speed * miss < y && y < baseY + note.speed * miss) //剩下的块如果在判定区里
                    {
                        break;
                    }
                    else if (baseY + note.speed * miss < y) //剩下的块如果未到
                    {
                        return;
                    }
                    else //剩下的块也超过了
                    {
                        noteList[barIndex].RemoveAt(0);
                        Destroy(noteObj);
                    }
                }
                if (noteList[barIndex].Count == 0)
                {
                    return;
                }
            }

            if (baseY - note.speed * perfect < y && y < baseY + note.speed * perfect)
            {
                Debug.Log("Perfect!");
            }
            else if (baseY - note.speed * good < y && y < baseY + note.speed * good)
            {
                Debug.Log("Good!");
            }
            else if (baseY - note.speed * soso < y && y < baseY + note.speed * soso)
            {
                Debug.Log("So-so!");
            }
            else if (baseY - note.speed * miss < y && y < baseY + note.speed * miss)
            {
                Debug.Log("Miss!");
            }


            noteList[barIndex].RemoveAt(0); //出队
            Destroy(noteObj);
            
        }
    }
}