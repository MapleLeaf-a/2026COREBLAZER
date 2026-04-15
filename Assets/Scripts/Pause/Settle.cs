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
    public Canvas settleCanvas;

    //结算的文本
    public TextMeshProUGUI settleText;

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
        settleText.text = "提前结算：" + score.GetFinalRate();

        score.InitScore();

        foreach (var track in TracksManager.instance.tracks)
        { 
            track.RemoveALLNotes();
        }

        CanvasManager.canvasManagerInstance.canvasStack.ReplaceAll(settleCanvas);
    }
}
