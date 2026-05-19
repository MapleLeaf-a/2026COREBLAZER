using System.Diagnostics;
using System.Runtime.InteropServices;

public class Score
{
    //分数
    private float score;
    
    //音符数量统计
    private int noteCount;

    //最终perfect率
    private float finalRate;

    //当前perfect率
    private float currentRate;


    public void AddScore(float score)
    {
        this.score += score;
    }

    public void AddScore(string state)
    {
        switch (state)
        {
            case "赞！":
                score += 1f;
                break;
            case "还行！":
                score += 0.8f;
                break;
            case "一般！":
                score += 0.6f;
                break;
            //case "遗憾！":
            //    break;
            //case "糟糕！":
            //    break;
            default:
                break;
        }
    }

    public float GetScore()
    { 
        return score;
    }

    public void AddNoteCount()
    {
        noteCount++;
    }

    public void UpdateCurrentRate()
    { 
        if (noteCount == 0) return;
        currentRate = score / noteCount;
    }

    public float GetCurrentRate()
    { 
        return currentRate;
    }

    public void ComputeFinalRate()
    {
        if (noteCount == 0)
        {
            return;
        }
        finalRate = score / noteCount;
    }

    public float GetFinalRate()
    { 
        return finalRate;
    }

    public void InitScore()
    { 
        score = 0f;
        noteCount = 0;
        finalRate = 0f;
        currentRate = 0f;
    }

    public int GetNoteCount()
    { 
        return noteCount;
    }
}
