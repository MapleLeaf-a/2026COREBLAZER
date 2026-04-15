using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarJudger : MonoBehaviour
{
    //判定时间
    public const float perfect = 0.04f; //<40ms
    public const float good = 0.08f; //<80ms
    public const float soso = 0.12f; //<120ms
    public const float miss = 0.18f; //<180ms

    public PopUpText text;

    //当前轨道的索引
    int trackIndex;

    //轨道总数
    int trackCount;

    //当前轨道的音符管理
    NoteManager noteManager;

    void Start()
    {
        text = GetComponentInChildren<PopUpText>();
    }

    void Update()
    {
        if (InputManager.InputManagerInstance.GetJudgeKeyDown_MusicGame(trackCount, trackIndex))
        {
            if (noteManager.NoteListCount > 0)
            {
                Note note = noteManager.PeekFirstNote();
                if (note.JudgeTime()) //假如音符判定了
                {
                    ScoreManager.ScoreManagerInstance?.score.AddNoteCount(); //增加音符计数
                    ScoreManager.ScoreManagerInstance?.score.UpdateCurrentRate(); //更新目前的perfect率
                    ScoreManager.ScoreManagerInstance?.UpdateCurrentScoreText(); //更新文本
                }
            }
            else
            {
                Debug.Log("轨道上已无音符！");
            }
        }
    }

    public void Initialize(NoteManager noteManager, int trackIndex, int trackCount)
    {
        this.noteManager = noteManager;
        this.trackIndex = trackIndex;
        this.trackCount = trackCount;
    }

    public void ShowText(string message, Color color, float duration = 0f)
    {
        text.ShowText(message, color, duration);
    }
}
