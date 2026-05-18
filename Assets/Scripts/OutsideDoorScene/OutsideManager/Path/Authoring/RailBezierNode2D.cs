using System;
using UnityEngine;

/// <summary>
/// 编辑期路径节点。
/// 节点用于表示起点、终点、普通连接点和分支点。
/// </summary>
[Serializable]
public sealed class RailBezierNode2D
{
    /// <summary>
    /// 节点稳定 ID。
    /// 导出运行时资产时会保留这个 ID。
    /// </summary>
    public int nodeId;

    /// <summary>
    /// 编辑器显示名称。
    /// 只用于编辑器中识别节点。
    /// </summary>
    public string displayName;

    /// <summary>
    /// 节点二维世界坐标。
    /// Scene 视图中的节点拖动会修改这个值。
    /// </summary>
    public Vector2 position;

    /// <summary>
    /// 按左方向时进入的路径段 ID。
    /// -1 表示没有配置。
    /// </summary>
    public int leftExitSegmentId = -1;

    /// <summary>
    /// 按右方向时进入的路径段 ID。
    /// -1 表示没有配置。
    /// </summary>
    public int rightExitSegmentId = -1;

    /// <summary>
    /// 按上方向时进入的路径段 ID。
    /// -1 表示没有配置。
    /// </summary>
    public int upExitSegmentId = -1;

    /// <summary>
    /// 按下方向时进入的路径段 ID。
    /// -1 表示没有配置。
    /// </summary>
    public int downExitSegmentId = -1;

    /// <summary>
    /// 没有明确上下选择时使用的默认路径段 ID。
    /// -1 表示没有配置。
    /// </summary>
    public int autoExitSegmentId = -1;
}
