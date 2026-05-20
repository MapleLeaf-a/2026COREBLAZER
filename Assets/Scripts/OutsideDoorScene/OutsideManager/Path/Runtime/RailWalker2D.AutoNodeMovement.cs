using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RailWalker2D 的自动节点移动扩展。
///
/// 作用：
/// 1. 根据目标 nodeKey 查找目标 RailNode2D。
/// 2. 根据当前 Segment 和目标 Node 自动计算路径。
/// 3. 沿 RailMap2DAsset 中烘焙好的 Segment 自动移动。
/// </summary>
public sealed partial class RailWalker2D : MonoBehaviour
{
	[Header("Auto Node Movement")]

	/// <summary>
	/// 是否启用自动节点移动。
	/// false 时，外部点击请求会被拒绝。
	/// </summary>
	[SerializeField]
	private bool autoNodeMovementEnabled = true;

	/// <summary>
	/// 自动移动到节点时的到达误差。
	///
	/// 作用：
	/// 当 distanceOnSegment 与目标距离的差值小于这个值时，
	/// 视为已经到达当前路径段的目标端点。
	/// </summary>
	[SerializeField]
	[Min(0f)]
	private float autoNodeArriveEpsilon = 0.03f;

	/// <summary>
	/// 是否打印自动寻路日志。
	/// 开发期建议打开，方便排查 nodeKey 或 Segment 配置问题。
	/// </summary>
	[SerializeField]
	private bool logAutoNodeMovementDebug = true;

	/// <summary>
	/// 自动节点移动结束事件。
	///
	/// 参数 1 (bool)：
	/// true 表示正常到达目标节点。
	/// false 表示被取消或失败。
	///
	/// 参数 2 (int)：
	/// 本次自动移动的目标节点 ID。
	/// </summary>
	public event System.Action<bool, int> AutoNodeMovementFinished;

	/// <summary>
	/// 当前是否正在自动移动到目标节点。
	/// </summary>
	public bool IsAutoNodeMoving
	{
		get { return isAutoNodeMoving; }
	}

	/// <summary>
	/// 自动移动时的横向视觉输入。
	///
	/// 作用：
	/// OutsideDoorCharacterController 可以用它更新角色朝向和移动动画。
	/// </summary>
	public float CurrentAutoMoveHorizontalAxis
	{
		get;
		private set;
	}

	/// <summary>
	/// 一段自动移动步骤。
	///
	/// 一次完整自动移动可能跨多个 Segment。
	/// 每个 AutoRailMoveLeg2D 只负责在一个 Segment 上从 startDistance 走到 targetDistance。
	/// </summary>
	private struct AutoRailMoveLeg2D
	{
		/// <summary>
		/// 本段移动所在的 Segment ID。
		/// </summary>
		public int segmentId;

		/// <summary>
		/// 本段移动开始时，在 Segment 上的路径距离。
		/// </summary>
		public float startDistance;

		/// <summary>
		/// 本段移动结束时，在 Segment 上的路径距离。
		/// </summary>
		public float targetDistance;

		/// <summary>
		/// 本段移动最终到达的 Node ID。
		/// </summary>
		public int targetNodeId;

		/// <summary>
		/// 创建自动移动步骤。
		/// </summary>
		/// <param name="segmentId">
		/// segmentId：
		/// 本段移动使用的路径段 ID。
		/// </param>
		/// <param name="startDistance">
		/// startDistance：
		/// 本段移动开始时，角色在该路径段上的距离。
		/// </param>
		/// <param name="targetDistance">
		/// targetDistance：
		/// 本段移动结束时，角色在该路径段上的距离。
		/// </param>
		/// <param name="targetNodeId">
		/// targetNodeId：
		/// 本段移动结束时到达的节点 ID。
		/// </param>
		public AutoRailMoveLeg2D(
			int segmentId,
			float startDistance,
			float targetDistance,
			int targetNodeId)
		{
			this.segmentId = segmentId;
			this.startDistance = startDistance;
			this.targetDistance = targetDistance;
			this.targetNodeId = targetNodeId;
		}
	}

