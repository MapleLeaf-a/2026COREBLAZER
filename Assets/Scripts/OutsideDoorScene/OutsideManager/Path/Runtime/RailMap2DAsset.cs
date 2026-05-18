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
    /// <param name="nodeId">
    /// 要查找的节点 ID。
    /// </param>
    /// <param name="node">
    /// 输出查找到的节点。
    /// 如果没有找到，会被设置为 null。
    /// </param>
    /// <returns>
    /// true 表示找到节点。
    /// false 表示没有找到节点。
    /// </returns>
    public bool TryGetNode(
        int nodeId,
        out RailNode2D node)
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
    /// 根据路径段 ID 查找路径段。
    /// </summary>
    /// <param name="segmentId">
    /// 要查找的路径段 ID。
    /// </param>
    /// <param name="segment">
    /// 输出查找到的路径段。
    /// 如果没有找到，会被设置为 null。
    /// </param>
    /// <returns>
    /// true 表示找到路径段。
    /// false 表示没有找到路径段。
    /// </returns>
    public bool TryGetSegment(
        int segmentId,
        out RailSegment2D segment)
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
    /// 在节点上根据输入选择出口。
    /// 优先级：上下分支选择 > 左右移动方向 > Auto 默认出口。
    /// </summary>
    /// <param name="nodeId">
    /// 当前到达的节点 ID。
    /// </param>
    /// <param name="verticalChoice">
    /// 上下分支选择。
    /// 没有上下输入时传 None。
    /// </param>
    /// <param name="horizontalChoice">
    /// 左右移动选择。
    /// 没有左右输入时传 None。
    /// </param>
    /// <param name="exit">
    /// 输出最终匹配到的出口。
    /// 如果没有匹配到，会被设置为 null。
    /// </param>
    /// <returns>
    /// true 表示找到出口。
    /// false 表示没有找到出口。
    /// </returns>
    public bool TryResolveExit(
        int nodeId,
        RailExitChoice2D verticalChoice,
        RailExitChoice2D horizontalChoice,
        out RailExit2D exit)
    {
        exit = null;

        if (!TryGetNode(nodeId, out RailNode2D node))
        {
            return false;
        }

        if ((verticalChoice == RailExitChoice2D.Up ||
             verticalChoice == RailExitChoice2D.Down) &&
            TryFindExit(node, verticalChoice, out exit))
        {
            return true;
        }

        if ((horizontalChoice == RailExitChoice2D.Left ||
             horizontalChoice == RailExitChoice2D.Right) &&
            TryFindExit(node, horizontalChoice, out exit))
        {
            return true;
        }

        return TryFindExit(
            node,
            RailExitChoice2D.Auto,
            out exit);
    }

    /// <summary>
    /// 从节点出口列表中查找指定方向的出口。
    /// </summary>
    /// <param name="node">
    /// 当前节点。
    /// </param>
    /// <param name="choice">
    /// 要查找的出口方向。
    /// </param>
    /// <param name="exit">
    /// 输出匹配到的出口。
    /// 如果没有找到，会被设置为 null。
    /// </param>
    /// <returns>
    /// true 表示找到出口。
    /// false 表示没有找到出口。
    /// </returns>
    private static bool TryFindExit(
        RailNode2D node,
        RailExitChoice2D choice,
        out RailExit2D exit)
    {
        exit = null;

        int bestPriority = int.MinValue;

        for (int i = 0; i < node.exits.Count; i++)
        {
            RailExit2D current = node.exits[i];

            if (current == null)
            {
                continue;
            }

            if (current.choice != choice)
            {
                continue;
            }

            if (current.priority > bestPriority)
            {
                bestPriority = current.priority;
                exit = current;
            }
        }

        return exit != null;
    }

    /// <summary>
    /// 重建所有路径段的累计长度表。
    /// 建议在导出、校验或运行时初始化时调用。
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
