using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//管理一轨道音符的脚本(生成、销毁)
public class NoteManager : MonoBehaviour
{
    //预设的音符列表
    private List<int> notes;
    //遍历预设音符列表的i
    int iForNotes;

    //待打的音符队列
    private Queue<Note> noteList = new Queue<Note>();

    private float spawnInterval = 0.0417f; //每多少秒生成一个节奏块
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

    [Header("音符种类→预制体映射 (按 NotesStatics.noteTypes 顺序)")]
    public List<GameObject> notePrefabsByType;  // 排谱工具按种类自动选: 第 N 项对应 noteTypes[N]

    //判定条
    public GameObject bar;
    private BarJudger barJudger;

    private float timer; //生成计时器

    //是否结束
    private bool over = false;

    //画布父物体
    public Canvas canvas;

    //提交菜品的canvas
    public Canvas SubmitCanvas;

    //轨道索引
    int trackIndex;
    //轨道总数量
    int trackCount;

    //轨道管理者
    TracksManager tracksManager;

    [Header("长音符预制体")]
    public GameObject longNotePrefab;

    // 长音符数据 (从 NotesStatics.stepsLongs 来)
    private List<LongNoteData> longs;
    // 已生成但还没完成的长音符
    private List<LongNote> activeLongNotes = new List<LongNote>();

    [System.Serializable]
    public class LongNoteData
    {
        public int start;
        public int length;
        public int typeIndex;
    }
    void Start()
    {
        movementStrategy = CreateMoveStategy(direction);
        // NOTE: BarJudger wiring moved to Initialize() — running here was a bug because
        // trackCount/trackIndex aren't set until TracksManager calls Initialize(),
        // and Unity makes no guarantee about Start() order between sibling MonoBehaviours.
        // For 4-track that bug was harmless (trackCount=0 still fell into the multi-track
        // branch and queried JudgeTrack0..3 which happen to map to A/D/J/L). For 1-track
        // it was fatal: trackCount=0 never queried the "Judge" / Space binding.
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (iForNotes < notes.Count && timer > spawnInterval)
        {
            // 先看这个格子是不是某个长音符的起点
            LongNoteData ln = FindLongNoteStartAt(iForNotes);
            if (ln != null)
            {
                SpawnLongNote(ln);
                // 长音符起点的位置不再生成普通音符 (即使 notes[iForNotes]>0)
            }
            else if (notes[iForNotes] > 0)
            {
                SpawnNote(notes[iForNotes]);
            }
            iForNotes++;
            timer = 0;
        }
        if (iForNotes >= notes.Count && !tracksManager.TrackOutOfPre() && !over
            && NoteListCount == 0 && activeLongNotes.Count == 0)
        {
            over = true;
            ScoreManager.ScoreManagerInstance?.score.ComputeFinalRate();
            canvas.gameObject.SetActive(false);
            SubmitCanvas.gameObject.SetActive(true);
            TestBackpack.instance.foodView.UpdateFoods(TestBackpack.instance.todaysRecipeView.todaysRecipeViewModel.CurrentPageItems);
        }
    }

    private LongNoteData FindLongNoteStartAt(int cellIdx)
    {
        foreach (var l in longs)
        {
            if (l.start == cellIdx) return l;
        }
        return null;
    }

    public void Initialize(int trackIndex, int trackCount, List<int> notesPre,
                           List<LongNoteData> longsPre, TracksManager tracksManager, float spawnInterval)
    {
        this.trackIndex = trackIndex;
        this.trackCount = trackCount;
        this.notes = notesPre;
        this.longs = longsPre ?? new List<LongNoteData>();
        this.tracksManager = tracksManager;
        this.spawnInterval = spawnInterval;

        // Wire up the BarJudger now that we have correct trackIndex / trackCount.
        // (bar is a public field assigned in Inspector — safe to access here.)
        if (bar != null)
        {
            barJudger = bar.GetComponent<BarJudger>();
            if (barJudger != null)
            {
                barJudger.Initialize(this, trackIndex, trackCount);
            }
            else
            {
                Debug.LogWarning("[NoteManager] bar 上没找到 BarJudger 组件 (轨道 " + trackIndex + ")");
            }
        }
        else
        {
            Debug.LogWarning("[NoteManager] bar 字段未在 Inspector 里赋值 (轨道 " + trackIndex + ")");
        }
    }

    /// <summary>
    /// 获取遍历预设音符列表的i
    /// </summary>
    public int IForNotes => iForNotes;

    /// <summary>
    /// 预设音符的列表长度
    /// </summary>
    public int PreNotesCount => notes.Count;


    /// <summary>
    /// 增加点击的轨道索引
    /// </summary>
    /// <param name="index"></param>
    public void AddBarIndex(int index)
    {
        tracksManager.AddBarIndex(index);
    }

    /// <summary>
    /// 清空已点击轨道索引列表
    /// </summary>
    public void ClearIndex()
    {
        tracksManager.ClearIndex();
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

    private void SpawnNote(int typeIdx)
    {
        Vector3 spawnPos = bar.transform.position - movementStrategy.GetMoveDirV3() * spawnHorizontalOffset;
        GameObject _resolvedPrefab = (notePrefabsByType != null && typeIdx - 1 >= 0 && typeIdx - 1 < notePrefabsByType.Count && notePrefabsByType[typeIdx - 1] != null) ? notePrefabsByType[typeIdx - 1] : notePrefab;
        GameObject noteObj =  Instantiate(_resolvedPrefab, spawnPos, _resolvedPrefab.transform.rotation, canvas.transform);

        Note note = noteObj.GetComponent<Note>();
        note.Initialize(this, bar, movementStrategy, noteSpeed);

        AddNote(note);
    }

    private void SpawnLongNote(LongNoteData data)
    {
        if (longNotePrefab == null)
        {
            Debug.LogWarning("longNotePrefab 未设置, 长音符跳过 (在 NoteManager 上拖一个 LongNote prefab)");
            return;
        }

        Vector3 spawnPos = bar.transform.position - movementStrategy.GetMoveDirV3() * spawnHorizontalOffset;
        GameObject obj = Instantiate(longNotePrefab, spawnPos, longNotePrefab.transform.rotation, canvas.transform);

        LongNote ln = obj.GetComponent<LongNote>();

        // 颜色按种类取
        Color c = Color.white;
        // 这里你可以根据 typeIndex 查 NotesStatics.noteTypes[typeIdx-1].color
        // 简化版: 先用白色, 之后再加查色逻辑
        // (typeIndex 1-based, 0=无)
        int idx = data.typeIndex - 1;
        if (idx >= 0 && idx < NotesStatics.noteTypes.Count)
        {
            ColorUtility.TryParseHtmlString(NotesStatics.noteTypes[idx].color, out c);
        }

        ln.Initialize(this, bar, movementStrategy, noteSpeed, data.length, spawnInterval, c);
        activeLongNotes.Add(ln);
    }

    public void RemoveLongNote(LongNote ln)
    {
        activeLongNotes.Remove(ln);
        // 不立即销毁 GameObject, 让 LongNote 自己出屏幕后销毁
    }

    public List<LongNote> ActiveLongNotes => activeLongNotes;

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
