using UnityEngine;

/// <summary>
/// 长音符: 头部和尾部各判定一次, 中间需保持按住
/// 状态机: WaitingForHead → Holding → WaitingForTail → Done / Broken
///
/// 计分规则 (与设计文档对齐):
///   长音符 = 1 个音符 (NoteCount + 1, 仅在结算时计 1 次)
///   总得分 = headPct × 0.3 + holdPct × 0.4 + tailPct × 0.3   (满分 1.0)
///     - headPct: 头部按下精度 (Perfect=1.0, Good=0.8, So-so=0.6, Miss=0)
///     - holdPct: 保持时长比例 (一直按住到 Hold 结束 = 1.0; 中途松手 = 已保持/总时长)
///     - tailPct: 尾部精度 (Perfect=1.0, Good=0.8, So-so=0.6, Miss=0)
///   BREAK 特殊: 中途松手得分 = headPct × 30% + holdPct × 40% (按实际按住的比例给 hold 段, 拿不到尾部 30%)
///   头部 Totally MISS: 整个长音符得分 = 0 (相当于完全没接住)
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

    // === 计分跟踪 (本长音符一生的过程量) ===
    private float headPct = 0f;            // 头部按下精度 [0, 1]
    private float tailPct = 0f;            // 尾部精度 [0, 1]
    private float holdStartTime = -1f;     // 头部 Judge 时的游戏时间
    private float expectedHoldDuration;    // 头到尾的世界长度对应的时间 (秒)

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
        // 预期保持时长 = 世界长度 / 速度 = (格数-1) × 每格秒数
        expectedHoldDuration = (lengthInCells - 1) * spawnInterval;

        if (tail != null)
        {
            // 尾部沿"音符前进方向的反方向"摆放
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
                // 头超出判定区还没按 → Totally MISS (整个长音符 0 分)
                if (headP < barP - BarJudger.miss * speed)
                {
                    barJudger.ShowText("糟糕！", Color.red);
                    state = State.Broken;
                    headPct = 0f;
                    tailPct = 0f;
                    FinalizeScore(brokenInMid: false);
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
                // (按到底视为完美完成, 不强制要求松手)
                if (Mathf.Abs(tailP - barP) < speed * BarJudger.perfect)
                {
                    barJudger.ShowText("赞！", Color.yellow);
                    tailPct = 1f;
                    tailJudged = true;
                    state = State.Done;
                    FinalizeScore(brokenInMid: false);

                    if (tailRenderer != null)
                        tailRenderer.color = new Color(tailRenderer.color.r, tailRenderer.color.g, tailRenderer.color.b, 0.5f);
                }
                else if (tailP < barP - BarJudger.miss * speed)
                {
                    // 尾部完全离开判定区还没松手 → 当作 Miss tail
                    barJudger.ShowText("遗憾！", Color.red);
                    tailPct = 0f;
                    tailJudged = true;
                    state = State.Done;
                    FinalizeScore(brokenInMid: false);
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

        // 头部精度判定 → 算 headPct
        if (Mathf.Abs(headP - barP) < speed * BarJudger.perfect)
        {
            barJudger.ShowText("赞！", Color.yellow);
            headPct = 1f;
        }
        else if (Mathf.Abs(headP - barP) < speed * BarJudger.good)
        {
            barJudger.ShowText("还行！", Color.green);
            headPct = 0.8f;
        }
        else if (Mathf.Abs(headP - barP) < speed * BarJudger.soso)
        {
            barJudger.ShowText("一般！", Color.cyan);
            headPct = 0.6f;
        }
        else
        {
            barJudger.ShowText("遗憾！", Color.red);
            headPct = 0f;
        }

        headJudged = true;
        state = State.Holding;
        holdStartTime = Time.time;

        // 头部视觉反馈: 半透明
        if (headRenderer != null)
            headRenderer.color = new Color(headRenderer.color.r, headRenderer.color.g, headRenderer.color.b, 0.5f);

        return true;
    }

    /// <summary>
    /// 玩家松开按键: 中途松手 → break, 末尾窗口内 → 尾判
    /// </summary>
    public void OnRelease()
    {
        if (state == State.Holding)
        {
            // 中途松手 → BREAK
            barJudger.ShowText("BREAK!", Color.red);
            state = State.Broken;
            tailPct = 0f;
            FinalizeScore(brokenInMid: true);
        }
        else if (state == State.WaitingForTail)
        {
            // 末尾窗口内松手 → 按尾部精度判定
            float tailP = tail != null ? movementStrategy.GetPositionOnAxis(tail) : 0f;

            if (Mathf.Abs(tailP - barP) < speed * BarJudger.perfect)
            {
                barJudger.ShowText("赞！", Color.yellow);
                tailPct = 1f;
            }
            else if (Mathf.Abs(tailP - barP) < speed * BarJudger.good)
            {
                barJudger.ShowText("还行！", Color.green);
                tailPct = 0.8f;
            }
            else if (Mathf.Abs(tailP - barP) < speed * BarJudger.soso)
            {
                barJudger.ShowText("一般！", Color.cyan);
                tailPct = 0.6f;
            }
            else
            {
                barJudger.ShowText("遗憾！", Color.red);
                tailPct = 0f;
            }

            tailJudged = true;
            state = State.Done;
            FinalizeScore(brokenInMid: false);

            // 尾部视觉反馈
            if (tailRenderer != null)
                tailRenderer.color = new Color(tailRenderer.color.r, tailRenderer.color.g, tailRenderer.color.b, 0.5f);
        }
    }

    /// <summary>
    /// 在长音符整个生命周期结束时调用一次, 计入 NoteCount + Score.
    /// brokenInMid: true 表示中途松手 BREAK (按 hold 比例给 hold 段分数, 拿不到尾部);
    ///              false 表示正常 / 尾部 Miss / 头部 Totally MISS (按 3:4:3 加权)
    /// </summary>
    private void FinalizeScore(bool brokenInMid)
    {
        float totalPct;
        float holdPct;

        if (brokenInMid)
        {
            // BREAK: 拿到 head 那 30% + 按 hold 比例的 0~40%, 但失去尾部那 30%
            // holdPct = 已按住时间 / 预期保持时长
            if (expectedHoldDuration > 0f && holdStartTime >= 0f)
            {
                holdPct = Mathf.Clamp01((Time.time - holdStartTime) / expectedHoldDuration);
            }
            else
            {
                holdPct = 0f;
            }
            totalPct = headPct * 0.3f + holdPct * 0.4f;
            // 尾部那 0.3f 永远拿不到 (BREAK 没接住尾)
        }
        else
        {
            // 正常完成 / 尾部 Miss / 头部 Totally MISS
            if (state == State.Broken)
            {
                // 头部 Totally MISS 进的 Broken: 没接住任何东西
                holdPct = 0f;
            }
            else
            {
                // 正常完成或尾部 Miss: 头部按下后一直按到 Done, hold 完整
                holdPct = 1f;
            }
            totalPct = headPct * 0.3f + holdPct * 0.4f + tailPct * 0.3f;
        }

        // 累加到 ScoreManager: 长音符算 1 个音符
        ScoreManager.ScoreManagerInstance?.score.AddScore(totalPct);
        ScoreManager.ScoreManagerInstance?.score.AddNoteCount();
        ScoreManager.ScoreManagerInstance?.score.UpdateCurrentRate();
        ScoreManager.ScoreManagerInstance?.UpdateCurrentScoreText();

        Debug.Log($"[LongNote] 结算: headPct={headPct:F2} holdPct={holdPct:F2} tailPct={tailPct:F2} totalPct={totalPct:F2} (broken={brokenInMid})");

        // 通知 NoteManager 移除自己
        noteManager.RemoveLongNote(this);
    }

    public bool IsFinished => state == State.Done || state == State.Broken;
}
