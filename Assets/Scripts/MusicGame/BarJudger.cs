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

    public static PopUpText text;


    void Start()
    {
        text = GetComponentInChildren<PopUpText>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (NoteManager.NoteManagerInstance.NoteListCount > 0)
            {
                Note note = NoteManager.NoteManagerInstance.PeekFirstNote();
                note.JudgeTime();
            }
            else
            {
                Debug.Log("轨道上已无音符！");
            }
        }
    }

    public static void ShowText(string message, Color color, float duration = 0f)
    {
        text.ShowText(message, color, duration);
    }
}
