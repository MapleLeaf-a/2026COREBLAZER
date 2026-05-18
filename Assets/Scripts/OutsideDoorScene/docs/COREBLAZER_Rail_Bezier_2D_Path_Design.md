# COREBLAZER 2D 贝塞尔路径编辑器与角色控制器设计方案

## 1. 文档目标

本文档用于交给 Codex 完成实现。

目标是在 Unity 中实现一套适合 `OutsideDoor` 场景使用的 2D 贝塞尔曲线路径系统，包含：

1. Editor 曲线编辑工具。
2. 贝塞尔曲线节点与曲柄编辑。
3. 曲线烘焙为 `Vector2[]` 路径点。
4. 分支点路线配置。
5. 运行时角色沿路径点移动。
6. Unity 老 Input 系统输入映射。
7. Editor 工具与角色控制器绑定，方便直接设置角色出生路径。

最终使用体验应该是：

```text
1. 打开 Tools / COREBLAZER / Rail Bezier 2D Editor。
2. Node 模式点击场景创建节点。
3. Segment 模式点击两个节点创建贝塞尔曲线。
4. Edit 模式拖动节点和曲柄调整路线。
5. 选中分支节点，配置 Left / Right / Up / Down / Auto 出口。
6. Bake / Export 生成运行时 RailMap2DAsset。
7. 在 Character Binding 面板中绑定 Player 角色和起始 Segment。
8. 运行游戏后，角色使用 A/D 或左右方向键沿曲线移动。
9. W/S 或上下方向键只在分支点选择上方或下方路线。
```

---

## 2. 当前移动逻辑需要替换的方向

当前角色移动方式是自由移动：

```text
读取 MoveUp / MoveDown / MoveLeft / MoveRight
组合为 moveDirection
用 Rigidbody2D.MovePosition 直接移动角色
```

新方案需要改为路径移动：

```text
读取 Horizontal / Vertical
Horizontal 只控制当前路径段上的 distanceOnSegment 增减
Vertical 不直接移动角色，只缓存 Up / Down 分支选择
角色位置由 currentSegmentId + distanceOnSegment 映射到路径点
```

---

## 3. 名词说明

### 3.1 Bézier Curve，贝塞尔曲线

贝塞尔曲线是一种由控制点控制形状的平滑曲线。

本方案使用三次贝塞尔曲线：

```text
P0：曲线起点
P1：起点曲柄控制点
P2：终点曲柄控制点
P3：曲线终点
```

其中：

```text
P0 = startNode.position
P3 = endNode.position
P1 = startNode.position + startHandleOffset
P2 = endNode.position + endHandleOffset
```

### 3.2 Handle，曲柄

曲柄用于控制曲线弯曲方向和弯曲程度。

曲柄不作为独立 Node 存在，而是保存在 Segment 中：

```text
startHandleOffset
endHandleOffset
```

这样同一个分支节点连接多条曲线时，每条曲线都有自己的曲柄，互不影响。

### 3.3 Node，节点

节点表示路径连接点。

节点可以是：

```text
起点
终点
普通连接点
分支点
```

分支点本质上也是 Node，只是它拥有多个出口。

### 3.4 Segment，路径段

Segment 表示两个 Node 之间的一条贝塞尔曲线。

一条 Segment 保存：

```text
segmentId
startNodeId
endNodeId
startHandleOffset
endHandleOffset
bakedPoints
cumulativeLengths
```

### 3.5 Bake，烘焙

烘焙就是把贝塞尔曲线转换成固定的 `Vector2[]` 点数组。

运行时角色不直接计算贝塞尔曲线，而是读取烘焙后的点数组。

### 3.6 cumulativeLengths，累计长度表

`cumulativeLengths[i]` 表示从 `bakedPoints[0]` 走到 `bakedPoints[i]` 的总距离。

它的作用是把：

```text
路径距离 distanceOnSegment
```

