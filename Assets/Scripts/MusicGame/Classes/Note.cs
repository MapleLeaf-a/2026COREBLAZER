using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    //音符运动速度
    private float speed;

    [Header("判定条")]
    public GameObject bar;

    //是否已经超出判定区
    bool isOutOfJugdingZone = false;
    //是否已经判断过
    bool isJugded = false;

    //音符图片
    SpriteRenderer spriteRenderer;

    //移动策略
    private IMovementStrategy movementStrategy;

    //对应方向的轴向的值
    float p;

    //bar对应方向的轴向的值
    float barP;

    //当前所在轨道
    BarJudger barJudger;

    NoteManager noteManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        barP = movementStrategy.GetPositionOnAxis(bar.transform);
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

    public void Initialize(NoteManager noteManager, GameObject bar, IMovementStrategy dir, float speed)
    {
        this.noteManager = noteManager;
        this.bar = bar;
        this.barJudger = bar.GetComponent<BarJudger>();
        movementStrategy = dir;
        this.speed = speed;
    }


    //音符移动
    void Move()
    {
        transform.Translate(speed * Time.deltaTime * movementStrategy.GetMoveDirV3());
        p = movementStrategy.GetPositionOnAxis(transform);
    }

    //是否已经离开了判定区
    void IsOutOfJudgingZone()
    {
        if (p < barP - BarJudger.miss * speed)
        {
            Debug.Log("Totally MISS!");
            barJudger.ShowText("Totally MISS!", Color.red);
            isOutOfJugdingZone = true;
            DestroyNote();
        }
    }

    public bool JudgeTime()
    { 
        float baseP = barP;

        float p = this.p;

        if (baseP + this.speed * BarJudger.miss < p)  //还未到判定区
        {
            return false;
        }

        //判定区内的时机判定
        if (baseP - speed * BarJudger.perfect < p && p < baseP + speed * BarJudger.perfect)
        {
            barJudger.ShowText("Perfect!", Color.yellow);
            ScoreManager.ScoreManagerInstance?.score.AddScore("Perfect!");
        }
        else if (baseP - speed * BarJudger.good < p && p < baseP + speed * BarJudger.good)
        {
            barJudger.ShowText("Good!", Color.green);
            ScoreManager.ScoreManagerInstance?.score.AddScore("Good!");
        }
        else if (baseP - speed * BarJudger.soso < p && p < baseP + speed * BarJudger.soso)
        {
            barJudger.ShowText("So-so!", Color.cyan);
            ScoreManager.ScoreManagerInstance?.score.AddScore("So-so!");
        }
        else if (baseP - speed * BarJudger.miss < p && p < baseP + speed * BarJudger.miss)
        {
            barJudger.ShowText("Miss!", Color.red);
        }
        
        DestroyNote();
        isJugded = true;
        return true;
    }

    public void DestroyNote()
    {
        noteManager.RemoveNote();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f);
    }

    public void IsOutOfScreen()
    {
        if (p < barP - 10f)
        {
            Destroy(gameObject);
        }
    }
}
