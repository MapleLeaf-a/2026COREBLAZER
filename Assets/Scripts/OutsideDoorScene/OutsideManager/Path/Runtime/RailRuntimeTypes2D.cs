using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 节点出口选择。
/// Left / Right 用于左右移动方向。
/// Up / Down 用于分支路线选择。
/// Auto 用于没有明确上下选择时的默认出口。
/// </summary>
public enum RailExitChoice2D
{
    None = 0,
    Left = 1,
    Right = 2,
    Up = 3,
    Down = 4,
    Auto = 5
}

/// <summary>
/// 从路径段哪一端进入。
/// Start 表示从 bakedPoints[0] 进入。
/// End 表示从 bakedPoints[bakedPoints.Length - 1] 进入。
/// </summary>
public enum RailEndpoint2D
{
    Start = 0,
    End = 1
}

/// <summary>
/// 运行时路径节点。
///
/// 节点可以表示：
/// 1. 路线起点。
/// 2. 路线终点。
/// 3. 普通连接点。
/// 4. 分支点。
/// 5. 特殊出生点。
/// </summary>
[Serializable]
public sealed class RailNode2D
{
    /// <summary>
    /// 节点稳定 ID。
    ///
    /// 这个 ID 由编辑器创建节点时生成。
    /// 不要依赖 nodes 列表下标，因为列表顺序可能变化。
    /// </summary>
    public int nodeId;

    /// <summary>
    /// 节点稳定查询名。
    ///
    /// 用途：
    /// 1. 运行时根据名字查找出生点。
    /// 2. 切场景后把 Player 放到某个门口。
    /// 3. 剧情系统把 Player 放到指定站位点。
    ///
    /// 示例：
    /// Spawn_Player_Start
    /// Spawn_Door_Left
    /// Spawn_Door_Right
    /// Fork_OutsideDoor_01
    /// </summary>
    public string nodeKey;

    /// <summary>
    /// 节点二维世界坐标。
    ///
    /// 用途：
    /// 1. Player 从 nodeKey 生成时使用。
    /// 2. 特效、NPC、提示标记定位时使用。
    /// 3. Editor 预览节点位置时使用。
    /// </summary>
    public Vector2 position;

    /// <summary>
    /// 当前节点拥有的出口列表。
    ///
    /// 出口不再只是 Node 全局出口。
    /// 每个出口都可以用 fromSegmentId 限定来源 Segment。
    /// 这样同一个分支点从正向和反向进入时，可以有不同的 Up / Down / Auto 规则。
    /// </summary>
    public List<RailExit2D> exits = new List<RailExit2D>();
}

/// <summary>
/// 运行时节点出口规则。
///
/// 一个出口表示：
/// 角色到达某个节点时，如果来源 Segment 和输入选择匹配，
/// 就进入指定目标 Segment。
///
/// 核心规则：
/// Node + fromSegmentId + choice -> segmentId
/// </summary>
[Serializable]
public sealed class RailExit2D
{
    /// <summary>
    /// 出口对应的输入选择。
    ///
    /// Up：
    ///     玩家选择上方路线。
    ///
    /// Down：
    ///     玩家选择下方路线。
    ///
    /// Left：
    ///     玩家选择向左方向路线，通常用于返回上一段。
    ///
    /// Right：
    ///     玩家选择向右方向路线，通常用于继续前进。
    ///
    /// Auto：
    ///     玩家没有主动选择上下分支时使用的默认路线。
    /// </summary>
    public RailExitChoice2D choice;

    /// <summary>
    /// 当前出口指向的目标 Segment ID。
    ///
    /// Player 切换到这个 Segment 后，
    /// RailWalker2D.currentSegmentId 会被设置为这个值。
    /// </summary>
    public int segmentId;

    /// <summary>
    /// 进入目标 Segment 时，从目标 Segment 的哪一端进入。
    ///
    /// 如果目标 Segment 的 startNodeId 是当前节点，
    /// enterFrom 应为 Start。
    ///
    /// 如果目标 Segment 的 endNodeId 是当前节点，
    /// enterFrom 应为 End。
    /// </summary>
    public RailEndpoint2D enterFrom;

    /// <summary>
    /// 这个出口规则适用的来源 Segment ID。
    ///
    /// -1：
    ///     通用规则。
    ///     不管 Player 从哪条 Segment 进入当前节点，都可以使用。
    ///
    /// 大于等于 0：
    ///     精确来源规则。
    ///     只有 Player 从这个 Segment 进入当前节点时，才可以使用。
    ///
    /// 用途：
    /// 解决反向进入同一个分支点时，
    /// Up / Down / Auto 语义不同的问题。
    /// </summary>
    public int fromSegmentId = -1;