映射为：

```text
bakedPoints 中两个相邻点之间的插值坐标
```

这样角色移动速度不会因为采样点密度变化而明显变化。

### 3.7 Topology，拓扑

拓扑表示路径连接关系。

本方案必须使用：

```text
nodeId -> segmentId
```

不要使用：

```text
bakedPoints[index] -> bakedPoints[index]
```

原因是重新烘焙曲线或改变采样数量后，数组下标会变化。

---

## 4. 总体架构

系统拆成三层：

```text
Editor Authoring Layer
    负责编辑节点、曲线、曲柄和分支出口。

Runtime Rail Layer
    负责保存烘焙后的路径数据，并提供距离到坐标的映射。

Character Control Layer
    负责读取输入、更新角色朝向和动画，把输入传给 RailWalker2D。
```

推荐目录：

```text
Assets/Scripts/OutsideDoorScene/OutsideManager/Path
├── Authoring
│   ├── RailBezierMap2DAuthoring.cs
│   ├── RailBezierNode2D.cs
│   └── RailBezierSegment2D.cs
│
├── Runtime
│   ├── RailRuntimeTypes2D.cs
│   ├── RailMap2DAsset.cs
│   └── RailWalker2D.cs
│
└── Editor
    └── RailBezier2DEditorWindow.cs

Assets/Scripts/OutsideDoorScene/OutsideManager/Manager/Character
└── OutsideDoorCharacterController.cs
```

第一版建议不使用 namespace，保持和当前 Unity 项目脚本风格一致，降低接入成本。

---

## 5. 分支点设计

### 5.1 分支点必须是 Node

错误设计：

```text
一条长曲线中间的 bakedPoints[37] 是分支点
```

正确设计：

```text
Segment_01_StartToFork
    endNode = Node_02_Fork

Segment_02_ForkToHouse
    startNode = Node_02_Fork

Segment_03_ForkToLower
    startNode = Node_02_Fork
```

### 5.2 分支点出口配置

分支点 `Node_02_Fork` 应该配置：

```text
Left Exit  -> Segment_01_StartToFork
Right Exit -> Segment_02_ForkToHouse
Down Exit  -> Segment_03_ForkToLower
Auto Exit  -> Segment_02_ForkToHouse
```

运行时选择优先级：

```text
1. 如果有 Up / Down 缓存，并且节点存在对应出口，则优先走 Up / Down。
2. 否则根据 Horizontal 选择 Left / Right。
3. 否则尝试 Auto。
4. 如果没有出口，则停在节点。
```

### 5.3 enterFrom 的意义

当节点出口指向某个 Segment 时，需要知道从该 Segment 的哪一端进入。

```text
enterFrom = Start
    表示从 bakedPoints[0] 进入目标路径段。

enterFrom = End
    表示从 bakedPoints[last] 进入目标路径段。
```

Exporter 可以根据当前 nodeId 自动判断：

```text
如果 nodeId == targetSegment.startNodeId
    enterFrom = Start

如果 nodeId == targetSegment.endNodeId
    enterFrom = End

否则说明出口配置错误
```

---

## 6. 运行时数据结构

### 6.1 RailRuntimeTypes2D.cs

```csharp
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
/// 它保存烘焙后的路径点，并提供“路径距离 -> 二维坐标”的转换。
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
```

---

## 7. 运行时地图资产

### 7.1 RailMap2DAsset.cs

```csharp
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
```

---

## 8. 编辑期数据结构

### 8.1 RailBezierNode2D.cs

```csharp
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
```

### 8.2 RailBezierSegment2D.cs

```csharp
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
```

### 8.3 RailBezierMap2DAuthoring.cs

```csharp
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
```

---

## 9. Export 运行时资产

Editor 工具需要支持：

```text
Bake All
Export Runtime Asset
```

`Export Runtime Asset` 将 `RailBezierMap2DAuthoring` 转换为 `RailMap2DAsset`。