	private readonly List<AutoRailMoveLeg2D> autoMoveLegs = new List<AutoRailMoveLeg2D>();
	private readonly List<int> autoMoveScratchPathA = new List<int>();
	private readonly List<int> autoMoveScratchPathB = new List<int>();

	private bool isAutoNodeMoving;
	private int autoMoveLegIndex = -1;
	private int autoMoveTargetNodeId = -1;

	/// <summary>
	/// 尝试自动移动到指定 nodeKey 对应的节点。
	/// </summary>
	/// <param name="targetNodeKey">
	/// targetNodeKey：
	/// 目标节点查询名。
	/// 这个值必须能在当前 railMap.nodes 中匹配到 RailNode2D.nodeKey。
	/// </param>
	/// <returns>
	/// true 表示成功开始自动移动，或者角色已经在目标节点附近。
	/// false 表示目标节点不存在、当前路径无效，或者无法找到连通路径。
	/// </returns>
	public bool TryAutoMoveToNodeKey(string targetNodeKey)
	{
		if (!autoNodeMovementEnabled)
		{
			LogAutoNodeMovementWarning("自动节点移动已关闭。");
			return false;
		}

		if (railMap == null)
		{
			LogAutoNodeMovementWarning("RailMap2DAsset 为空，无法根据 nodeKey 自动移动。请确保 RailWalker2D 的 RailMap 已设置。");
			return false;
		}

		if (currentSegmentId < 0)
		{
			LogAutoNodeMovementWarning($"currentSegmentId 为 {currentSegmentId}，RailWalker2D 尚未初始化。");
			return false;
		}

		if (string.IsNullOrWhiteSpace(targetNodeKey))
		{
			LogAutoNodeMovementWarning("目标 nodeKey 为空，无法自动移动。");
			return false;
		}

		if (!railMap.TryGetNodeByKey(targetNodeKey, out RailNode2D targetNode))
		{
			LogAutoNodeMovementWarning($"找不到目标节点：{targetNodeKey}");
			return false;
		}

		return TryAutoMoveToNode(targetNode.nodeId);
	}

	/// <summary>
	/// 取消当前自动节点移动。
	/// </summary>
	public void CancelAutoNodeMovement()
	{
		StopAutoNodeMovement(false, false, true);
	}

	/// <summary>
	/// 尝试自动移动到指定节点 ID。
	/// </summary>
	/// <param name="targetNodeId">
	/// targetNodeId：
	/// 目标 RailNode2D.nodeId。
	/// 该 ID 来自 RailMap2DAsset.nodes。
	/// </param>
	/// <returns>
	/// true 表示成功开始自动移动，或者角色已经在目标节点附近。
	/// false 表示无法构建路径。
	/// </returns>
	private bool TryAutoMoveToNode(int targetNodeId)
	{
		// 新点击请求覆盖旧自动移动时，静默停止旧移动，避免旧失败事件干扰新交互。
		if (isAutoNodeMoving)
		{
			StopAutoNodeMovement(false, false, false);
		}

		autoMoveTargetNodeId = targetNodeId;

		if (!TryBuildAutoMoveLegs(targetNodeId, autoMoveLegs))
		{
			LogAutoNodeMovementWarning($"无法构建自动移动路径。targetNodeId={targetNodeId}");
			return false;
		}

		if (autoMoveLegs.Count == 0)
		{
			SnapToCurrentSegment();
			LogAutoNodeMovement($"角色已经位于目标节点附近。targetNodeId={targetNodeId}");

			// 不需要移动也视为已经到达，方便点击脚本继续检查碰撞重合。
			AutoNodeMovementFinished?.Invoke(true, targetNodeId);

			autoMoveTargetNodeId = -1;
			return true;
		}

		isAutoNodeMoving = true;
		currentMoveSpeed = 0f;

		BeginAutoMoveLeg(0);

		LogAutoNodeMovement(
			$"开始自动移动。targetNodeId={targetNodeId}, legCount={autoMoveLegs.Count}");

		return true;
	}

