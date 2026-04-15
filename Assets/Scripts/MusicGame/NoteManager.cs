using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//管理一轨道音符的脚本(生成、销毁)
public class NoteManager : MonoBehaviour
{
    //预设的音符列表
    private List<bool> notes;
    int i;

    //待打的音符队列
    private Queue<Note> noteList = new Queue<Note>();

    private float spawnInterval = 0.8f; //每多少秒生成一个节奏块
    [Header("配置生成时音符离Bar的距离")]
    public float spawnHorizontalOffset = 8f; //生成时离Bar的距离

    [Header("配置音符移动方向")]
    public NoteMoveDirEnum direction;

    //移动策略
    private IMovementStrategy movementStrategy;


    //音符速度
    float noteSpeed = 5f;

    //音符预制体
    public GameObject notePrefab;

    //判定条
    public GameObject bar;
    private BarJudger barJudger;

    private float timer; //生成计时器

    //是否结束
    private bool over = false;

    //画布父物体
    public Canvas canvas;

    //轨道索引
    int trackIndex;
    //轨道总数量
    int trackCount;

    void Start()
    {
        movementStrategy = CreateMoveStategy(direction);
        barJudger = bar.GetComponent<BarJudger>();
        barJudger.Initialize(this, trackIndex, trackCount);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (i < notes.Count && timer > spawnInterval)
        {
            if (notes[i])
            {
                SpawnNote();
            }
            i++;
            timer = 0;
        }

        if (i >= notes.Count && !over && NoteListCount == 0)
        { 
            over = true;
            ScoreManager.ScoreManagerInstance?.score.ComputeFinalRate();
            Debug.Log("Perfect率:" + ScoreManager.ScoreManagerInstance?.score.GetFinalRate());
        }
    }

    public void Initialize(int trackIndex, int trackCount, List<bool> notesPre)
    { 
        this.trackIndex = trackIndex;
        this.trackCount = trackCount;
        this.notes = notesPre;
    }

    //多态解决音符的不同运动方向
    IMovementStrategy CreateMoveStategy(NoteMoveDirEnum dir)
    {
        switch (dir)
        {
            case NoteMoveDirEnum.RightToLeft:
                return new RightToLeftMovement();
            case NoteMoveDirEnum.TopToBottom:
                return new TopToBottomMovement();
            default:
                throw new System.Exception("音符运动方向设置错误！");
        }
    }

    private void SpawnNote()
    {
        Vector3 spawnPos = bar.transform.position - movementStrategy.GetMoveDirV3() * spawnHorizontalOffset;
        GameObject noteObj =  Instantiate(notePrefab, spawnPos, notePrefab.transform.rotation, canvas.transform);

        Note note = noteObj.GetComponent<Note>();
        note.Initialize(this, bar, movementStrategy, noteSpeed);

        AddNote(note);

        //ScoreManager.ScoreManagerInstance.score.AddNoteCount();
    }

    //添加一个音符
    public void AddNote(Note note)
    { 
        noteList.Enqueue(note);
    }
    //删除一个音符(包括物体)
    public void RemoveNote()
    {
        if (noteList.Count > 0)
        {
            Note note = noteList.Dequeue();
        }
        else 
        {
            Debug.Log("RemoveNote : noteList is EMPTY!");
        }
    }
    //获取队首音符
    public Note PeekFirstNote()
    {
        if (noteList.Count > 0)
        {
            return noteList.Peek();
        }
        else 
        {
            Debug.Log("GetFirstNote : noteList is EMPTY!");
            return null;
        }
    }

    //获取音符列表含有的音符数量
    public int NoteListCount => noteList.Count;

    //删除所有音符(包括其GameObject)
    public void RemoveALLNotes()
    {
        while (noteList.Count != 0)
        {
            Note note = noteList.Dequeue();
            Destroy(note.gameObject);
        }

        Debug.Log("RemoveALLNotes!");
    }
}