转换规则：

```text
Authoring Node -> RailNode2D
Authoring Segment -> RailSegment2D
Authoring Node 的 left/right/up/down/auto segmentId -> RailExit2D
根据 nodeId 和 targetSegment 自动推导 enterFrom
```

### 9.1 出口转换规则

```csharp
/// <summary>
/// 尝试把编辑期节点出口转换成运行时出口。
/// </summary>
/// <param name="map">
/// 编辑期路径地图。
/// 用于查找目标 Segment。
/// </param>
/// <param name="ownerNodeId">
/// 当前拥有出口的节点 ID。
/// 用它判断目标 Segment 是从 Start 进入还是 End 进入。
/// </param>
/// <param name="choice">
/// 出口触发方向。
/// 例如 Left、Right、Up、Down 或 Auto。
/// </param>
/// <param name="targetSegmentId">
/// 目标路径段 ID。
/// -1 表示没有配置出口。
/// </param>
/// <param name="exit">
/// 输出转换后的运行时出口。
/// 如果转换失败，输出 null。
/// </param>
/// <returns>
/// true 表示成功转换。
/// false 表示没有出口或出口配置无效。
/// </returns>
private static bool TryBuildRuntimeExit(
    RailBezierMap2DAuthoring map,
    int ownerNodeId,
    RailExitChoice2D choice,
    int targetSegmentId,
    out RailExit2D exit)
{
    exit = null;

    if (targetSegmentId < 0)
    {
        return false;
    }

    RailBezierSegment2D targetSegment = map.FindSegment(targetSegmentId);

    if (targetSegment == null)
    {
        return false;
    }

    RailEndpoint2D enterFrom;

    if (targetSegment.startNodeId == ownerNodeId)
    {
        enterFrom = RailEndpoint2D.Start;
    }
    else if (targetSegment.endNodeId == ownerNodeId)
    {
        enterFrom = RailEndpoint2D.End;
    }
    else
    {
        return false;
    }

    exit = new RailExit2D
    {
        choice = choice,
        segmentId = targetSegmentId,
        enterFrom = enterFrom,
        priority = 0
    };

    return true;
}
```

---

## 10. 角色移动器 RailWalker2D

### 10.1 设计原则

`RailWalker2D` 不读取 Unity Input。

它只提供：

```text
TickMove(horizontalAxis, verticalAxis, deltaTime)
```

这样可以被以下输入来源复用：

```text
Unity 老 Input 系统
自定义 InputManager
New Input System
AI 控制
剧情自动移动
```

### 10.2 RailWalker2D.cs

```csharp
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
```

---

## 11. Character 控制器

### 11.1 OutsideDoorCharacterController.cs

该类使用 Unity 老 Input 系统读取输入，并把输入传给 `RailWalker2D`。

