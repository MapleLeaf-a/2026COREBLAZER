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
        currentScoreText.text = Mathf.Round(ScoreManager.ScoreManagerInstance.score.GetCurrentRate() * 100).ToString() + "%";
        Debug.Log("NoteCount = " + score.GetNoteCount());
    }
}
