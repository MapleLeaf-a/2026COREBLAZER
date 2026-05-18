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
/// 节点表示起点、终点、普通连接点或分支点。
/// </summary>
[Serializable]
public sealed class RailNode2D
{
    /// <summary>
    /// 节点稳定 ID。
    /// 不能依赖列表下标，因为编辑器中列表顺序可能变化。
    /// </summary>
    public int nodeId;

    /// <summary>
    /// 节点二维世界坐标。
    /// 主要用于调试显示、出生点吸附和 Scene 视图预览。
    /// </summary>
    public Vector2 position;

    /// <summary>
    /// 当前节点拥有的出口列表。
    /// 普通节点可以只有 Left / Right。
    /// 分支点可以额外拥有 Up / Down / Auto。
    /// </summary>
    public List<RailExit2D> exits = new List<RailExit2D>();
}

/// <summary>
/// 节点出口。
/// 表示在某个节点上，某个输入选择应该进入哪条路径段。
/// </summary>
[Serializable]
public sealed class RailExit2D
{
    /// <summary>
    /// 触发这个出口的输入选择。
    /// 例如 Down 表示玩家在节点附近按下时进入这条出口。
    /// </summary>
    public RailExitChoice2D choice;

    /// <summary>
    /// 目标路径段 ID。
    /// 运行时会通过这个 ID 在 RailMap2DAsset 中查找 RailSegment2D。
    /// </summary>
    public int segmentId;

    /// <summary>
    /// 从目标路径段的哪一端进入。
    /// Start 表示 distanceOnSegment 从 0 开始。
    /// End 表示 distanceOnSegment 从 nextSegment.Length 开始。
    /// </summary>
    public RailEndpoint2D enterFrom;

    /// <summary>
    /// 同一个 choice 下的优先级。
    /// 第一版可以全部为 0。
    /// 如果以后一个节点配置多个 Down 出口，可以用它做优先选择。
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
            // 累加相邻烘焙点之间的距离，得到从起点到当前点的总路径长度。
            totalLength += Vector2.Distance(
                bakedPoints[i - 1],
                bakedPoints[i]);

            cumulativeLengths[i] = totalLength;
        }
    }

    /// <summary>
    /// 根据路径距离取得二维坐标。
    /// </summary>
    /// <param name="distance">
    /// 角色在当前路径段上已经走过的距离。
    /// 0 表示路径段起点。
    /// Length 表示路径段终点。
    /// </param>
    /// <returns>
    /// 返回路径上的二维世界坐标。
    /// 如果数据无效，返回 Vector2.zero。
    /// </returns>
    public Vector2 GetPointByDistance(float distance)
    {
        if (bakedPoints == null ||
            cumulativeLengths == null ||
            bakedPoints.Length == 0 ||
            cumulativeLengths.Length != bakedPoints.Length)
        {
            return Vector2.zero;
        }

        float clampedDistance = Mathf.Clamp(
            distance,
            0f,
            Length);

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

            return Vector2.Lerp(
                bakedPoints[i - 1],
                bakedPoints[i],
                lerpFactor);
        }

        return bakedPoints[bakedPoints.Length - 1];
    }
}