```csharp
using UnityEngine;

/// <summary>
/// OutsideDoor 场景角色控制器。
/// 它负责读取 Unity 老 Input 系统、更新朝向和动画，再把输入交给 RailWalker2D。
/// </summary>
[DisallowMultipleComponent]
public sealed class OutsideDoorCharacterController : MonoBehaviour
{
    [Header("Rail Movement")]

    /// <summary>
    /// 路径移动组件。
    /// 真正的位置移动由它处理。
    /// </summary>
    [SerializeField]
    private RailWalker2D railWalker;

    [Header("Old Input System")]

    /// <summary>
    /// Unity 老 Input 系统里的横向轴名称。
    /// 默认 Horizontal 通常对应 A/D 和左右方向键。
    /// </summary>
    [SerializeField]
    private string horizontalAxisName = "Horizontal";

    /// <summary>
    /// Unity 老 Input 系统里的纵向轴名称。
    /// 默认 Vertical 通常对应 W/S 和上下方向键。
    /// </summary>
    [SerializeField]
    private string verticalAxisName = "Vertical";

    [Header("Visual")]

    /// <summary>
    /// 角色 SpriteRenderer。
    /// 用于根据左右输入翻转角色朝向。
    /// </summary>
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// 是否根据左右输入翻转 SpriteRenderer.flipX。
    /// </summary>
    [SerializeField]
    private bool flipSpriteByMoveDirection = true;

    /// <summary>
    /// 角色 Animator。
    /// 如果暂时没有动画，可以留空。
    /// </summary>
    [SerializeField]
    private Animator animator;

    /// <summary>
    /// Animator 中用于表示移动速度的 Float 参数名。
    /// </summary>
    [SerializeField]
    private string animatorMoveSpeedParameter = "MoveSpeed";

    /// <summary>
    /// Animator 中用于表示是否移动的 Bool 参数名。
    /// </summary>
    [SerializeField]
    private string animatorIsMovingParameter = "IsMoving";

    /// <summary>
    /// 是否写入 Animator 参数。
    /// 如果 Animator 没有对应参数，应该关闭。
    /// </summary>
    [SerializeField]
    private bool updateAnimatorParameters = false;

    private float cachedHorizontalInput;
    private float cachedVerticalInput;
    private int facingSign = 1;

    /// <summary>
    /// 暴露给 Editor 工具使用。
    /// Editor 可以通过这个属性找到 RailWalker2D 并设置出生路径。
    /// </summary>
    public RailWalker2D Walker
    {
        get { return railWalker; }
    }

    private void Awake()
    {
        if (railWalker == null)
        {
            railWalker = GetComponent<RailWalker2D>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        // 如果项目里必须使用自定义 InputManager 上下文，可以在这里打开。
        // InputManager.instance.SetContext(InputContext.CHARACTER);
    }

    private void Update()
    {
        ReadOldInput();
        UpdateFacingByInput(cachedHorizontalInput);
        UpdateAnimatorByInput(cachedHorizontalInput);
    }

    private void FixedUpdate()
    {
        if (railWalker == null)
        {
            return;
        }

        railWalker.TickMove(
            cachedHorizontalInput,
            cachedVerticalInput,
            Time.fixedDeltaTime);
    }

    private void ReadOldInput()
    {
        cachedHorizontalInput = Input.GetAxisRaw(horizontalAxisName);
        cachedVerticalInput = Input.GetAxisRaw(verticalAxisName);
    }

    private void UpdateFacingByInput(float horizontalInput)
    {
        if (!flipSpriteByMoveDirection || spriteRenderer == null)
        {
            return;
        }

        if (horizontalInput > 0.01f)
        {
            facingSign = 1;
        }
        else if (horizontalInput < -0.01f)
        {
            facingSign = -1;
        }

        spriteRenderer.flipX = facingSign < 0;
    }

    private void UpdateAnimatorByInput(float horizontalInput)
    {
        if (!updateAnimatorParameters || animator == null)
        {
            return;
        }

        float moveAmount = Mathf.Abs(horizontalInput);
        bool isMoving = moveAmount > 0.01f;

        animator.SetFloat(animatorMoveSpeedParameter, moveAmount);
        animator.SetBool(animatorIsMovingParameter, isMoving);
    }
}
```

### 11.2 原 Character.cs 的处理方式

建议把原 `Character.cs` 的自由移动逻辑删除或停用。

推荐替换为：

```text
Character.cs
    只负责输入上下文和角色入口。

RailWalker2D
    负责路径移动。

OutsideDoorCharacterController
    负责读取输入和驱动 RailWalker2D。
```

如果保留 `Character.cs`，不要再在它的 `FixedUpdate` 中调用 `Rigidbody2D.MovePosition`。

---

## 12. Editor 工具设计

### 12.1 窗口入口

Unity 顶部菜单：

```text
Tools / COREBLAZER / Rail Bezier 2D Editor
```

### 12.2 工具模式