	/// <summary>
	/// 每个 FixedUpdate 中推进自动节点移动。
	/// </summary>
	/// <param name="deltaTime">
	/// deltaTime：
	/// 当前物理帧耗时。
	/// 通常由 RailWalker2D.TickMove 传入 Time.fixedDeltaTime。
	/// </param>
	private void TickAutoNodeMovement(float deltaTime)
	{
		if (!isAutoNodeMoving)
		{
			return;
		}

		if (deltaTime <= 0f)
		{
			return;
		}

		if (autoMoveLegIndex < 0 || autoMoveLegIndex >= autoMoveLegs.Count)
		{
			StopAutoNodeMovement(true, true, true);
			return;
		}

		AutoRailMoveLeg2D leg = autoMoveLegs[autoMoveLegIndex];

		if (!railMap.TryGetSegment(leg.segmentId, out RailSegment2D segment))
		{
			LogAutoNodeMovementWarning($"自动移动失败，Segment 不存在。segmentId={leg.segmentId}");
			StopAutoNodeMovement(false, false, true);
			return;
		}

		EnsureSegmentLengthTable(segment);

		if (!IsSegmentUsable(segment))
		{
			LogAutoNodeMovementWarning($"自动移动失败，Segment 不可用。segmentId={leg.segmentId}");
			StopAutoNodeMovement(false, false, true);
			return;
		}

		int distanceMoveSign = leg.targetDistance >= distanceOnSegment ? 1 : -1;
		float frameMoveDistance = CalculateFrameMoveDistance(deltaTime);

		distanceOnSegment += distanceMoveSign * frameMoveDistance;

		UpdateAutoMoveVisualAxis(segment, leg);

		if (HasReachedAutoMoveLegTarget(leg, distanceMoveSign))
		{
			distanceOnSegment = leg.targetDistance;
			SnapToCurrentSegment();
			AdvanceAutoMoveLeg();
			return;
		}

		if (useVectorTargetMovement)
		{
			MoveTowardsCurrentRailTarget(frameMoveDistance, distanceMoveSign);
			return;
		}

		SnapToCurrentSegment();
	}

	/// <summary>
	/// 构建自动移动步骤列表。
	/// </summary>
	/// <param name="targetNodeId">
	/// targetNodeId：
	/// 目标节点 ID。
	/// </param>
	/// <param name="result">
	/// result：
	/// 输出的自动移动步骤列表。
	/// 方法内部会先清空它，再写入新的路径步骤。
	/// </param>
	/// <returns>
	/// true 表示路径构建成功。
	/// false 表示当前 Segment 无效，或者目标节点不可达。
	/// </returns>
	private bool TryBuildAutoMoveLegs(
		int targetNodeId,
		List<AutoRailMoveLeg2D> result)
	{
		result.Clear();

		if (railMap == null)
		{
			return false;
		}

		if (!railMap.TryGetSegment(currentSegmentId, out RailSegment2D currentSegment))
		{
			return false;
		}

		EnsureSegmentLengthTable(currentSegment);

		if (!IsSegmentUsable(currentSegment))
		{
			return false;
		}

		distanceOnSegment = Mathf.Clamp(distanceOnSegment, 0f, currentSegment.Length);

		bool hasStartPath = TryFindShortestNodePath(
			currentSegment.startNodeId,
			targetNodeId,
			autoMoveScratchPathA,
			out float startPathCost);

		bool hasEndPath = TryFindShortestNodePath(
			currentSegment.endNodeId,
			targetNodeId,
			autoMoveScratchPathB,
			out float endPathCost);

		if (!hasStartPath && !hasEndPath)
		{
			return false;
		}

		float totalCostViaStart = hasStartPath
			? distanceOnSegment + startPathCost
			: float.PositiveInfinity;

		float totalCostViaEnd = hasEndPath
			? currentSegment.Length - distanceOnSegment + endPathCost
			: float.PositiveInfinity;

		bool useStartEndpoint = totalCostViaStart <= totalCostViaEnd;

		int chosenEndpointNodeId = useStartEndpoint
			? currentSegment.startNodeId
			: currentSegment.endNodeId;

		float chosenEndpointDistance = useStartEndpoint
			? 0f
			: currentSegment.Length;

		if (Mathf.Abs(distanceOnSegment - chosenEndpointDistance) > autoNodeArriveEpsilon)
		{
			result.Add(new AutoRailMoveLeg2D(
				currentSegment.segmentId,
				distanceOnSegment,
				chosenEndpointDistance,
				chosenEndpointNodeId));
		}
		else
		{
			distanceOnSegment = chosenEndpointDistance;
		}

		List<int> chosenNodePath = useStartEndpoint
			? autoMoveScratchPathA
			: autoMoveScratchPathB;

		return TryAppendPathLegs(chosenNodePath, result);
	}

