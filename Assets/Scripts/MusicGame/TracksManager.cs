using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TracksManager : MonoBehaviour
{
    //轨道
    public List<NoteManager> tracks;

    //音游画布
    public Canvas canvas;

    //预设的各音符轨道的音符情况
    //四轨道
    public List<List<bool>> notesPres = new List<List<bool>>()
    {new List<bool>(),  
     new List<bool>(),  
     new List<bool>(), 
     new List<bool>()
    };
    //单轨道
    private List<bool> notesPre;// = new List<bool> { true, false, false, true, false, true, true, true, true };

    //判定的次数
    int count;

    //预设的四轨道最大判定次数
    private int maxCountOf4Tracks = 3;

    //轨道生成器
    TrackGenerator trackGenerator = new TrackGenerator();

    //存每次点击音符的轨道
    List<int> barIndexs = new List<int>();

    [Header("设置剩余文本")]
    public SetRemainingCountsText text;

    //单例
    public static TracksManager instance;

    void OnEnable()
    {
        CanvasManager.instance.canvasStack.Push(canvas);

        InputManager.instance.SetContext(InputContext.MUSICGAME);
    }

    void OnDisable()
    {
        CanvasManager.instance.canvasStack.PopTo(canvas);
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }


        //需要先于NoteManager调用，防止时序出现问题
        if (tracks.Count == 1)
        {
            tracks[0].Initialize(0, tracks.Count, notesPre, this);
        }
        else
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                tracks[i].Initialize(i, tracks.Count, notesPres[i], this);
            }
        }

        //for (int i = 0; i < 5; i++)
        //    trackGenerator.AddOneLine(notesPres);

        text?.SetText(RemainingCounts);
    }

    /// <summary>
    /// 初始化单轨道的音符序列
    /// </summary>
    /// <param name="noteSequence"></param>
    public void InitializeSingleTrack(List<bool> noteSequence)
    {
        notesPre = noteSequence;

        if (tracks.Count == 1)
        {
            tracks[0].Initialize(0, 1, notesPre, this);
        }
        else
        {
            throw new UnityException("本该为1的轨道数量不为1！");
        }
    }

    /// <summary>
    /// 增加点击的轨道索引
    /// </summary>
    /// <param name="index"></param>
    public void AddBarIndex(int index)
    {
        barIndexs.Add(index);
        if (barIndexs.Count == 1)
        {
            count++;

            text?.SetText(RemainingCounts);
        }
    }

    /// <summary>
    /// 清空已点击轨道索引列表
    /// </summary>
    public void ClearIndex()
    {
        barIndexs.Clear();
    }

    /// <summary>
    /// 剩余的可判定次数
    /// </summary>
    public int RemainingCounts => maxCountOf4Tracks - count;

    /// <summary>
    /// 轨道遍历预设音符超出预设长度,增加一列,返回是否添加成功
    /// </summary>
    public bool TrackOutOfPre()
    {
        if (tracks.Count == 4 && count < maxCountOf4Tracks)
        {
            trackGenerator.AddOneLine(notesPres);
            return true;
        }
        else if (tracks.Count == 4)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                tracks[i].RemoveALLNotes();
            }
            Debug.Log("超出最大判定数！");
            return false;
        }

        return false;
    }
}