```text
Node 模式
    点击空白处创建 Node。
    点击已有 Node 选中 Node。

Segment 模式
    第一次点击 Node 选择起点。
    第二次点击 Node 选择终点。
    自动创建一条贝塞尔 Segment，并自动生成两个曲柄。

Edit 模式
    拖动 Node。
    点击 Segment 选中 Segment。
    拖动当前选中 Segment 的两个曲柄。
```

### 12.3 工具面板

窗口需要包含：

```text
Rail Map
Mode Toolbar
Bake All
Export Runtime Asset
Selected Node Exits
Selected Segment Settings
Character Binding
Validate
```

### 12.4 Character Binding 面板

字段：

```text
Target Character
Runtime Rail Map
Start Normalized
Set Selected Segment As Character Start
Snap Character To Start
```

按钮行为：

```text
1. 检查 targetCharacter 是否存在。
2. 检查 targetCharacter.Walker 是否存在。
3. 检查 targetRuntimeRailMap 是否存在。
4. 检查 selectedSegmentId 是否在 Runtime Rail Map 中存在。
5. 调用 walker.SetStartForEditorOrRuntime。
6. 标记对象 Dirty。
```

---

## 13. EditorWindow 核心实现要求

`RailBezier2DEditorWindow.cs` 必须放在 `Editor` 文件夹下。

原因：

```text
该脚本使用 UnityEditor 命名空间。
UnityEditor 只能在编辑器中使用，不能进入运行时构建。
```

### 13.1 需要实现的核心字段

```csharp
private RailBezierMap2DAuthoring map;
private ToolMode mode = ToolMode.Node;

private int pendingStartNodeId = -1;
private int selectedNodeId = -1;
private int selectedSegmentId = -1;

private OutsideDoorCharacterController targetCharacter;
private RailMap2DAsset targetRuntimeRailMap;
private float characterStartNormalizedPosition;
```

### 13.2 OnGUI 需要调用

```text
DrawMapSelector
DrawModeToolbar
DrawBakeAndExportButtons
DrawSelectedNodePanel
DrawSelectedSegmentPanel
DrawCharacterBindingPanel
DrawValidationPanel
```

### 13.3 DuringSceneGui 需要调用

```text
HandleUtility.AddDefaultControl
DrawSegments
DrawNodes
DrawSelectedSegmentHandles
DrawCharacterStartPreview
HandleSceneMouseInput
sceneView.Repaint
```

### 13.4 曲柄交互规则

```text
只显示当前 selectedSegmentId 的两个曲柄。
拖动曲柄时修改 startHandleOffset 或 endHandleOffset。
节点移动时不需要修改曲柄偏移，因为曲柄是相对节点的 offset。
```

### 13.5 Scene 坐标转换

```csharp
/// <summary>
/// 把 Scene 视图鼠标坐标转换为 2D 世界坐标。
/// </summary>
/// <param name="guiPosition">
/// 鼠标在 SceneView GUI 中的位置。
/// </param>
/// <param name="targetZ">
/// 目标 Z 平面。
/// 2D 路径一般放在同一个 Z 平面上。
/// </param>
/// <returns>
/// 返回二维世界坐标。
/// </returns>
private static Vector2 GetMouseWorld2D(
    Vector2 guiPosition,
    float targetZ)
{
    Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);

    float distance = Mathf.Approximately(ray.direction.z, 0f)
        ? 0f
        : (targetZ - ray.origin.z) / ray.direction.z;

    Vector3 world = ray.origin + ray.direction * distance;

    return new Vector2(world.x, world.y);
}
```

---

## 14. Validate 校验规则

Editor 工具需要一个 `Validate Map` 按钮。

校验内容：

