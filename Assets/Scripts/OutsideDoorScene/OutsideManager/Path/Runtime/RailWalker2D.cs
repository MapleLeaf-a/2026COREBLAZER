using UnityEngine;

/// <summary>
/// 2D 路径移动器。
/// 它根据 RailMap2DAsset 的烘焙路径点移动角色。
/// </summary>
[DisallowMultipleComponent]
public sealed class RailWalker2D : MonoBehaviour
{
    [Header("Rail Data")]

    /// <summary>
    /// 运行时路径地图资产。
    /// 它保存节点、路径段、出口和烘焙点。
    /// </summary>
    [SerializeField]
    private RailMap2DAsset railMap;

    /// <summary>
    /// 当前所在路径段 ID。
    /// 这个 ID 必须存在于 railMap.segments。
    /// </summary>
    [SerializeField]
    private int currentSegmentId;

    /// <summary>
    /// 初始归一化位置。
    /// 0 表示当前路径段起点。
    /// 1 表示当前路径段终点。
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float normalizedStartPosition = 0f;

    [Header("Movement")]

    /// <summary>
    /// 沿路径移动速度。
    /// 单位是 Unity 世界单位每秒。
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float moveSpeed = 4f;

    /// <summary>
    /// 横向输入死区。
    /// 输入绝对值小于该值时视为没有横向输入。
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float horizontalDeadZone = 0.1f;

    /// <summary>
    /// 纵向输入死区。
    /// 输入绝对值大于该值时才认为玩家选择了上分支或下分支。
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float verticalDeadZone = 0.5f;

    /// <summary>
    /// 上下分支输入缓存时间。
    /// 玩家提前按下上或下时，可以在这个时间内保留选择。
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float branchInputBufferTime = 0.15f;

    /// <summary>
    /// 节点到达误差。
    /// 角色距离路径端点小于该值时，认为角色站在节点上。
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float nodeArriveEpsilon = 0.02f;

    [Header("Physics")]

    /// <summary>
    /// 角色 Rigidbody2D。
    /// 如果存在，则使用 Rigidbody2D.MovePosition。
    /// 如果不存在，则直接修改 transform.position。
    /// </summary>
    [SerializeField]
    private Rigidbody2D rb;

    /// <summary>
    /// 当前路径段上的距离。
    /// 0 表示当前 Segment 起点。
    /// Segment.Length 表示当前 Segment 终点。
    /// </summary>
    [SerializeField]
    private float distanceOnSegment;

    private RailExitChoice2D bufferedVerticalChoice = RailExitChoice2D.None;
    private float bufferedVerticalTimer;

    /// <summary>
    /// 当前路径地图。
    /// </summary>
    public RailMap2DAsset RailMap
    {
        get { return railMap; }
    }

    /// <summary>
    /// 当前路径段 ID。
    /// </summary>
    public int CurrentSegmentId
    {
        get { return currentSegmentId; }
    }

    /// <summary>
    /// 当前路径段上的距离。
    /// </summary>
    public float DistanceOnSegment
    {
        get { return distanceOnSegment; }
    }

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Start()
    {
        InitializeStartPosition();
    }

    /// <summary>
    /// 初始化角色在路径上的起始位置。
    /// </summary>
    public void InitializeStartPosition()
    {
        if (railMap == null)
        {
            return;
        }

        if (!railMap.TryGetSegment(currentSegmentId, out RailSegment2D segment))
        {
            return;
        }

        EnsureSegmentLengthTable(segment);

        distanceOnSegment = segment.Length * Mathf.Clamp01(normalizedStartPosition);

        SnapToCurrentSegment();
    }