	/// <summary>
	/// 使用简单 Dijkstra 算法查找节点最短路径。
	/// </summary>
	/// <param name="startNodeId">
	/// startNodeId：
	/// 路径起点节点 ID。
	/// </param>
	/// <param name="targetNodeId">
	/// targetNodeId：
	/// 路径终点节点 ID。
	/// </param>
	/// <param name="nodePath">
	/// nodePath：
	/// 输出的节点路径。
	/// 成功时包含 startNodeId 和 targetNodeId。
	/// </param>
	/// <param name="pathCost">
	/// pathCost：
	/// 输出路径总长度。
	/// 这里使用每个 Segment 的 Length 作为路径成本。
	/// </param>
	/// <returns>
	/// true 表示找到路径。
	/// false 表示目标节点不可达。
	/// </returns>
	private bool TryFindShortestNodePath(
		int startNodeId,
		int targetNodeId,
		List<int> nodePath,
		out float pathCost)
	{
		nodePath.Clear();
		pathCost = 0f;

		if (startNodeId == targetNodeId)
		{
			nodePath.Add(startNodeId);
			return true;
		}

		Dictionary<int, float> distanceByNode = new Dictionary<int, float>();
		Dictionary<int, int> previousByNode = new Dictionary<int, int>();
		HashSet<int> visitedNodes = new HashSet<int>();

		distanceByNode[startNodeId] = 0f;

		while (TrySelectUnvisitedNodeWithSmallestDistance(
				   distanceByNode,
				   visitedNodes,
				   out int currentNodeId))
		{
			if (currentNodeId == targetNodeId)
			{
				break;
			}

			visitedNodes.Add(currentNodeId);

			RelaxConnectedSegments(
				currentNodeId,
				distanceByNode,
				previousByNode,
				visitedNodes);
		}

		if (!distanceByNode.TryGetValue(targetNodeId, out pathCost))
		{
			return false;
		}

		return TryRebuildNodePath(
			startNodeId,
			targetNodeId,
			previousByNode,
			nodePath);
	}

	/// <summary>
	/// 从未访问节点中选择当前距离最小的节点。
	/// </summary>
	/// <param name="distanceByNode">
	/// distanceByNode：
	/// 节点 ID 到当前最短距离的映射表。
	/// </param>
	/// <param name="visitedNodes">
	/// visitedNodes：
	/// 已经处理过的节点集合。
	/// </param>
	/// <param name="nodeId">
	/// nodeId：
	/// 输出找到的节点 ID。
	/// </param>
	/// <returns>
	/// true 表示找到一个可继续扩展的节点。
	/// false 表示没有可继续扩展的节点。
	/// </returns>
	private static bool TrySelectUnvisitedNodeWithSmallestDistance(
		Dictionary<int, float> distanceByNode,
		HashSet<int> visitedNodes,
		out int nodeId)
	{
		nodeId = -1;
		float bestDistance = float.PositiveInfinity;

		foreach (KeyValuePair<int, float> pair in distanceByNode)
		{
			if (visitedNodes.Contains(pair.Key))
			{
				continue;
			}

			if (pair.Value >= bestDistance)
			{
				continue;
			}

			bestDistance = pair.Value;
			nodeId = pair.Key;
		}

		return nodeId >= 0;
	}