```text
1. nodeId 不能重复。
2. segmentId 不能重复。
3. 每条 Segment 必须能找到 startNode。
4. 每条 Segment 必须能找到 endNode。
5. 每条 Segment 的 sampleCount 必须大于等于 4。
6. 每条 Segment 烘焙后 bakedPoints 长度必须大于等于 2。
7. 每个出口指向的 segmentId 必须存在。
8. 每个出口指向的 Segment 必须和当前 Node 相连。
9. Runtime RailMap 中的 selectedSegmentId 必须存在，才能设置角色出生点。
```

校验结果可以用 `EditorUtility.DisplayDialog` 显示，也可以在窗口里用 `HelpBox` 显示。

---

## 15. Codex 实现任务清单

### 15.1 第一阶段：运行时数据

创建文件：

```text
Assets/Scripts/OutsideDoorScene/OutsideManager/Path/Runtime/RailRuntimeTypes2D.cs
Assets/Scripts/OutsideDoorScene/OutsideManager/Path/Runtime/RailMap2DAsset.cs
```

完成：

```text
RailExitChoice2D
RailEndpoint2D
RailNode2D
RailExit2D
RailSegment2D
RailMap2DAsset
```

验收：

```text
Unity 编译通过。
RailMap2DAsset 可以通过 CreateAssetMenu 创建。
RailSegment2D.RebuildLengthTable 可正确生成累计长度。
RailSegment2D.GetPointByDistance 可返回插值点。
```

### 15.2 第二阶段：编辑期数据

创建文件：

```text
Assets/Scripts/OutsideDoorScene/OutsideManager/Path/Authoring/RailBezierNode2D.cs
Assets/Scripts/OutsideDoorScene/OutsideManager/Path/Authoring/RailBezierSegment2D.cs
Assets/Scripts/OutsideDoorScene/OutsideManager/Path/Authoring/RailBezierMap2DAuthoring.cs
```

完成：

```text
节点创建
路径段创建
曲柄默认生成
贝塞尔烘焙
Reverse Segment
Reset Handles
```

验收：

```text
在场景中挂 RailBezierMap2DAuthoring 后，可以通过代码创建节点和 Segment。
BakeAll 后每条 Segment 都有 bakedPoints。
```

### 15.3 第三阶段：Editor 工具

创建文件：

```text
Assets/Scripts/OutsideDoorScene/OutsideManager/Path/Editor/RailBezier2DEditorWindow.cs
```

完成：

```text
菜单入口
Node 模式
Segment 模式
Edit 模式
曲线绘制
节点拖动
曲柄拖动
Bake All
Export Runtime Asset
Validate Map
Character Binding
角色出生点预览
```

验收：

```text
Scene 视图点击可以创建节点。
点击两个节点可以创建曲线。
选中曲线可以拖动曲柄。
Bake 后可以生成路径点。
Export 后可以生成 RailMap2DAsset。
```

### 15.4 第四阶段：角色移动

创建文件：

```text
Assets/Scripts/OutsideDoorScene/OutsideManager/Path/Runtime/RailWalker2D.cs
Assets/Scripts/OutsideDoorScene/OutsideManager/Manager/Character/OutsideDoorCharacterController.cs
```

完成：

```text
RailWalker2D.TickMove
Horizontal -> distanceOnSegment
Vertical -> branch input buffer
Node endpoint switch
Rigidbody2D.MovePosition
SpriteRenderer flipX
Animator 参数可选写入
```

验收：

```text
角色按左右沿曲线移动。
角色不会因上下输入直接移动。
角色在分支点按下可以进入下方路线。
角色在分支点不按下时可以继续走 Auto 或 Right 路线。
无出口时角色停在节点。
```

### 15.5 第五阶段：替换原 Character.cs 自由移动

处理方式：

```text
删除或停用原 Character.cs 中的 FixedUpdate 自由移动逻辑。
给 Player 添加 OutsideDoorCharacterController。
给 Player 添加 RailWalker2D。
保留 Rigidbody2D。
Rigidbody2D 设置为 Kinematic。
```

验收：

