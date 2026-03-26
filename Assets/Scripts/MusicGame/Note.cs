using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    //音符运动速度
    public float speed;

    //判定条
    public static Transform bar;

    //是否已经超出判定区
    bool isOutOfJugdingZone = false;
    //是否已经判断过
    bool isJugded = false;

    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();    
    }

    void Update()
    {
        Move();

        if (!isOutOfJugdingZone && !isJugded)
        {
            IsOutOfJudgingZone();
        }

        IsOutOfScreen();
    }

    //音符移动
    void Move()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.left);
    }

    //是否已经离开了判定区
    void IsOutOfJudgingZone()
    {
        if (transform.position.x < bar.position.x - BarJudger.miss * speed)
        {
            Debug.Log("Totally MISS!");
            BarJudger.ShowText("Totally MISS!", Color.red);
            isOutOfJugdingZone = true;
            DestroyNote();
        }
    }

    public bool JudgeTime()
    { 
        float baseX = bar.position.x;

        float x = transform.position.x;

        if (baseX + this.speed * BarJudger.miss < x)  //还未到判定区
        {
            return false;
        }

        //判定区内的时机判定
        if (baseX - speed * BarJudger.perfect < x && x < baseX + speed * BarJudger.perfect)
        {
            BarJudger.ShowText("Perfect!", Color.yellow);
            ScoreManager.ScoreManagerInstance.score.AddScore("Perfect!");
        }
        else if (baseX - speed * BarJudger.good < x && x < baseX + speed * BarJudger.good)
        {
            BarJudger.ShowText("Good!", Color.green);
            ScoreManager.ScoreManagerInstance.score.AddScore("Good!");
        }
        else if (baseX - speed * BarJudger.soso < x && x < baseX + speed * BarJudger.soso)
        {
            BarJudger.ShowText("So-so!", Color.cyan);
            ScoreManager.ScoreManagerInstance.score.AddScore("So-so!");
        }
        else if (baseX - speed * BarJudger.miss < x && x < baseX + speed * BarJudger.miss)
        {
            BarJudger.ShowText("Miss!", Color.red);
        }
        DestroyNote();
        isJugded = true;
        return true;
    }

    public void DestroyNote()
    {
        NoteManager.NoteManagerInstance.RemoveNote();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f);
    }

    public void IsOutOfScreen()
    {
        if (transform.position.x < bar.position.x - 10f)
        {
            Destroy(gameObject);
        }
    }
}
