using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    //�����˶��ٶ�
    private float speed;

    [Header("�ж���")]
    public GameObject bar;

    [Header("��������ͼƬ")]
    public SpriteRenderer image;

    //�Ƿ��Ѿ������ж���
    bool isOutOfJugdingZone = false;
    //�Ƿ��Ѿ��жϹ�
    bool isJugded = false;

    //����ͼƬ
    SpriteRenderer spriteRenderer;

    //�ƶ�����
    private IMovementStrategy movementStrategy;

    //��Ӧ����������ֵ
    float p;

    //bar��Ӧ����������ֵ
    float barP;

    //��ǰ���ڹ��
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
        this.image.sprite = Resources.Load<Sprite>("Images/MusicGame/����/" + type);
    }


    //�����ƶ�
    void Move()
    {
        transform.Translate(speed * Time.deltaTime * movementStrategy.GetMoveDirV3());
        p = movementStrategy.GetPositionOnAxis(transform);
    }

    //�Ƿ��Ѿ��뿪���ж���
    void IsOutOfJudgingZone()
    {
        if (p < barP - BarJudger.miss * speed)
        {
            Debug.Log("��⣡");
            barJudger.ShowText("��⣡", Color.red);
            isOutOfJugdingZone = true;
            DestroyNote();

            ScoreManager.ScoreManagerInstance?.score.AddNoteCount(); //������������
            ScoreManager.ScoreManagerInstance?.score.UpdateCurrentRate(); //����Ŀǰ��perfect��
            ScoreManager.ScoreManagerInstance?.UpdateCurrentScoreText(); //�����ı�
        }
    }

    public bool JudgeTime()
    { 
        float baseP = barP;

        float p = this.p;

        if (baseP + this.speed * BarJudger.miss < p)  //��δ���ж���
        {
            return false;
        }

        //�ж����ڵ�ʱ���ж�
        if (baseP - speed * BarJudger.perfect < p && p < baseP + speed * BarJudger.perfect)
        {
            barJudger.ShowText("�ޣ�", Color.yellow);
            ScoreManager.ScoreManagerInstance?.score.AddScore("�ޣ�");
        }
        else if (baseP - speed * BarJudger.good < p && p < baseP + speed * BarJudger.good)
        {
            barJudger.ShowText("���У�", Color.green);
            ScoreManager.ScoreManagerInstance?.score.AddScore("���У�");
        }
        else if (baseP - speed * BarJudger.soso < p && p < baseP + speed * BarJudger.soso)
        {
            barJudger.ShowText("һ�㣡", Color.cyan);
            ScoreManager.ScoreManagerInstance?.score.AddScore("һ�㣡");
        }
        else if (baseP - speed * BarJudger.miss < p && p < baseP + speed * BarJudger.miss)
        {
            barJudger.ShowText("�ź���", Color.red);
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

        //����Ƿ񳬳���Ļ�߽磨�����壩
        float buffer = 100f;  //������Ļ��Ե100���ؾ�����

        if (screenPos.y < -buffer || screenPos.x < -buffer)
        {
            Destroy(gameObject);
         
            noteManager.ClearIndex();
        }
    }
}