```text
项目不存在两个脚本同时移动角色的问题。
```

---

## 16. 手动测试用例

### 16.1 基础路径测试

场景配置：

```text
Node_1 -> Segment_1 -> Node_2
```

操作：

```text
按 Right
```

预期：

```text
角色沿 Segment_1 从 Node_1 走向 Node_2。
```

操作：

```text
按 Left
```

预期：

```text
角色沿 Segment_1 从 Node_2 返回 Node_1。
```

### 16.2 分支测试

场景配置：

```text
Node_1 -> Segment_1 -> Node_Fork
Node_Fork -> Segment_2 -> Node_House
Node_Fork -> Segment_3 -> Node_Lower
```

出口配置：

```text
Node_Fork.Right = Segment_2
Node_Fork.Down = Segment_3
Node_Fork.Auto = Segment_2
```

操作：

```text
角色走到 Node_Fork 前按 Down。
```

预期：

```text
角色进入 Segment_3。
```

操作：

```text
角色走到 Node_Fork，不按 Down，继续按 Right。
```

预期：

```text
角色进入 Segment_2。
```

### 16.3 无出口测试

场景配置：

```text
Node_End 没有出口。
```

操作：

```text
角色走到 Node_End 后继续按 Right。
```

预期：

```text
角色停在 Node_End，不报错。
```

### 16.4 反向进入测试

场景配置：

```text
Node_Fork.Left = Segment_1
Segment_1.endNodeId = Node_Fork
```

操作：

```text
角色在 Node_Fork 按 Left。
```

预期：

```text
角色从 Segment_1 的 End 端进入，distanceOnSegment 从 Segment_1.Length 开始减少。
```

### 16.5 Editor 绑定测试

操作：

```text
选中 Segment_1
Target Character = Player
Runtime Rail Map = Export 出来的资产
Start Normalized = 0.5
点击 Set Selected Segment As Character Start
```

预期：

```text
Player 吸附到 Segment_1 中点附近。
运行游戏后从该点开始移动。
```

---

## 17. 注意事项

### 17.1 路径方向规则

建议统一规定：

```text
bakedPoints[0] 是逻辑左端
bakedPoints[last] 是逻辑右端
```

这样运行时代码可以保持简单：

```text
按右：distanceOnSegment 增加
按左：distanceOnSegment 减少
```

如果曲线画反了，使用 Editor 的 `Reverse Segment`。

### 17.2 不要把曲柄做成 Node

曲柄只控制曲线形状，不参与分支连接。

如果曲柄也是 Node，会导致拓扑和形状混在一起，后期维护困难。

### 17.3 上下输入只做分支选择

不要让 `Vertical` 改变角色坐标。

如果未来需要梯子、跳跃、进入门等，也应该通过节点交互或状态切换处理。

### 17.4 Editor 代码不能进 Runtime

所有使用 `UnityEditor` 的代码必须放在：

```text
Editor 文件夹
```

否则打包会失败。

### 17.5 避免双重移动

如果启用 `RailWalker2D`，原自由移动脚本必须停用。

否则会出现：

```text
RailWalker2D 设置位置
原 Character.cs 又设置位置
```

角色会抖动或偏离路径。

---

## 18. 最终完成标准

Codex 完成后，应达到：

```text
1. Unity 编译无错误。
2. Tools 菜单中能打开 Rail Bezier 2D Editor。
3. 能在 Scene 视图点击创建节点。
4. 能点击两个节点创建曲线。
5. 能拖动曲柄调整曲线。
6. 能配置分支点出口。
7. 能 Bake 和 Export RailMap2DAsset。
8. 能把选中 Segment 设置为 Player 出生路径。
9. Play 模式下角色能用老 Input 系统左右沿曲线移动。
10. 上下输入只在分支点选择路线。
11. 无出口时角色停在节点。
12. 不再使用原 Character.cs 的四向自由移动。
```
