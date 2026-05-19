using System;
using UnityEngine;

/// <summary>
/// 编辑期贝塞尔路径段。
/// P0 和 P3 来自节点，P1 和 P2 来自曲柄偏移。
/// </summary>
[Serializable]
public sealed class RailBezierSegment2D
{
    /// <summary>
    /// 路径段稳定 ID。
    /// 节点出口通过这个 ID 指向路径段。
    /// </summary>
    public int segmentId;

    /// <summary>
    /// 编辑器显示名称。
    /// </summary>
    public string displayName;

    /// <summary>
    /// 起点节点 ID。
    /// P0 使用该节点位置。
    /// </summary>
    public int startNodeId;

    /// <summary>
    /// 终点节点 ID。
    /// P3 使用该节点位置。
    /// </summary>
    public int endNodeId;

    /// <summary>
    /// 起点曲柄偏移。
    /// P1 = startNode.position + startHandleOffset。
    /// </summary>
    public Vector2 startHandleOffset;

    /// <summary>
    /// 终点曲柄偏移。
    /// P2 = endNode.position + endHandleOffset。
    /// </summary>
    public Vector2 endHandleOffset;

    /// <summary>
    /// 烘焙采样数量。
    /// 数值越大，烘焙点越密集。
    /// </summary>
    public int sampleCount = 32;

    /// <summary>
    /// 烘焙后的二维路径点。
    /// 这份数据用于编辑器预览，导出时会复制到 RailSegment2D。
    /// </summary>
    public Vector2[] bakedPoints = Array.Empty<Vector2>();

    /// <summary>
    /// 烘焙当前贝塞尔曲线段。
    /// </summary>
    /// <param name="map">
    /// 当前编辑期路径地图。
    /// 用于通过 startNodeId 和 endNodeId 查找节点坐标。
    /// </param>
    public void Bake(RailBezierMap2DAuthoring map)
    {
        RailBezierNode2D startNode = map.FindNode(startNodeId);
        RailBezierNode2D endNode = map.FindNode(endNodeId);

        if (startNode == null || endNode == null)
        {
            bakedPoints = Array.Empty<Vector2>();
            return;
        }

        int safeSampleCount = Mathf.Max(4, sampleCount);
        bakedPoints = new Vector2[safeSampleCount + 1];

        Vector2 p0 = startNode.position;
        Vector2 p1 = startNode.position + startHandleOffset;
        Vector2 p2 = endNode.position + endHandleOffset;
        Vector2 p3 = endNode.position;

        for (int i = 0; i <= safeSampleCount; i++)
        {
            float t = i / (float)safeSampleCount;
            bakedPoints[i] = EvaluateCubicBezier(p0, p1, p2, p3, t);
        }
    }

    /// <summary>
    /// 反转路径段方向。
    /// 用于把画反的路径修正为 bakedPoints[0] 是逻辑左端。
    /// </summary>
    public void Reverse()
    {
        int oldStartNodeId = startNodeId;
        startNodeId = endNodeId;
        endNodeId = oldStartNodeId;

        Vector2 oldStartHandleOffset = startHandleOffset;
        startHandleOffset = endHandleOffset;
        endHandleOffset = oldStartHandleOffset;

        if (bakedPoints != null)
        {
            Array.Reverse(bakedPoints);
        }
    }

    /// <summary>
    /// 重置曲柄为默认平滑状态。
    /// </summary>
    /// <param name="map">
    /// 当前编辑期路径地图。
    /// 用于查找起点和终点节点。
    /// </param>
    public void ResetHandles(RailBezierMap2DAuthoring map)
    {
        RailBezierNode2D startNode = map.FindNode(startNodeId);
        RailBezierNode2D endNode = map.FindNode(endNodeId);

        if (startNode == null || endNode == null)
        {
            return;
        }

        Vector2 handleOffset = (endNode.position - startNode.position) / 3f;

        startHandleOffset = handleOffset;
        endHandleOffset = -handleOffset;
    }

    /// <summary>
    /// 如果当前路径段是从右往左绘制，则反转为从左往右。
    ///
    /// 判断方式：
    /// startNode.position.x > endNode.position.x 时，认为它是右到左。
    ///
    /// 作用：
    /// 保证 bakedPoints[0] 更接近逻辑左端，
    /// bakedPoints[last] 更接近逻辑右端。
    ///
    /// 注意：
    /// 如果路线接近垂直，x 差值很小，
    /// 不建议自动归一化，应交给设计者手动判断。
    /// </summary>
    /// <param name="map">
    /// 当前编辑期路径地图。
    /// </param>
    /// <returns>
    /// true 表示发生了反转。
    /// false 表示没有反转。
    /// </returns>
    public bool NormalizeLeftToRight(RailBezierMap2DAuthoring map)
    {
        if (map == null)
        {
            return false;
        }

        RailBezierNode2D startNode = map.FindNode(startNodeId);
        RailBezierNode2D endNode = map.FindNode(endNodeId);

        if (startNode == null || endNode == null)
        {
            return false;
        }

        float deltaX = endNode.position.x - startNode.position.x;

        if (Mathf.Abs(deltaX) <= 0.001f)
        {
            return false;
        }

        if (deltaX > 0f)
        {
            return false;
        }

        Reverse();
        Bake(map);

        return true;
    }

    /// <summary>
    /// 计算三次贝塞尔曲线上的点。
    /// </summary>
    /// <param name="p0">
    /// 曲线起点。
    /// </param>
    /// <param name="p1">
    /// 起点曲柄控制点。
    /// </param>
    /// <param name="p2">
    /// 终点曲柄控制点。
    /// </param>
    /// <param name="p3">
    /// 曲线终点。
    /// </param>
    /// <param name="t">
    /// 曲线参数。
    /// 0 表示起点，1 表示终点。
    /// </param>
    /// <returns>
    /// 返回曲线上的二维坐标。
    /// </returns>
    private static Vector2 EvaluateCubicBezier(
        Vector2 p0,
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        float t)
    {
        t = Mathf.Clamp01(t);

        float u = 1f - t;

        return
            u * u * u * p0 +
            3f * u * u * t * p1 +
            3f * u * t * t * p2 +
            t * t * t * p3;
    }
}
