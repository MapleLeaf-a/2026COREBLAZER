using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public Score score;

    //目前perfect率的文本
    public TextMeshProUGUI currentScoreText;
    
    //单例
    public static ScoreManager ScoreManagerInstance;

    void Awake()
    {
        if (ScoreManagerInstance == null)
        {
            ScoreManagerInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        score = new Score();
    }

    void Start()
    {
        currentScoreText.text = "0%";
    }

    public void UpdateCurrentScoreText()
    {
        currentScoreText.text = (score.GetCurrentRate() * 100).ToString("F2") + "%";
        Debug.Log("NoteCount = " + score.GetNoteCount());
    }
}
