using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 运行时 2D 路径地图资产。
/// 它保存所有节点、路径段和分支出口。
/// </summary>
[CreateAssetMenu(menuName = "COREBLAZER/Rail Map 2D")]
public sealed class RailMap2DAsset : ScriptableObject
{
    /// <summary>
    /// 默认起始路径段 ID。
    /// -1 表示没有默认起点。
    /// </summary>
    public int defaultStartSegmentId = -1;

    /// <summary>
    /// 地图里的所有运行时节点。
    /// </summary>
    public List<RailNode2D> nodes = new List<RailNode2D>();

    /// <summary>
    /// 地图里的所有运行时路径段。
    /// </summary>
    public List<RailSegment2D> segments = new List<RailSegment2D>();

    /// <summary>
    /// 根据节点 ID 查找节点。
    /// </summary>
    public bool TryGetNode(int nodeId, out RailNode2D node)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            RailNode2D current = nodes[i];

            if (current != null && current.nodeId == nodeId)
            {
                node = current;
                return true;
            }
        }

        node = null;
        return false;
    }

    /// <summary>
    /// 根据 nodeKey 查找运行时节点。
    /// </summary>
    public bool TryGetNodeByKey(string nodeKey, out RailNode2D node)
    {
        if (string.IsNullOrWhiteSpace(nodeKey))
        {
            node = null;
            return false;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            RailNode2D current = nodes[i];

            if (current == null)
            {
                continue;
            }

            if (string.Equals(current.nodeKey, nodeKey, System.StringComparison.Ordinal))
            {
                node = current;
                return true;
            }
        }

        node = null;
        return false;
    }

    /// <summary>
    /// 根据 nodeKey 获取节点世界坐标。
    /// </summary>
    public bool TryGetNodePositionByKey(string nodeKey, out Vector2 position)
    {
        if (TryGetNodeByKey(nodeKey, out RailNode2D node))
        {
            position = node.position;
            return true;
        }

        position = Vector2.zero;
        return false;
    }

    /// <summary>
    /// 根据路径段 ID 查找路径段。
    /// </summary>
    public bool TryGetSegment(int segmentId, out RailSegment2D segment)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            RailSegment2D current = segments[i];

            if (current != null && current.segmentId == segmentId)
            {
                segment = current;
                return true;
            }
        }

        segment = null;
        return false;
    }

    /// <summary>
    /// 尝试获取默认起始路径段。
    /// </summary>
    public bool TryGetDefaultStartSegment(out RailSegment2D segment)
    {
        if (defaultStartSegmentId < 0)
        {
            segment = null;
            return false;
        }

        return TryGetSegment(defaultStartSegmentId, out segment);
    }

    /// <summary>
    /// 查找连接某个节点的第一条路径段。
    /// </summary>
    public bool TryGetFirstConnectedSegment(
        int nodeId,
        int exceptSegmentId,
        out RailSegment2D segment,
        out RailEndpoint2D enterFrom)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            RailSegment2D current = segments[i];

            if (current == null)
            {
                continue;
            }

            if (current.segmentId == exceptSegmentId)
            {
                continue;
            }

            if (current.startNodeId == nodeId)
            {
                segment = current;
                enterFrom = RailEndpoint2D.Start;
                return true;
            }

            if (current.endNodeId == nodeId)
            {
                segment = current;
                enterFrom = RailEndpoint2D.End;
                return true;
            }
        }

        segment = null;
        enterFrom = RailEndpoint2D.Start;
        return false;
    }

    /// <summary>
    /// 根据世界坐标查找最近的 Rail Segment。
    /// </summary>
    public bool TryFindNearestRailPoint(Vector2 worldPosition, out RailAttachResult2D result)
    {
        bool hasResult = false;

        int bestSegmentId = -1;
        float bestDistanceOnSegment = 0f;
        Vector2 bestPosition = Vector2.zero;
        float bestSqrDistance = float.PositiveInfinity;

        for (int i = 0; i < segments.Count; i++)
        {
            RailSegment2D segment = segments[i];

            if (segment == null ||
                segment.bakedPoints == null ||
                segment.bakedPoints.Length < 2)
            {
                continue;
            }

            segment.RebuildLengthTable();

            for (int pointIndex = 1; pointIndex < segment.bakedPoints.Length; pointIndex++)
            {
                Vector2 a = segment.bakedPoints[pointIndex - 1];
                Vector2 b = segment.bakedPoints[pointIndex];

                Vector2 nearestOnLine = ProjectPointToLineSegment(
                    worldPosition, a, b, out float lerpFactor);

                float sqrDistance = (worldPosition - nearestOnLine).sqrMagnitude;

                if (sqrDistance >= bestSqrDistance)
                {
                    continue;
                }

                float previousDistance = segment.cumulativeLengths[pointIndex - 1];
                float nextDistance = segment.cumulativeLengths[pointIndex];

                float distanceOnSegment = Mathf.Lerp(previousDistance, nextDistance, lerpFactor);

                bestSqrDistance = sqrDistance;
                bestSegmentId = segment.segmentId;
                bestDistanceOnSegment = distanceOnSegment;
                bestPosition = nearestOnLine;
                hasResult = true;
            }
        }

        if (!hasResult)
        {
            result = default;
            return false;
        }

        result = new RailAttachResult2D(
            bestSegmentId,
            bestDistanceOnSegment,
            bestPosition,
            Mathf.Sqrt(bestSqrDistance));

        return true;
    }

    /// <summary>
    /// 把一个点投影到线段上。
    /// </summary>
    private static Vector2 ProjectPointToLineSegment(
        Vector2 point, Vector2 a, Vector2 b, out float lerpFactor)
    {
        Vector2 ab = b - a;
        float abSqrMagnitude = ab.sqrMagnitude;

        if (abSqrMagnitude <= Mathf.Epsilon)
        {
            lerpFactor = 0f;
            return a;
        }

        lerpFactor = Vector2.Dot(point - a, ab) / abSqrMagnitude;
        lerpFactor = Mathf.Clamp01(lerpFactor);

        return Vector2.Lerp(a, b, lerpFactor);
    }

    /// <summary>
    /// 根据当前节点、来源 Segment、Vertical 输入和 Horizontal 输入解析出口。
    /// </summary>
    public bool TryResolveBranchExit(
        int nodeId,
        int fromSegmentId,
        RailExitChoice2D verticalChoice,
        RailExitChoice2D horizontalChoice,
        out RailExit2D exit)
    {
        exit = null;

        if (!TryGetNode(nodeId, out RailNode2D node))
        {
            return false;
        }

        if (verticalChoice == RailExitChoice2D.Up ||
            verticalChoice == RailExitChoice2D.Down)
        {
            if (TryFindExitForSource(node, fromSegmentId, verticalChoice, out exit))
            {
                return true;
            }
        }

        if (verticalChoice == RailExitChoice2D.None)
        {
            if (TryFindExitForSource(node, fromSegmentId, RailExitChoice2D.Auto, out exit))
            {
                return true;
            }
        }

        if (horizontalChoice == RailExitChoice2D.Left ||
            horizontalChoice == RailExitChoice2D.Right)
        {
            if (TryFindExitForSource(node, fromSegmentId, horizontalChoice, out exit))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 查找指定来源 Segment 下的出口。
    /// </summary>
    private static bool TryFindExitForSource(
        RailNode2D node,
        int fromSegmentId,
        RailExitChoice2D choice,
        out RailExit2D exit)
    {
        exit = null;

        if (node == null || node.exits == null)
        {
            return false;
        }

        RailExit2D bestExactExit = null;
        RailExit2D bestFallbackExit = null;

        for (int i = 0; i < node.exits.Count; i++)
        {
            RailExit2D candidate = node.exits[i];

            if (candidate == null)
            {
                continue;
            }

            if (candidate.choice != choice)
            {
                continue;
            }

            if (candidate.fromSegmentId == fromSegmentId)
            {
                if (bestExactExit == null || candidate.priority > bestExactExit.priority)
                {
                    bestExactExit = candidate;
                }
                continue;
            }

            if (candidate.fromSegmentId < 0)
            {
                if (bestFallbackExit == null || candidate.priority > bestFallbackExit.priority)
                {
                    bestFallbackExit = candidate;
                }
            }
        }

        if (bestExactExit != null)
        {
            exit = bestExactExit;
            return true;
        }

        if (bestFallbackExit != null)
        {
            exit = bestFallbackExit;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 重建所有路径段的累计长度表。
    /// </summary>
    public void RebuildAllLengthTables()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            RailSegment2D segment = segments[i];

            if (segment == null)
            {
                continue;
            }

            segment.RebuildLengthTable();
        }
    }
}