    /// <summary>
    /// 出口优先级。
    ///
    /// 同一个 choice 和 fromSegmentId 下如果存在多个出口，
    /// priority 数字越大，越优先。
    /// </summary>
    public int priority;
}

/// <summary>
/// 运行时路径段。
/// 它保存烘焙后的路径点，并提供"路径距离 -> 二维坐标"的转换。
/// </summary>
[Serializable]
public sealed class RailSegment2D
{
    /// <summary>
    /// 路径段稳定 ID。
    /// 节点出口通过这个 ID 指向路径段。
    /// </summary>
    public int segmentId;

    /// <summary>
    /// 起点节点 ID。
    /// bakedPoints[0] 应该贴近该节点坐标。
    /// </summary>
    public int startNodeId;

    /// <summary>
    /// 终点节点 ID。
    /// bakedPoints[bakedPoints.Length - 1] 应该贴近该节点坐标。
    /// </summary>
    public int endNodeId;

    /// <summary>
    /// 贝塞尔曲线烘焙出来的二维路径点。
    /// 运行时角色会沿这组点移动。
    /// </summary>
    public Vector2[] bakedPoints = Array.Empty<Vector2>();

    /// <summary>
    /// 累计长度表。
    /// cumulativeLengths[i] 表示从 bakedPoints[0] 走到 bakedPoints[i] 的路径距离。
    /// </summary>
    public float[] cumulativeLengths = Array.Empty<float>();

    /// <summary>
    /// 当前路径段总长度。
    /// </summary>
    public float Length
    {
        get
        {
            if (cumulativeLengths == null || cumulativeLengths.Length == 0)
            {
                return 0f;
            }

            return cumulativeLengths[cumulativeLengths.Length - 1];
        }
    }

    /// <summary>
    /// 重建累计长度表。
    /// 每次 bakedPoints 改变后都必须调用。
    /// </summary>
    public void RebuildLengthTable()
    {
        if (bakedPoints == null || bakedPoints.Length == 0)
        {
            cumulativeLengths = Array.Empty<float>();
            return;
        }

        cumulativeLengths = new float[bakedPoints.Length];
        cumulativeLengths[0] = 0f;

        float totalLength = 0f;

        for (int i = 1; i < bakedPoints.Length; i++)
        {
            totalLength += Vector2.Distance(
                bakedPoints[i - 1],
                bakedPoints[i]);

            cumulativeLengths[i] = totalLength;
        }
    }

    /// <summary>
    /// 根据路径距离取得二维坐标。
    /// </summary>
    public Vector2 GetPointByDistance(float distance)
    {
        if (bakedPoints == null ||
            cumulativeLengths == null ||
            bakedPoints.Length == 0 ||
            cumulativeLengths.Length != bakedPoints.Length)
        {
            return Vector2.zero;
        }

        float clampedDistance = Mathf.Clamp(distance, 0f, Length);

        for (int i = 1; i < cumulativeLengths.Length; i++)
        {
            if (cumulativeLengths[i] < clampedDistance)
            {
                continue;
            }

            float previousDistance = cumulativeLengths[i - 1];
            float nextDistance = cumulativeLengths[i];
            float localLength = nextDistance - previousDistance;

            float lerpFactor = localLength <= Mathf.Epsilon
                ? 0f
                : (clampedDistance - previousDistance) / localLength;

            return Vector2.Lerp(bakedPoints[i - 1], bakedPoints[i], lerpFactor);
        }

        return bakedPoints[bakedPoints.Length - 1];
    }
}

/// <summary>
/// Player 从任意世界坐标接入 Rail 路线后的结果。
///
/// 它表示：
/// 某个世界坐标点，最接近哪条 Segment，
/// 并且对应这条 Segment 上的哪个路径距离。
/// </summary>
[Serializable]
public struct RailAttachResult2D
{
    /// <summary>
    /// 最近的 Segment ID。
    /// </summary>
    public int segmentId;

    /// <summary>
    /// 最近点在 Segment 上的路径距离。
    /// </summary>
    public float distanceOnSegment;

    /// <summary>
    /// Segment 上距离输入点最近的位置。
    /// </summary>
    public Vector2 nearestPosition;

    /// <summary>
    /// 输入点到最近路径点的距离。
    /// </summary>
    public float distanceToRail;

    /// <summary>
    /// 构造 Rail 接入结果。
    /// </summary>
    public RailAttachResult2D(
        int segmentId,
        float distanceOnSegment,
        Vector2 nearestPosition,
        float distanceToRail)
    {
        this.segmentId = segmentId;
        this.distanceOnSegment = distanceOnSegment;
        this.nearestPosition = nearestPosition;
        this.distanceToRail = distanceToRail;
    }
}
