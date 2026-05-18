using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D 贝塞尔路径地图的编辑期组件。
/// 挂在场景对象 RailMap_OutsideDoor 上。
/// </summary>
public sealed class RailBezierMap2DAuthoring : MonoBehaviour
{
    /// <summary>
    /// 编辑器中的节点列表。
    /// </summary>
    public List<RailBezierNode2D> nodes = new List<RailBezierNode2D>();

    /// <summary>
    /// 编辑器中的路径段列表。
    /// </summary>
    public List<RailBezierSegment2D> segments = new List<RailBezierSegment2D>();

    /// <summary>
    /// 下一个节点 ID。
    /// 创建节点后自增，保证 ID 稳定且唯一。
    /// </summary>
    [SerializeField]
    private int nextNodeId = 1;

    /// <summary>
    /// 下一个路径段 ID。
    /// 创建路径段后自增，保证 ID 稳定且唯一。
    /// </summary>
    [SerializeField]
    private int nextSegmentId = 1;

    /// <summary>
    /// 创建一个新节点。
    /// </summary>
    /// <param name="position">
    /// 新节点的二维世界坐标。
    /// Editor 工具会把鼠标点击位置转换成这个值。
    /// </param>
    /// <returns>
    /// 返回新节点的 nodeId。
    /// </returns>
    public int CreateNode(Vector2 position)
    {
        int nodeId = nextNodeId++;

        nodes.Add(new RailBezierNode2D
        {
            nodeId = nodeId,
            displayName = $"Node_{nodeId}",
            position = position
        });

        return nodeId;
    }

    /// <summary>
    /// 创建一条连接两个节点的贝塞尔路径段。
    /// </summary>
    /// <param name="startNodeId">
    /// 起点节点 ID。
    /// </param>
    /// <param name="endNodeId">
    /// 终点节点 ID。
    /// </param>
    /// <returns>
    /// 返回新路径段的 segmentId。
    /// 如果节点无效，返回 -1。
    /// </returns>
    public int CreateSegment(
        int startNodeId,
        int endNodeId)
    {
        RailBezierNode2D startNode = FindNode(startNodeId);
        RailBezierNode2D endNode = FindNode(endNodeId);

        if (startNode == null || endNode == null)
        {
            return -1;
        }

        int segmentId = nextSegmentId++;

        Vector2 handleOffset = (endNode.position - startNode.position) / 3f;

        segments.Add(new RailBezierSegment2D
        {
            segmentId = segmentId,
            displayName = $"Segment_{segmentId}",
            startNodeId = startNodeId,
            endNodeId = endNodeId,
            startHandleOffset = handleOffset,
            endHandleOffset = -handleOffset,
            sampleCount = 32
        });

        return segmentId;
    }

    /// <summary>
    /// 根据节点 ID 查找编辑期节点。
    /// </summary>
    /// <param name="nodeId">
    /// 要查找的节点 ID。
    /// </param>
    /// <returns>
    /// 找到时返回节点对象。
    /// 找不到时返回 null。
    /// </returns>
    public RailBezierNode2D FindNode(int nodeId)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            RailBezierNode2D node = nodes[i];

            if (node != null && node.nodeId == nodeId)
            {
                return node;
            }
        }

        return null;
    }

    /// <summary>
    /// 根据路径段 ID 查找编辑期路径段。
    /// </summary>
    /// <param name="segmentId">
    /// 要查找的路径段 ID。
    /// </param>
    /// <returns>
    /// 找到时返回路径段对象。
    /// 找不到时返回 null。
    /// </returns>
    public RailBezierSegment2D FindSegment(int segmentId)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            RailBezierSegment2D segment = segments[i];

            if (segment != null && segment.segmentId == segmentId)
            {
                return segment;
            }
        }

        return null;
    }

    /// <summary>
    /// 烘焙所有路径段。
    /// </summary>
    public void BakeAll()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            RailBezierSegment2D segment = segments[i];

            if (segment == null)
            {
                continue;
            }

            segment.Bake(this);
        }
    }
}
