using System.Diagnostics;

public class Score
{
    //分数
    private float score;
    
    //音符数量统计
    private int noteCount;

    //最终perfect率
    private float finalRate;


    public void AddScore(float score)
    {
        this.score += score;
    }

    public void AddScore(string state)
    {
        switch (state)
        {
            case "Perfect!":
                score += 1f;
                break;
            case "Good!":
                score += 0.8f;
                break;
            case "So-so!":
                score += 0.6f;
                break;
            //case "Miss!":
            //    break;
            //case "Totally MISS!":
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

    public void ComputeFinalRate()
    {
        if (noteCount == 0)
        {
            return;
        }
        if (finalRate != 0)
        {
            throw new System.Exception("重复计算，已经有最终率的值了！");
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
    }
}
