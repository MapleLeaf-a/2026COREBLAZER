using UnityEngine;

/// <summary>
/// 长音符: 头部和尾部各判定一次, 中间需保持按住
/// 状态机: WaitingForHead → Holding → WaitingForTail → Done / Broken
/// </summary>
public class LongNote : MonoBehaviour
{
    public enum State { WaitingForHead, Holding, WaitingForTail, Done, Broken }
    public State state = State.WaitingForHead;

    [Header("视觉部件 (在 prefab 里指定)")]
    public Transform head;        // 头部 (SpriteRenderer)
    public Transform tail;        // 尾部 (SpriteRenderer)
    public SpriteRenderer body;   // 中间连接条 (Sprite Draw Mode = Tiled)

    [Header("音符种类着色")]
    public SpriteRenderer headRenderer;
    public SpriteRenderer tailRenderer;

    private float speed;
    private int lengthInCells;
    private float spawnInterval;

    private IMovementStrategy movementStrategy;
    private GameObject bar;
    private BarJudger barJudger;
    private NoteManager noteManager;
    private Camera mainCamera;

    // 头部和尾部分别相对判定条的轴向位置
    private float barP;
    private bool headJudged = false;
    private bool tailJudged = false;

    public void Initialize(NoteManager nm, GameObject bar, IMovementStrategy strategy,
                           float speed, int lengthInCells, float spawnInterval, Color color)
    {
        this.noteManager = nm;
        this.bar = bar;
        this.barJudger = bar.GetComponent<BarJudger>();
        this.movementStrategy = strategy;
        this.speed = speed;
        this.lengthInCells = lengthInCells;
        this.spawnInterval = spawnInterval;
        this.mainCamera = Camera.main;

        // 调整尾部位置和中间条的长度
        // 长度 = (格数-1) × 每格秒数 × 速度 (世界单位)
        float worldLength = (lengthInCells - 1) * spawnInterval * speed;
        if (tail != null)
        {
            // 尾部沿"音符前进方向的反方向"摆放
            // 比如向左移动, 尾部就在右边
            Vector3 backward = -movementStrategy.GetMoveDirV3();
            tail.localPosition = backward * worldLength;
        }
        if (body != null)
        {
            // body 的 size.x 拉到匹配长度
            body.size = new Vector2(worldLength, body.size.y);
            // body 居中
            body.transform.localPosition = (-movementStrategy.GetMoveDirV3()) * (worldLength / 2f);
        }

        // 按种类着色 (用音符种类的颜色)
        if (headRenderer != null) headRenderer.color = color;
        if (tailRenderer != null) tailRenderer.color = color;
        if (body != null) body.color = new Color(color.r, color.g, color.b, 0.5f);

        barP = movementStrategy.GetPositionOnAxis(bar.transform);
    }