    /// <summary>
    /// 外部控制器每个物理帧调用。
    /// </summary>
    /// <param name="horizontalAxis">
    /// 横向输入轴。
    /// 大于 0 表示向右，小于 0 表示向左。
    /// </param>
    /// <param name="verticalAxis">
    /// 纵向输入轴。
    /// 大于 0 表示选择上分支，小于 0 表示选择下分支。
    /// </param>
    /// <param name="deltaTime">
    /// 当前物理帧间隔。
    /// 通常传入 Time.fixedDeltaTime。
    /// </param>
    public void TickMove(
        float horizontalAxis,
        float verticalAxis,
        float deltaTime)
    {
        RailExitChoice2D verticalChoice = ReadVerticalChoice(verticalAxis);
        int horizontalSign = ReadHorizontalSign(horizontalAxis);

        TickBranchInputBuffer(verticalChoice, deltaTime);
        MoveAlongCurrentSegment(horizontalSign, deltaTime);
    }

    /// <summary>
    /// 给 Editor 工具或初始化逻辑使用。
    /// 用来设置角色起始路径段。
    /// </summary>
    /// <param name="newRailMap">
    /// 新的运行时路径地图资产。
    /// </param>
    /// <param name="newSegmentId">
    /// 新的起始路径段 ID。
    /// </param>
    /// <param name="newNormalizedStartPosition">
    /// 新的起始归一化位置。
    /// 0 表示路径段起点，1 表示路径段终点。
    /// </param>
    /// <param name="snapImmediately">
    /// 是否立刻把角色吸附到新位置。
    /// Editor 工具中通常传 true。
    /// </param>
    public void SetStartForEditorOrRuntime(
        RailMap2DAsset newRailMap,
        int newSegmentId,
        float newNormalizedStartPosition,
        bool snapImmediately)
    {
        railMap = newRailMap;
        currentSegmentId = newSegmentId;
        normalizedStartPosition = Mathf.Clamp01(newNormalizedStartPosition);

        if (snapImmediately)
        {
            InitializeStartPosition();
        }
    }

    private RailExitChoice2D ReadVerticalChoice(float verticalAxis)
    {
        if (verticalAxis >= verticalDeadZone)
        {
            return RailExitChoice2D.Up;
        }

        if (verticalAxis <= -verticalDeadZone)
        {
            return RailExitChoice2D.Down;
        }

        return RailExitChoice2D.None;
    }

    private int ReadHorizontalSign(float horizontalAxis)
    {
        if (horizontalAxis >= horizontalDeadZone)
        {
            return 1;
        }

        if (horizontalAxis <= -horizontalDeadZone)
        {
            return -1;
        }

        return 0;
    }

    private void TickBranchInputBuffer(
        RailExitChoice2D verticalChoice,
        float deltaTime)
    {
        if (verticalChoice == RailExitChoice2D.Up ||
            verticalChoice == RailExitChoice2D.Down)
        {
            bufferedVerticalChoice = verticalChoice;
            bufferedVerticalTimer = branchInputBufferTime;
            return;
        }

        if (bufferedVerticalTimer <= 0f)
        {
            bufferedVerticalChoice = RailExitChoice2D.None;
            return;
        }

        bufferedVerticalTimer -= deltaTime;

        if (bufferedVerticalTimer <= 0f)
        {
            bufferedVerticalChoice = RailExitChoice2D.None;
        }
    }

    private void MoveAlongCurrentSegment(
        int horizontalSign,
        float deltaTime)
    {
        if (railMap == null)
        {
            return;
        }

        if (!railMap.TryGetSegment(currentSegmentId, out RailSegment2D segment))
        {
            return;
        }

        EnsureSegmentLengthTable(segment);

        distanceOnSegment += horizontalSign * moveSpeed * deltaTime;

        ResolveSegmentBoundary(segment, horizontalSign);
        SnapToCurrentSegment();
    }

