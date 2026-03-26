using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;


//直接结算脚本
public class Settle : MonoBehaviour
{
    //结算按钮
    public Button settleButton;

    //结算的画布
    public Canvas canvas;

    public TextMeshProUGUI text;

    void Start()
    {
        settleButton.onClick.AddListener(SettleGame);
    }

    //结算音游
    public void SettleGame()
    {
        Score score = ScoreManager.ScoreManagerInstance.score;
        score.ComputeFinalRate();
        Debug.Log("提前结算：" + score.GetFinalRate());
        text.text = "提前结算：" + score.GetFinalRate();

        score.InitScore();

        NoteManager.NoteManagerInstance.RemoveALLNotes();

        CanvasManager.canvasManagerInstance.canvasStack.ReplaceAll(canvas);
    }
}
