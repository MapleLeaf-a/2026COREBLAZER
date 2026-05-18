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
    ///
    /// -1 表示尚未配置。
    ///
    /// 注意：
    /// Segment ID 是编辑器生成的稳定 ID，不保证从 0 开始。
    /// 所以不能把 0 当作默认合法值。
    /// </summary>
    [SerializeField]
    private int currentSegmentId = -1;

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

    /// <summary>
    /// 是否根据当前 Segment 的起点和终点 X 坐标自动修正输入方向。
    ///
    /// 开启后：
    /// 如果 Segment 是从左到右，按右 distance 增加。
    /// 如果 Segment 是从右到左，按右 distance 减少。
    ///
    /// 这样即使某条 Segment 没有被 Normalize，
    /// 玩家按右也仍然会在世界坐标上向右移动。
    /// </summary>
    [SerializeField]
    private bool autoMatchInputToWorldX = true;

    /// <summary>
    /// 是否输出路径移动调试日志。
    ///
    /// 开启后会打印：
    /// 1. 无效 currentSegmentId。
    /// 2. 自动切换到默认起始 Segment。
    /// 3. 节点缺少出口。
    /// 4. 目标 Segment 无效。
    /// </summary>
    [SerializeField]
    private bool logRailDebug = true;

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
    ///
    /// 执行顺序：
    /// 1. 检查 railMap 是否存在。
    /// 2. 优先使用 currentSegmentId。
    /// 3. currentSegmentId 无效时，尝试使用 RailMap.defaultStartSegmentId。
    /// 4. 默认起点无效时，尝试使用 RailMap 中第一条有效 Segment。
    /// 5. 根据 normalizedStartPosition 计算 distanceOnSegment。
    /// 6. 把角色吸附到路径点上。
    /// </summary>
    public void InitializeStartPosition()
    {
        if (railMap == null)
        {
            LogRailWarning("railMap is null. Player cannot move on rail.");
            return;
        }

        if (!TryResolveInitialSegment(out RailSegment2D segment))
        {
            LogRailWarning($"No valid start segment found. currentSegmentId={currentSegmentId}.");
            return;
        }

        EnsureSegmentLengthTable(segment);

        if (!IsSegmentUsable(segment))
        {
            LogRailWarning($"Start segment {segment.segmentId} has invalid bakedPoints or zero length.");
            return;
        }

        distanceOnSegment = segment.Length * Mathf.Clamp01(normalizedStartPosition);

        SnapToCurrentSegment();
    }

    /// <summary>
    /// 解析初始路径段。
    ///
    /// 优先级：
    /// 1. 当前 RailWalker2D.currentSegmentId。
    /// 2. RailMap2DAsset.defaultStartSegmentId。
    /// 3. RailMap2DAsset.segments 中第一条有效路径段。
    ///
    /// 这样即使 Editor 没有正确写入 currentSegmentId，
    /// 角色也不会完全静默卡死。
    /// </summary>
    /// <param name="segment">
    /// 输出解析到的初始路径段。
    /// 如果失败，输出 null。
    /// </param>
    /// <returns>
    /// true 表示成功找到可用路径段。
    /// false 表示 RailMap 中没有任何可用路径段。
    /// </returns>
    private bool TryResolveInitialSegment(out RailSegment2D segment)
    {
        if (railMap.TryGetSegment(currentSegmentId, out segment) && IsSegmentUsable(segment))
        {
            return true;
        }

        LogRailWarning($"currentSegmentId {currentSegmentId} is invalid. Trying defaultStartSegmentId.");

        if (railMap.TryGetDefaultStartSegment(out segment) && IsSegmentUsable(segment))
        {
            currentSegmentId = segment.segmentId;
            LogRailWarning($"Auto switched to defaultStartSegmentId {currentSegmentId}.");
            return true;
        }

        if (TryUseFirstAvailableSegment(out segment))
        {
            return true;
        }

        segment = null;
        return false;
    }

    /// <summary>
    /// 使用 RailMap 中第一条有效 Segment 作为兜底起点。
    ///
    /// 有效 Segment 的条件：
    /// 1. segment 不为空。
    /// 2. bakedPoints 至少有两个点。
    /// 3. Segment.Length 大于 0。
    ///
    /// 这个方法是容错逻辑。
    /// 正式关卡仍然应该通过 Editor Binding 明确配置 currentSegmentId。
    /// </summary>
    /// <param name="segment">
    /// 输出第一条有效 Segment。
    /// </param>
    /// <returns>
    /// true 表示找到并切换成功。
    /// false 表示没有可用 Segment。
    /// </returns>
    private bool TryUseFirstAvailableSegment(out RailSegment2D segment)
    {
        segment = null;

        if (railMap == null || railMap.segments == null)
        {
            return false;
        }

        for (int i = 0; i < railMap.segments.Count; i++)
        {
            RailSegment2D candidate = railMap.segments[i];

            if (!IsSegmentUsable(candidate))
            {
                continue;
            }

            currentSegmentId = candidate.segmentId;
            segment = candidate;

            LogRailWarning($"Auto switched to first available segment {currentSegmentId}.");

            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断路径段是否可用于角色移动。
    /// </summary>
    /// <param name="segment">
    /// 要检查的路径段。
    /// </param>
    /// <returns>
    /// true 表示路径段可用于移动。
    /// false 表示路径段为空、没有烘焙点，或者长度为 0。
    /// </returns>
    private static bool IsSegmentUsable(RailSegment2D segment)
    {
        if (segment == null)
        {
            return false;
        }

        if (segment.bakedPoints == null || segment.bakedPoints.Length < 2)
        {
            return false;
        }

        EnsureSegmentLengthTable(segment);

        return segment.Length > Mathf.Epsilon;
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

    /// <summary>
    /// 根据当前路径段的世界方向，计算 distanceOnSegment 的变化方向。
    ///
    /// 如果 autoMatchInputToWorldX 为 false：
    /// 直接返回 horizontalSign。
    ///
    /// 如果 autoMatchInputToWorldX 为 true：
    /// 使用 bakedPoints[0].x 和 bakedPoints[last].x 判断路径方向。
    ///
    /// 示例：
    /// bakedPoints[0].x 小于 bakedPoints[last].x，表示路径从左到右。
    /// 按右 horizontalSign = 1，distance 应该增加。
    ///
    /// bakedPoints[0].x 大于 bakedPoints[last].x，表示路径从右到左。
    /// 按右 horizontalSign = 1，distance 应该减少。
    /// </summary>
    /// <param name="segment">
    /// 当前路径段。
    /// </param>
    /// <param name="horizontalSign">
    /// 玩家横向输入方向。
    /// 1 表示按右，-1 表示按左，0 表示无输入。
    /// </param>
    /// <returns>
    /// 返回 distanceOnSegment 应该增加还是减少。
    /// 1 表示增加，-1 表示减少，0 表示不变。
    /// </returns>
    private int GetDistanceMoveSign(
        RailSegment2D segment,
        int horizontalSign)
    {
        if (horizontalSign == 0)
        {
            return 0;
        }

        if (!autoMatchInputToWorldX)
        {
            return horizontalSign;
        }

        if (segment == null || segment.bakedPoints == null || segment.bakedPoints.Length < 2)
        {
            return horizontalSign;
        }

        Vector2 firstPoint = segment.bakedPoints[0];
        Vector2 lastPoint = segment.bakedPoints[segment.bakedPoints.Length - 1];

        float directionX = lastPoint.x - firstPoint.x;

        if (Mathf.Abs(directionX) <= Mathf.Epsilon)
        {
            return horizontalSign;
        }

        int segmentWorldXSign = directionX > 0f
            ? 1
            : -1;

        return horizontalSign * segmentWorldXSign;
    }

    /// <summary>
    /// 根据玩家输入沿当前路径段移动。
    ///
    /// horizontalSign 是玩家输入方向：
    /// 1 表示按右。
    /// -1 表示按左。
    ///
    /// distanceMoveSign 是路径距离方向：
    /// 1 表示 distanceOnSegment 增加。
    /// -1 表示 distanceOnSegment 减少。
    ///
    /// 二者不一定相同。
    /// 如果当前 Segment 是从右往左烘焙，
    /// 按右时 distanceMoveSign 应该是 -1。
    /// </summary>
    /// <param name="horizontalSign">
    /// 玩家横向输入方向。
    /// </param>
    /// <param name="deltaTime">
    /// 当前物理帧间隔。
    /// </param>
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
            LogRailWarning($"Cannot move. Segment {currentSegmentId} does not exist.");
            return;
        }

        EnsureSegmentLengthTable(segment);

        if (!IsSegmentUsable(segment))
        {
            LogRailWarning($"Cannot move. Segment {currentSegmentId} is not usable.");
            return;
        }

        int distanceMoveSign = GetDistanceMoveSign(segment, horizontalSign);

        distanceOnSegment += distanceMoveSign * moveSpeed * deltaTime;

        ResolveSegmentBoundary(
            segment,
            horizontalSign,
            distanceMoveSign);

        SnapToCurrentSegment();
    }

    /// <summary>
    /// 处理角色到达当前 Segment 起点或终点后的换段逻辑。
    ///
    /// 注意：
    /// 是否离开 Start / End 应该看 distanceMoveSign，
    /// 而不是直接看 horizontalSign。
    ///
    /// 因为某些 Segment 可能从右往左烘焙，
    /// 此时玩家按右 horizontalSign = 1，
    /// 但 distanceMoveSign = -1，实际会走向 Start。
    /// </summary>
    /// <param name="segment">
    /// 当前路径段。
    /// </param>
    /// <param name="horizontalSign">
    /// 玩家横向输入方向。
    /// 用于在节点上选择 Left / Right 出口。
    /// </param>
    /// <param name="distanceMoveSign">
    /// 当前 distanceOnSegment 的变化方向。
    /// 用于判断角色正在离开 Start 还是 End。
    /// </param>
    private void ResolveSegmentBoundary(
        RailSegment2D segment,
        int horizontalSign,
        int distanceMoveSign)
    {
        bool hasVerticalBranchChoice =
            bufferedVerticalChoice == RailExitChoice2D.Up ||
            bufferedVerticalChoice == RailExitChoice2D.Down;

        if (distanceOnSegment <= nodeArriveEpsilon)
        {
            bool wantsLeaveStart =
                distanceOnSegment < 0f ||
                distanceMoveSign < 0 ||
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
                distanceMoveSign > 0 ||
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

    /// <summary>
    /// 在节点处尝试切换到下一条路径段。
    ///
    /// horizontalSign 用于选择节点出口：
    /// 1 -> Right
    /// -1 -> Left
    ///
    /// bufferedVerticalChoice 优先级更高：
    /// Up / Down 会先于 Left / Right 被选择。
    /// </summary>
    /// <param name="nodeId">
    /// 当前抵达的节点 ID。
    /// </param>
    /// <param name="horizontalSign">
    /// 玩家横向输入方向。
    /// </param>
    /// <param name="arrivedEndpoint">
    /// 当前角色是从当前 Segment 的哪一端抵达节点。
    /// 没有出口时用它把角色限制在当前端点。
    /// </param>
    /// <param name="overflowDistance">
    /// 当前帧越过节点后多走出的距离。
    /// 切换到下一段后要保留这部分距离，避免速度损失。
    /// </param>
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
            LogRailWarning(
                $"Node {nodeId} has no exit. " +
                $"verticalChoice={bufferedVerticalChoice}, " +
                $"horizontalChoice={horizontalChoice}, " +
                $"currentSegmentId={currentSegmentId}.");

            ClampToCurrentSegmentEnd(arrivedEndpoint);
            return;
        }

        if (!railMap.TryGetSegment(exit.segmentId, out RailSegment2D nextSegment))
        {
            LogRailWarning(
                $"Node {nodeId} resolved exit to segment {exit.segmentId}, " +
                "but that segment does not exist in railMap.");

            ClampToCurrentSegmentEnd(arrivedEndpoint);
            return;
        }

        EnsureSegmentLengthTable(nextSegment);

        if (!IsSegmentUsable(nextSegment))
        {
            LogRailWarning(
                $"Node {nodeId} resolved exit to segment {exit.segmentId}, " +
                "but that segment is not usable.");

            ClampToCurrentSegmentEnd(arrivedEndpoint);
            return;
        }

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

    /// <summary>
    /// 输出 RailWalker2D 调试警告。
    /// </summary>
    /// <param name="message">
    /// 警告内容。
    /// </param>
    private void LogRailWarning(string message)
    {
        if (!logRailDebug)
        {
            return;
        }

        Debug.LogWarning(
            $"{nameof(RailWalker2D)} [{name}]: {message}",
            this);
    }
}