    private void ResolveSegmentBoundary(
        RailSegment2D segment,
        int horizontalSign)
    {
        bool hasVerticalBranchChoice =
            bufferedVerticalChoice == RailExitChoice2D.Up ||
            bufferedVerticalChoice == RailExitChoice2D.Down;

        if (distanceOnSegment <= nodeArriveEpsilon)
        {
            bool wantsLeaveStart =
                distanceOnSegment < 0f ||
                horizontalSign < 0 ||
                hasVerticalBranchChoice;

            if (wantsLeaveStart)
            {
                float overflowDistance = Mathf.Max(0f, -distanceOnSegment);

                TrySwitchAtNode(
                    segment.startNodeId,
                    horizontalSign,
                    RailEndpoint2D.Start,
                    overflowDistance);
            }
            else
            {
                distanceOnSegment = Mathf.Max(0f, distanceOnSegment);
            }

            return;
        }

        if (distanceOnSegment >= segment.Length - nodeArriveEpsilon)
        {
            bool wantsLeaveEnd =
                distanceOnSegment > segment.Length ||
                horizontalSign > 0 ||
                hasVerticalBranchChoice;

            if (wantsLeaveEnd)
            {
                float overflowDistance = Mathf.Max(
                    0f,
                    distanceOnSegment - segment.Length);

                TrySwitchAtNode(
                    segment.endNodeId,
                    horizontalSign,
                    RailEndpoint2D.End,
                    overflowDistance);
            }
            else
            {
                distanceOnSegment = Mathf.Min(
                    segment.Length,
                    distanceOnSegment);
            }
        }
    }

    private void TrySwitchAtNode(
        int nodeId,
        int horizontalSign,
        RailEndpoint2D arrivedEndpoint,
        float overflowDistance)
    {
        RailExitChoice2D horizontalChoice = ToHorizontalChoice(horizontalSign);

        bool hasExit = railMap.TryResolveExit(
            nodeId,
            bufferedVerticalChoice,
            horizontalChoice,
            out RailExit2D exit);

        if (!hasExit)
        {
            ClampToCurrentSegmentEnd(arrivedEndpoint);
            return;
        }

        if (!railMap.TryGetSegment(exit.segmentId, out RailSegment2D nextSegment))
        {
            ClampToCurrentSegmentEnd(arrivedEndpoint);
            return;
        }

        EnsureSegmentLengthTable(nextSegment);

        currentSegmentId = exit.segmentId;

        if (exit.enterFrom == RailEndpoint2D.Start)
        {
            distanceOnSegment = overflowDistance;
        }
        else
        {
            distanceOnSegment = nextSegment.Length - overflowDistance;
        }

        distanceOnSegment = Mathf.Clamp(
            distanceOnSegment,
            0f,
            nextSegment.Length);

        bufferedVerticalChoice = RailExitChoice2D.None;
        bufferedVerticalTimer = 0f;
    }

    private static RailExitChoice2D ToHorizontalChoice(int horizontalSign)
    {
        if (horizontalSign > 0)
        {
            return RailExitChoice2D.Right;
        }

        if (horizontalSign < 0)
        {
            return RailExitChoice2D.Left;
        }

        return RailExitChoice2D.None;
    }

    private void ClampToCurrentSegmentEnd(RailEndpoint2D arrivedEndpoint)
    {
        if (!railMap.TryGetSegment(currentSegmentId, out RailSegment2D segment))
        {
            return;
        }

        EnsureSegmentLengthTable(segment);

        distanceOnSegment = arrivedEndpoint == RailEndpoint2D.Start
            ? 0f
            : segment.Length;
    }

    private void SnapToCurrentSegment()
    {
        if (railMap == null)
        {
            return;
        }

        if (!railMap.TryGetSegment(currentSegmentId, out RailSegment2D segment))
        {
            return;
        }

        EnsureSegmentLengthTable(segment);

        Vector2 targetPosition = segment.GetPointByDistance(distanceOnSegment);

        if (Application.isPlaying && rb != null)
        {
            rb.MovePosition(targetPosition);
            return;
        }

        transform.position = new Vector3(
            targetPosition.x,
            targetPosition.y,
            transform.position.z);
    }

    private static void EnsureSegmentLengthTable(RailSegment2D segment)
    {
        if (segment == null)
        {
            return;
        }

        bool needRebuild =
            segment.bakedPoints != null &&
            (segment.cumulativeLengths == null ||
             segment.cumulativeLengths.Length != segment.bakedPoints.Length);

        if (needRebuild)
        {
            segment.RebuildLengthTable();
        }
    }
}