	/// <summary>
	/// 扩展当前节点连接的所有 Segment。
	/// </summary>
	/// <param name="currentNodeId">
	/// currentNodeId：
	/// 当前正在扩展的节点 ID。
	/// </param>
	/// <param name="distanceByNode">
	/// distanceByNode：
	/// 节点 ID 到当前最短距离的映射表。
	/// </param>
	/// <param name="previousByNode">
	/// previousByNode：
	/// 节点 ID 到路径上一个节点 ID 的映射表。
	/// </param>
	/// <param name="visitedNodes">
	/// visitedNodes：
	/// 已经处理过的节点集合。
	/// </param>
	private void RelaxConnectedSegments(
		int currentNodeId,
		Dictionary<int, float> distanceByNode,
		Dictionary<int, int> previousByNode,
		HashSet<int> visitedNodes)
	{
		if (railMap == null || railMap.segments == null)
		{
			return;
		}

		float currentDistance = distanceByNode[currentNodeId];

		for (int i = 0; i < railMap.segments.Count; i++)
		{
			RailSegment2D segment = railMap.segments[i];

			if (!IsSegmentUsable(segment))
			{
				continue;
			}

			int nextNodeId;

			if (segment.startNodeId == currentNodeId)
			{
				nextNodeId = segment.endNodeId;
			}
			else if (segment.endNodeId == currentNodeId)
			{
				nextNodeId = segment.startNodeId;
			}
			else
			{
				continue;
			}

			if (visitedNodes.Contains(nextNodeId))
			{
				continue;
			}

			float nextDistance = currentDistance + segment.Length;

			if (distanceByNode.TryGetValue(nextNodeId, out float oldDistance) &&
				nextDistance >= oldDistance)
			{
				continue;
			}

			distanceByNode[nextNodeId] = nextDistance;
			previousByNode[nextNodeId] = currentNodeId;
		}
	}

	/// <summary>
	/// 根据 previousByNode 反推出完整节点路径。
	/// </summary>
	/// <param name="startNodeId">
	/// startNodeId：
	/// 路径起点节点 ID。
	/// </param>
	/// <param name="targetNodeId">
	/// targetNodeId：
	/// 路径终点节点 ID。
	/// </param>
	/// <param name="previousByNode">
	/// previousByNode：
	/// 节点 ID 到路径上一个节点 ID 的映射表。
	/// </param>
	/// <param name="nodePath">
	/// nodePath：
	/// 输出的正向节点路径。
	/// </param>
	/// <returns>
	/// true 表示路径重建成功。
	/// false 表示 previousByNode 数据不完整。
	/// </returns>
	private static bool TryRebuildNodePath(
		int startNodeId,
		int targetNodeId,
		Dictionary<int, int> previousByNode,
		List<int> nodePath)
	{
		List<int> reversedPath = new List<int>();
		int currentNodeId = targetNodeId;

		reversedPath.Add(currentNodeId);

		while (currentNodeId != startNodeId)
		{
			if (!previousByNode.TryGetValue(currentNodeId, out int previousNodeId))
			{
				return false;
			}

			currentNodeId = previousNodeId;
			reversedPath.Add(currentNodeId);
		}

		nodePath.Clear();

		for (int i = reversedPath.Count - 1; i >= 0; i--)
		{
			nodePath.Add(reversedPath[i]);
		}

		return true;
	}

