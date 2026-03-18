using UnityEngine;
using System.Collections;

public class Note : MonoBehaviour
{
    public float speed;
    
    //所在轨道的index
    public int barIndex;

    //所在菜谱的index
    public int mealIndex;

    private SpriteRenderer spriteRenderer;

    float randomOffset;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        randomOffset = Random.Range(0f, Mathf.PI * 2f);

        StartCoroutine(CheckOverlapAndFlicker());
    }

    void Update()
    {
        transform.Translate(Vector3.down * (speed * Time.deltaTime), Space.World);

        IsOutOfScreen();
    }

    IEnumerator CheckOverlapAndFlicker()
    {
        // 等待一帧，确保所有音符都已生成
        yield return null;

        // 检查同一轨道是否有其他音符在附近（重叠）
        bool isOverlapped = false;
        foreach (var note in BarJudger.BarJudgerInstance.noteList[barIndex])
        {
            if (note != this && Mathf.Abs(note.transform.position.y - transform.position.y) < 1f)
            {
                isOverlapped = true;
                break;
            }
        }

        // 如果重叠，开始闪烁
        if (isOverlapped)
        {
            StartCoroutine(FlickerRoutine());
        }
    }

    IEnumerator FlickerRoutine()
    {
        float flickerSpeed = 0.4f; // 闪烁速度
        float minAlpha = 0.3f;
        float maxAlpha = 1f;

        while (true)
        {
            // 使用时间 + 随机偏移量，产生连续的正弦波
            float t = Time.time * (1f / flickerSpeed) + randomOffset;
            // 将sin值从[-1,1]映射到[minAlpha, maxAlpha]
            float alpha = (Mathf.Sin(t) + 1f) * 0.5f * (maxAlpha - minAlpha) + minAlpha;

            SetAlpha(alpha);
            yield return null;
        }
    }

    void SetAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
    }



    void IsOutOfScreen()
    {
        if (transform.position.y < BarJudger.BarJudgerInstance.barList[0].position.y - BarJudger.BarJudgerInstance.miss * speed)
        {
            Debug.Log("Totally MISS!");
            BarJudger.BarJudgerInstance.ShowText(barIndex, "Totally MISS!", Color.red);
            DestroyNote();
        }
    }

    public bool JudgeTime()
    {
        Transform bar = BarJudger.BarJudgerInstance.barList[barIndex];
        float baseY = bar.position.y;

        float y = this.transform.position.y;

        if (baseY + this.speed * BarJudger.BarJudgerInstance.miss < y)  //还未到判定区
        {
            return false;
        }

        //Debug.Log($"判定时: mealIndex={mealIndex}, mealScores.Count={BarJudger.BarJudgerInstance.mealScores.Count}");
        //if (mealIndex >= BarJudger.BarJudgerInstance.mealScores.Count)
        //{
        //    Debug.LogError($"mealIndex {mealIndex} 超出范围，最大值应为 {BarJudger.BarJudgerInstance.mealScores.Count - 1}");
        //}

        //判定区内的时机判定
        if (baseY - speed * BarJudger.BarJudgerInstance.perfect < y && y < baseY + speed * BarJudger.BarJudgerInstance.perfect)
        {
            BarJudger.BarJudgerInstance.ShowText(barIndex, "Perfect!", Color.yellow);
            BarJudger.BarJudgerInstance.mealScores[mealIndex].AddScore("Perfect!");
        }
        else if (baseY - speed * BarJudger.BarJudgerInstance.good < y && y < baseY + speed * BarJudger.BarJudgerInstance.good)
        {
            BarJudger.BarJudgerInstance.ShowText(barIndex, "Good!", Color.green);
            BarJudger.BarJudgerInstance.mealScores[mealIndex].AddScore("Good!");
        }
        else if (baseY - speed * BarJudger.BarJudgerInstance.soso < y && y < baseY + speed * BarJudger.BarJudgerInstance.soso)
        {
            BarJudger.BarJudgerInstance.ShowText(barIndex, "So-so!", Color.gray);
            BarJudger.BarJudgerInstance.mealScores[mealIndex].AddScore("So-so!");
        }
        else if (baseY - speed * BarJudger.BarJudgerInstance.miss < y && y < baseY + speed * BarJudger.BarJudgerInstance.miss)
        {
            BarJudger.BarJudgerInstance.ShowText(barIndex, "Miss!", Color.red);
        }
        DestroyNote();
        return true;
    }

    private void DestroyNote()
    {
        Destroy(gameObject);
        //BarJudger.BarJudgerInstance.noteList[barIndex].RemoveAt(0); //出队
        BarJudger.BarJudgerInstance.noteList[barIndex].Remove(this);
    }
}