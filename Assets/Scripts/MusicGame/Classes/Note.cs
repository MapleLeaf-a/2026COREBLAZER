using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    //音符运动速度
    public float speed;

    [Header("判定条")]
    public Transform bar;

    //是否已经超出判定区
    bool isOutOfJugdingZone = false;
    //是否已经判断过
    bool isJugded = false;

    //音符图片
    SpriteRenderer spriteRenderer;

    [Header("配置音符移动方向")]
    public NoteMoveDirEnum direction;

    //移动策略
    private IMovementStrategy movementStrategy;

    //对应方向的轴向的值
    float p;

    //bar对应方向的轴向的值
    float barP;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movementStrategy = CreateMoveStategy(direction);
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

    //多态解决音符的不同运动方向
    IMovementStrategy CreateMoveStategy(NoteMoveDirEnum dir)
    {
        switch (dir)
        {
            case NoteMoveDirEnum.RightToLeft:
                return new RightToLeftMovement();
            case NoteMoveDirEnum.TopToBottom:
                return new TopToBottomMovement();
            default:
                throw new System.Exception("音符运动方向设置错误！");
        }
    }


    //音符移动
    void Move()
    {
        transform.Translate(speed * Time.deltaTime * movementStrategy.GetMoveDirV3());
    }

    //是否已经离开了判定区
    void IsOutOfJudgingZone()
    {
        if (p < barP - BarJudger.miss * speed)
        {
            Debug.Log("Totally MISS!");
            BarJudger.ShowText("Totally MISS!", Color.red);
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
            BarJudger.ShowText("Perfect!", Color.yellow);
            ScoreManager.ScoreManagerInstance.score.AddScore("Perfect!");
        }
        else if (baseP - speed * BarJudger.good < p && p < baseP + speed * BarJudger.good)
        {
            BarJudger.ShowText("Good!", Color.green);
            ScoreManager.ScoreManagerInstance.score.AddScore("Good!");
        }
        else if (baseP - speed * BarJudger.soso < p && p < baseP + speed * BarJudger.soso)
        {
            BarJudger.ShowText("So-so!", Color.cyan);
            ScoreManager.ScoreManagerInstance.score.AddScore("So-so!");
        }
        else if (baseP - speed * BarJudger.miss < p && p < baseP + speed * BarJudger.miss)
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