	/// <summary>
	/// 把节点路径转换成 Segment 移动步骤。
	/// </summary>
	/// <param name="nodePath">
	/// nodePath：
	/// 节点路径。
	/// 相邻两个节点之间必须存在一个可用 Segment。
	/// </param>
	/// <param name="result">
	/// result：
	/// 输出的自动移动步骤列表。
	/// </param>
	/// <returns>
	/// true 表示转换成功。
	/// false 表示某两个相邻节点之间没有可用 Segment。
	/// </returns>
	private bool TryAppendPathLegs(
		List<int> nodePath,
		List<AutoRailMoveLeg2D> result)
	{
		for (int i = 1; i < nodePath.Count; i++)
		{
			int fromNodeId = nodePath[i - 1];
			int toNodeId = nodePath[i];

			if (!TryFindShortestSegmentBetweenNodes(
					fromNodeId,
					toNodeId,
					out RailSegment2D segment))
			{
				return false;
			}

			bool moveFromStartToEnd = segment.startNodeId == fromNodeId;

			float startDistance = moveFromStartToEnd
				? 0f
				: segment.Length;

			float targetDistance = moveFromStartToEnd
				? segment.Length
				: 0f;

			result.Add(new AutoRailMoveLeg2D(
				segment.segmentId,
				startDistance,
				targetDistance,
				toNodeId));
		}

		return true;
	}

	/// <summary>
	/// 查找连接两个节点的最短 Segment。
	/// </summary>
	/// <param name="fromNodeId">
	/// fromNodeId：
	/// 起点节点 ID。
	/// </param>
	/// <param name="toNodeId">
	/// toNodeId：
	/// 终点节点 ID。
	/// </param>
	/// <param name="segment">
	/// segment：
	/// 输出找到的最短 Segment。
	/// </param>
	/// <returns>
	/// true 表示找到可用 Segment。
	/// false 表示两个节点之间没有直接连接。
	/// </returns>
	private bool TryFindShortestSegmentBetweenNodes(
		int fromNodeId,
		int toNodeId,
		out RailSegment2D segment)
	{
		segment = null;

		if (railMap == null || railMap.segments == null)
		{
			return false;
		}

		float bestLength = float.PositiveInfinity;

		for (int i = 0; i < railMap.segments.Count; i++)
		{
			RailSegment2D candidate = railMap.segments[i];

			if (!IsSegmentUsable(candidate))
			{
				continue;
			}

			bool connectedForward =
				candidate.startNodeId == fromNodeId &&
				candidate.endNodeId == toNodeId;

			bool connectedBackward =
				candidate.startNodeId == toNodeId &&
				candidate.endNodeId == fromNodeId;

			if (!connectedForward && !connectedBackward)
			{
				continue;
			}

			if (candidate.Length >= bestLength)
			{
				continue;
			}

			bestLength = candidate.Length;
			segment = candidate;
		}

		return segment != null;
	}

	/// <summary>
	/// 开始执行指定下标的自动移动步骤。
	/// </summary>
	/// <param name="legIndex">
	/// legIndex：
	/// autoMoveLegs 中的步骤下标。
	/// </param>
	private void BeginAutoMoveLeg(int legIndex)
	{
		autoMoveLegIndex = legIndex;

		AutoRailMoveLeg2D leg = autoMoveLegs[autoMoveLegIndex];

		currentSegmentId = leg.segmentId;
		distanceOnSegment = leg.startDistance;

		SnapToCurrentSegment();

		if (railMap.TryGetSegment(leg.segmentId, out RailSegment2D segment))
		{
			UpdateAutoMoveVisualAxis(segment, leg);
		}
	}

	/// <summary>
	/// 推进到下一段自动移动步骤。
	/// </summary>
	private void AdvanceAutoMoveLeg()
	{
		int nextLegIndex = autoMoveLegIndex + 1;

		if (nextLegIndex >= autoMoveLegs.Count)
		{
			StopAutoNodeMovement(true, false, true);
			return;
		}

		BeginAutoMoveLeg(nextLegIndex);
	}

