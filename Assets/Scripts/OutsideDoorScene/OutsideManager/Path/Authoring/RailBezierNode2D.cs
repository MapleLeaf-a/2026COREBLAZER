using System;
using System.Collections.Generic;
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
    /// 节点稳定查询名。
    ///
    /// 用途：
    /// 运行时可以通过这个名字查找节点，
    /// 例如根据 "Spawn_Player_Start" 找到 Player 出生点。
    ///
    /// 和 displayName 的区别：
    /// displayName 主要给编辑器显示；
    /// nodeKey 主要给运行时代码查询。
    ///
    /// 约定：
    /// 1. 特殊节点必须填写 nodeKey。
    /// 2. nodeKey 在同一个 RailMap 中应保持唯一。
    /// 3. 普通节点可以留空。
    /// 4. nodeKey 不建议频繁修改，因为运行时代码会依赖它。
    /// </summary>
    public string nodeKey;

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

    /// <summary>
    /// 带来源 Segment 的出口规则列表。
    ///
    /// 新系统优先使用 exitRules。
    /// 旧字段 leftExitSegmentId / rightExitSegmentId 等
    /// 可以作为兼容的通用规则继续导出。
    /// </summary>
    public List<RailBezierExitRule2D> exitRules = new List<RailBezierExitRule2D>();
}

/// <summary>
/// 编辑期节点出口规则。
///
/// 它表示：
/// 在某个 Node 上，
/// 当 Player 从 fromSegmentId 进入，
/// 并且玩家输入 choice，
/// 就切换到 targetSegmentId。
/// </summary>
[Serializable]
public sealed class RailBezierExitRule2D
{
    /// <summary>
    /// 来源 Segment ID。
    ///
    /// -1 表示通用规则。
    /// 大于等于 0 表示只有从该 Segment 进入当前节点时才生效。
    /// </summary>
    public int fromSegmentId = -1;

    /// <summary>
    /// 输入选择。
    ///
    /// Up / Down / Left / Right / Auto。
    /// </summary>
    public RailExitChoice2D choice = RailExitChoice2D.Auto;

    /// <summary>
    /// 目标 Segment ID。
    ///
    /// Player 将切换到这条 Segment。
    /// </summary>
    public int targetSegmentId = -1;

    /// <summary>
    /// 优先级。
    ///
    /// 同一个 fromSegmentId 和 choice 下出现多个规则时，
    /// priority 越大越优先。
    /// </summary>
    public int priority = 0;
}
