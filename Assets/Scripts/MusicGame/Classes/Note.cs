using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    //音符运动速度
    private float speed;

    [Header("判定条")]
    public GameObject bar;

    [Header("音符种类图片")]
    public SpriteRenderer image;

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

    public Camera mainCamera;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        barP = movementStrategy.GetPositionOnAxis(bar.transform);
        mainCamera = Camera.main;
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

    public void Initialize(NoteManager noteManager, GameObject bar, IMovementStrategy dir, float speed, string type)
    {
        this.noteManager = noteManager;
        this.bar = bar;
        this.barJudger = bar.GetComponent<BarJudger>();
        movementStrategy = dir;
        this.speed = speed;
        this.image.sprite = Resources.Load<Sprite>("Images/MusicGame/音符/" + type);
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
            Debug.Log("糟糕！");
            barJudger.ShowText("糟糕！", Color.red);
            isOutOfJugdingZone = true;
            DestroyNote();

            ScoreManager.ScoreManagerInstance?.score.AddNoteCount(); //增加音符计数
            ScoreManager.ScoreManagerInstance?.score.UpdateCurrentRate(); //更新目前的perfect率
            ScoreManager.ScoreManagerInstance?.UpdateCurrentScoreText(); //更新文本
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
            barJudger.ShowText("赞！", Color.yellow);
            ScoreManager.ScoreManagerInstance?.score.AddScore("赞！");
        }
        else if (baseP - speed * BarJudger.good < p && p < baseP + speed * BarJudger.good)
        {
            barJudger.ShowText("还行！", Color.green);
            ScoreManager.ScoreManagerInstance?.score.AddScore("还行！");
        }
        else if (baseP - speed * BarJudger.soso < p && p < baseP + speed * BarJudger.soso)
        {
            barJudger.ShowText("一般！", Color.cyan);
            ScoreManager.ScoreManagerInstance?.score.AddScore("一般！");
        }
        else if (baseP - speed * BarJudger.miss < p && p < baseP + speed * BarJudger.miss)
        {
            barJudger.ShowText("遗憾！", Color.red);
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
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);

        //检测是否超出屏幕边界（带缓冲）
        float buffer = 100f;  //超出屏幕边缘100像素就销毁

        if (screenPos.y < -buffer || screenPos.x < -buffer)
        {
            Destroy(gameObject);

            noteManager.ClearIndex();
        }
    }
}