    void Update()
    {
        // 整体移动 (跟 Note 一样)
        transform.Translate(speed * Time.deltaTime * movementStrategy.GetMoveDirV3());

        // 头尾的世界位置
        float headP = head != null ? movementStrategy.GetPositionOnAxis(head) : movementStrategy.GetPositionOnAxis(transform);
        float tailP = tail != null ? movementStrategy.GetPositionOnAxis(tail) : headP;

        // === 状态机 ===
        switch (state)
        {
            case State.WaitingForHead:
                // 头超出判定区还没判 → Totally MISS
                if (headP < barP - BarJudger.miss * speed)
                {
                    barJudger.ShowText("Totally MISS!", Color.red);
                    state = State.Broken;
                    OnLongNoteEnd(missCountIncrement: 1);
                }
                break;

            case State.Holding:
                // 等待尾部进入判定区
                if (tailP < barP + BarJudger.miss * speed)
                {
                    state = State.WaitingForTail;
                }
                break;

            case State.WaitingForTail:
                // 玩家一直按着, 尾到达判定条 → 自动判 Perfect
                // (按住到底视为完美完成, 不强制要求松手)
                if (Mathf.Abs(tailP - barP) < speed * BarJudger.perfect)
                {
                    barJudger.ShowText("Perfect!", Color.yellow);
                    ScoreManager.ScoreManagerInstance?.score.AddScore("Perfect!");
                    tailJudged = true;
                    state = State.Done;
                    OnLongNoteEnd(missCountIncrement: 0);

                    if (tailRenderer != null)
                        tailRenderer.color = new Color(tailRenderer.color.r, tailRenderer.color.g, tailRenderer.color.b, 0.5f);
                }
                break;
        }

        // 等尾巴也出屏幕之后再销毁
        if (mainCamera != null && tail != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(tail.position);
            float buffer = 200f;
            if (screenPos.y < -buffer || screenPos.x < -buffer)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 头部判定: 玩家按下时 BarJudger 调用
    /// </summary>
    public bool JudgeHead()
    {
        if (state != State.WaitingForHead) return false;

        float headP = head != null ? movementStrategy.GetPositionOnAxis(head) : movementStrategy.GetPositionOnAxis(transform);

        // 未到判定区
        if (barP + speed * BarJudger.miss < headP) return false;

        if (Mathf.Abs(headP - barP) < speed * BarJudger.perfect)
            barJudger.ShowText("Perfect!", Color.yellow);
        else if (Mathf.Abs(headP - barP) < speed * BarJudger.good)
            barJudger.ShowText("Good!", Color.green);
        else if (Mathf.Abs(headP - barP) < speed * BarJudger.soso)
            barJudger.ShowText("So-so!", Color.cyan);
        else
            barJudger.ShowText("Miss!", Color.red);

        headJudged = true;
        state = State.Holding;

        // 头判算一个音符
        ScoreManager.ScoreManagerInstance?.score.AddNoteCount();
        ScoreManager.ScoreManagerInstance?.score.UpdateCurrentRate();
        ScoreManager.ScoreManagerInstance?.UpdateCurrentScoreText();

        // 头部视觉反馈: 半透明
        if (headRenderer != null)
            headRenderer.color = new Color(headRenderer.color.r, headRenderer.color.g, headRenderer.color.b, 0.5f);

        return true;
    }

    /// <summary>
    /// 玩家松开按键: 中途松手 → break, 末尾窗内 → 尾判
    /// </summary>
    public void OnRelease()
    {
        if (state == State.Holding)
        {
            // 中途松手 → break
            barJudger.ShowText("BREAK!", Color.red);
            state = State.Broken;
            OnLongNoteEnd(missCountIncrement: 1);  // 尾巴算一个 miss
        }
        else if (state == State.WaitingForTail)
        {
            // 末尾窗内松手 → 尾判
            float tailP = tail != null ? movementStrategy.GetPositionOnAxis(tail) : 0f;

            if (Mathf.Abs(tailP - barP) < speed * BarJudger.perfect)
            {
                barJudger.ShowText("Perfect!", Color.yellow);
                ScoreManager.ScoreManagerInstance?.score.AddScore("Perfect!");
            }
            else if (Mathf.Abs(tailP - barP) < speed * BarJudger.good)
            {
                barJudger.ShowText("Good!", Color.green);
                ScoreManager.ScoreManagerInstance?.score.AddScore("Good!");
            }
            else if (Mathf.Abs(tailP - barP) < speed * BarJudger.soso)
            {
                barJudger.ShowText("So-so!", Color.cyan);
                ScoreManager.ScoreManagerInstance?.score.AddScore("So-so!");
            }
            else
            {
                barJudger.ShowText("Miss!", Color.red);
            }

            tailJudged = true;
            state = State.Done;
            OnLongNoteEnd(missCountIncrement: 0);

            // 尾部视觉反馈
            if (tailRenderer != null)
                tailRenderer.color = new Color(tailRenderer.color.r, tailRenderer.color.g, tailRenderer.color.b, 0.5f);
        }
    }

    private void OnLongNoteEnd(int missCountIncrement)
    {
        // 尾巴算一个音符 (无论 break 还是正常完成)
        ScoreManager.ScoreManagerInstance?.score.AddNoteCount();
        ScoreManager.ScoreManagerInstance?.score.UpdateCurrentRate();
        ScoreManager.ScoreManagerInstance?.UpdateCurrentScoreText();

        // 通知 NoteManager 移除自己
        noteManager.RemoveLongNote(this);
    }

    public bool IsFinished => state == State.Done || state == State.Broken;
}