	/// <summary>
	/// 判断当前自动移动步骤是否已经到达目标距离。
	/// </summary>
	/// <param name="leg">
	/// leg：
	/// 当前正在执行的自动移动步骤。
	/// </param>
	/// <param name="distanceMoveSign">
	/// distanceMoveSign：
	/// 路径距离移动方向。
	/// 1 表示 distanceOnSegment 增大。
	/// -1 表示 distanceOnSegment 减小。
	/// </param>
	/// <returns>
	/// true 表示已经到达当前步骤目标距离。
	/// false 表示还需要继续移动。
	/// </returns>
	private bool HasReachedAutoMoveLegTarget(
		AutoRailMoveLeg2D leg,
		int distanceMoveSign)
	{
		if (distanceMoveSign > 0)
		{
			return distanceOnSegment >= leg.targetDistance - autoNodeArriveEpsilon;
		}

		return distanceOnSegment <= leg.targetDistance + autoNodeArriveEpsilon;
	}

	/// <summary>
	/// 停止自动节点移动。
	/// </summary>
	/// <param name="reachedTarget">
	/// reachedTarget：
	/// true 表示正常到达目标。
	/// false 表示中途取消或失败。
	/// </param>
	/// <param name="snapToCurrentRail">
	/// snapToCurrentRail：
	/// true 表示停止时把角色吸附到当前 Segment 对应位置。
	/// false 表示不额外吸附。
	/// </param>
	/// <param name="notify">
	/// notify：
	/// true 表示触发 AutoNodeMovementFinished 事件。
	/// false 表示静默停止，不触发事件。
	/// 新请求覆盖旧请求时应传 false，避免旧事件干扰新交互。
	/// </param>
	private void StopAutoNodeMovement(
		bool reachedTarget,
		bool snapToCurrentRail,
		bool notify)
	{
		int finishedTargetNodeId = autoMoveTargetNodeId;

		isAutoNodeMoving = false;
		autoMoveLegIndex = -1;
		autoMoveTargetNodeId = -1;
		CurrentAutoMoveHorizontalAxis = 0f;
		currentMoveSpeed = 0f;
		autoMoveLegs.Clear();

		if (snapToCurrentRail)
		{
			SnapToCurrentSegment();
		}

		if (notify)
		{
			AutoNodeMovementFinished?.Invoke(reachedTarget, finishedTargetNodeId);
		}

		if (reachedTarget)
		{
			LogAutoNodeMovement("自动移动结束，已到达目标节点。");
		}
	}

	/// <summary>
	/// 更新自动移动时的横向视觉输入。
	/// </summary>
	/// <param name="segment">
	/// segment：
	/// 当前正在移动的路径段。
	/// </param>
	/// <param name="leg">
	/// leg：
	/// 当前正在执行的自动移动步骤。
	/// </param>
	private void UpdateAutoMoveVisualAxis(
		RailSegment2D segment,
		AutoRailMoveLeg2D leg)
	{
		if (segment == null)
		{
			CurrentAutoMoveHorizontalAxis = 0f;
			return;
		}

		Vector2 currentPosition = segment.GetPointByDistance(distanceOnSegment);
		Vector2 targetPosition = segment.GetPointByDistance(leg.targetDistance);

		float deltaX = targetPosition.x - currentPosition.x;

		if (Mathf.Abs(deltaX) <= 0.01f)
		{
			CurrentAutoMoveHorizontalAxis = 0f;
			return;
		}

		CurrentAutoMoveHorizontalAxis = deltaX > 0f ? 1f : -1f;
	}

	/// <summary>
	/// 打印自动移动普通日志。
	/// </summary>
	/// <param name="message">
	/// message：
	/// 需要输出的日志内容。
	/// </param>
	private void LogAutoNodeMovement(string message)
	{
		if (!logAutoNodeMovementDebug)
		{
			return;
		}

		Debug.Log($"[RailWalker2D AutoMove] {message}", this);
	}

	/// <summary>
	/// 打印自动移动警告日志。
	/// </summary>
	/// <param name="message">
	/// message：
	/// 需要输出的警告内容。
	/// </param>
	private void LogAutoNodeMovementWarning(string message)
	{
		if (!logAutoNodeMovementDebug)
		{
			return;
		}

		Debug.LogWarning($"[RailWalker2D AutoMove] {message}", this);
	}
}
