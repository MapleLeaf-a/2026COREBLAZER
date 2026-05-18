using UnityEngine;

/// <summary>
/// 2D 路径移动器。
/// 它根据 RailMap2DAsset 的烘焙路径点移动角色。
/// </summary>
[DisallowMultipleComponent]
public sealed class RailWalker2D : MonoBehaviour
{
	[Header("Rail Data")]
	[SerializeField]
	private RailMap2DAsset railMap;

	[SerializeField]
	private int currentSegmentId = -1;

	[SerializeField]
	[Range(0f, 1f)]
	private float normalizedStartPosition = 0f;

	[Header("Movement")]
	[SerializeField]
	[Min(0f)]
	private float moveSpeed = 4f;

	[SerializeField]
	[Range(0f, 1f)]
	private float horizontalDeadZone = 0.1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float verticalDeadZone = 0.5f;

	[SerializeField]
	[Min(0f)]
	private float branchInputBufferTime = 0.15f;

	[SerializeField]
	[Min(0f)]
	private float nodeArriveEpsilon = 0.02f;

	[SerializeField]
	private bool autoMatchInputToWorldX = true;

	[SerializeField]
	private bool logRailDebug = true;

	[SerializeField]
	private bool autoContinueThroughConnectedSegments = true;

	[Header("Vector Target Movement")]
	[SerializeField]
	private bool useVectorTargetMovement = true;

	[SerializeField]
	[Min(0f)]
	private float targetLeadDistance = 0.15f;

	[SerializeField]
	[Min(0f)]
	private float targetArriveEpsilon = 0.01f;

	[SerializeField]
	[Min(0f)]
	private float acceleration = 20f;

	[Header("Physics")]
	[SerializeField]
	private Rigidbody2D rb;

	[SerializeField]
	private float distanceOnSegment;

	private RailExitChoice2D bufferedVerticalChoice = RailExitChoice2D.None;
	private float bufferedVerticalTimer;
	private float currentMoveSpeed;

	public RailMap2DAsset RailMap => railMap;
	public int CurrentSegmentId => currentSegmentId;
	public float DistanceOnSegment => distanceOnSegment;

	public float MoveSpeed
	{
		get { return moveSpeed; }
	}

	public void SetMoveSpeed(float newMoveSpeed)
	{
		moveSpeed = Mathf.Max(0f, newMoveSpeed);
	}

	private void OnValidate()
	{
		moveSpeed = Mathf.Max(0f, moveSpeed);
		branchInputBufferTime = Mathf.Max(0f, branchInputBufferTime);
		nodeArriveEpsilon = Mathf.Max(0f, nodeArriveEpsilon);
		targetLeadDistance = Mathf.Max(0f, targetLeadDistance);
		targetArriveEpsilon = Mathf.Max(0f, targetArriveEpsilon);
		acceleration = Mathf.Max(0f, acceleration);
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

	public void InitializeStartPosition()
	{
		if (railMap == null)
		{
			LogRailWarning("railMap is null. Player cannot move on rail.");
			return;
		}

		if (!TryResolveInitialSegment(out RailSegment2D segment))
		{
			LogRailWarning($"No valid start segment found. currentSegmentId={currentSegmentId}.");
			return;
		}

		EnsureSegmentLengthTable(segment);

		if (!IsSegmentUsable(segment))
		{
			LogRailWarning($"Start segment {segment.segmentId} has invalid bakedPoints or zero length.");
			return;
		}

		distanceOnSegment = segment.Length * Mathf.Clamp01(normalizedStartPosition);
		SnapToCurrentSegment();
	}

	private bool TryResolveInitialSegment(out RailSegment2D segment)
	{
		if (railMap.TryGetSegment(currentSegmentId, out segment) && IsSegmentUsable(segment))
		{
			return true;
		}

		LogRailWarning($"currentSegmentId {currentSegmentId} is invalid. Trying defaultStartSegmentId.");

		if (railMap.TryGetDefaultStartSegment(out segment) && IsSegmentUsable(segment))
		{
			currentSegmentId = segment.segmentId;
			LogRailWarning($"Auto switched to defaultStartSegmentId {currentSegmentId}.");
			return true;
		}

		if (TryUseFirstAvailableSegment(out segment))
		{
			return true;
		}

		segment = null;
		return false;
	}

	private bool TryUseFirstAvailableSegment(out RailSegment2D segment)
	{
		segment = null;

		if (railMap == null || railMap.segments == null)
		{
			return false;
		}

		for (int i = 0; i < railMap.segments.Count; i++)
		{
			RailSegment2D candidate = railMap.segments[i];

			if (!IsSegmentUsable(candidate))
			{
				continue;
			}

			currentSegmentId = candidate.segmentId;
			segment = candidate;

			LogRailWarning($"Auto switched to first available segment {currentSegmentId}.");
			return true;
		}

		return false;
	}

	private static bool IsSegmentUsable(RailSegment2D segment)
	{
		if (segment == null)
		{
			return false;
		}

		if (segment.bakedPoints == null || segment.bakedPoints.Length < 2)
		{
			return false;
		}

		EnsureSegmentLengthTable(segment);

		return segment.Length > Mathf.Epsilon;
	}

	public void TickMove(float horizontalAxis, float verticalAxis, float deltaTime)
	{
		RailExitChoice2D verticalChoice = ReadVerticalChoice(verticalAxis);
		int horizontalSign = ReadHorizontalSign(horizontalAxis);

		TickBranchInputBuffer(verticalChoice, deltaTime);
		MoveAlongCurrentSegment(horizontalSign, deltaTime);
	}

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

	/// <summary>
	/// 把 Player 设置到指定命名节点上。
	/// </summary>
	public bool TrySetStartAtNode(
		string nodeKey,
		RailExitChoice2D preferredExitChoice = RailExitChoice2D.Auto,
		bool snapImmediately = true)
	{
		if (railMap == null)
		{
			LogRailWarning("Cannot set start at node because railMap is null.");
			return false;
		}

		if (!railMap.TryGetNodeByKey(nodeKey, out RailNode2D node))
		{
			LogRailWarning($"Cannot find node by key: {nodeKey}.");
			return false;
		}

		if (!TryResolveSpawnSegmentAtNode(node, preferredExitChoice, out RailSegment2D segment, out RailEndpoint2D enterFrom))
		{
			LogRailWarning($"Node {node.nodeKey} exists, but no connected segment can be used as start segment.");
			return false;
		}

		EnsureSegmentLengthTable(segment);

		if (!IsSegmentUsable(segment))
		{
			LogRailWarning($"Resolved spawn segment {segment.segmentId} is not usable.");
			return false;
		}

		currentSegmentId = segment.segmentId;
		distanceOnSegment = enterFrom == RailEndpoint2D.Start ? 0f : segment.Length;

		if (snapImmediately)
		{
			MoveBodyPosition(node.position);
		}

		return true;
	}

	private bool TryResolveSpawnSegmentAtNode(
		RailNode2D node,
		RailExitChoice2D preferredExitChoice,
		out RailSegment2D segment,
		out RailEndpoint2D enterFrom)
	{
		segment = null;
		enterFrom = RailEndpoint2D.Start;

		if (node == null)
		{
			return false;
		}

		if (TryResolveSpawnExit(node.nodeId, preferredExitChoice, out segment, out enterFrom))
		{
			return true;
		}

		if (preferredExitChoice != RailExitChoice2D.Auto &&
			TryResolveSpawnExit(node.nodeId, RailExitChoice2D.Auto, out segment, out enterFrom))
		{
			return true;
		}

		if (preferredExitChoice != RailExitChoice2D.Right &&
			TryResolveSpawnExit(node.nodeId, RailExitChoice2D.Right, out segment, out enterFrom))
		{
			return true;
		}

		if (preferredExitChoice != RailExitChoice2D.Left &&
			TryResolveSpawnExit(node.nodeId, RailExitChoice2D.Left, out segment, out enterFrom))
		{
			return true;
		}

		return railMap.TryGetFirstConnectedSegment(node.nodeId, -1, out segment, out enterFrom);
	}

	private bool TryResolveSpawnExit(
		int nodeId,
		RailExitChoice2D choice,
		out RailSegment2D segment,
		out RailEndpoint2D enterFrom)
	{
		segment = null;
		enterFrom = RailEndpoint2D.Start;

		bool hasExit = railMap.TryResolveBranchExit(
			nodeId,
			-1,
			choice == RailExitChoice2D.Up || choice == RailExitChoice2D.Down ? choice : RailExitChoice2D.None,
			choice == RailExitChoice2D.Left || choice == RailExitChoice2D.Right ? choice : RailExitChoice2D.None,
			out RailExit2D exit);

		return TryGetSegmentFromExit(exit, hasExit, out segment, out enterFrom);
	}

	private bool TryGetSegmentFromExit(
		RailExit2D exit,
		bool hasExit,
		out RailSegment2D segment,
		out RailEndpoint2D enterFrom)
	{
		segment = null;
		enterFrom = RailEndpoint2D.Start;

		if (!hasExit || exit == null)
		{
			return false;
		}

		if (!railMap.TryGetSegment(exit.segmentId, out segment))
		{
			return false;
		}

		if (!IsSegmentUsable(segment))
		{
			return false;
		}

		enterFrom = exit.enterFrom;
		return true;
	}

	/// <summary>
	/// 把 Player 从任意世界坐标接入最近的 Rail 路线。
	/// </summary>
	public bool TryAttachToNearestRail(Vector2 worldPosition, bool snapToRail)
	{
		if (railMap == null)
		{
			LogRailWarning("Cannot attach to nearest rail because railMap is null.");
			return false;
		}

		if (!railMap.TryFindNearestRailPoint(worldPosition, out RailAttachResult2D result))
		{
			LogRailWarning("Cannot attach to nearest rail because no usable rail point was found.");
			return false;
		}

		currentSegmentId = result.segmentId;
		distanceOnSegment = result.distanceOnSegment;

		if (snapToRail)
		{
			MoveBodyPosition(result.nearestPosition);
		}

		return true;
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

	private void TickBranchInputBuffer(RailExitChoice2D verticalChoice, float deltaTime)
	{
		if (verticalChoice == RailExitChoice2D.Up || verticalChoice == RailExitChoice2D.Down)
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

	private int GetDistanceMoveSign(RailSegment2D segment, int horizontalSign)
	{
		if (horizontalSign == 0)
		{
			return 0;
		}

		if (!autoMatchInputToWorldX)
		{
			return horizontalSign;
		}

		if (segment == null || segment.bakedPoints == null || segment.bakedPoints.Length < 2)
		{
			return horizontalSign;
		}

		Vector2 firstPoint = segment.bakedPoints[0];
		Vector2 lastPoint = segment.bakedPoints[segment.bakedPoints.Length - 1];
		float directionX = lastPoint.x - firstPoint.x;

		if (Mathf.Abs(directionX) <= Mathf.Epsilon)
		{
			return horizontalSign;
		}

		int segmentWorldXSign = directionX > 0f ? 1 : -1;
		return horizontalSign * segmentWorldXSign;
	}

	private void MoveAlongCurrentSegment(int horizontalSign, float deltaTime)
	{
		if (railMap == null)
		{
			return;
		}

		if (!railMap.TryGetSegment(currentSegmentId, out RailSegment2D segment))
		{
			LogRailWarning($"Cannot move. Segment {currentSegmentId} does not exist.");
			return;
		}

		EnsureSegmentLengthTable(segment);

		if (!IsSegmentUsable(segment))
		{
			LogRailWarning($"Cannot move. Segment {currentSegmentId} is not usable.");
			return;
		}

		int distanceMoveSign = GetDistanceMoveSign(segment, horizontalSign);

		if (distanceMoveSign == 0)
		{
			currentMoveSpeed = 0f;
			return;
		}

		float frameMoveDistance = CalculateFrameMoveDistance(deltaTime);
		distanceOnSegment += distanceMoveSign * frameMoveDistance;

		ResolveSegmentBoundary(segment, horizontalSign, distanceMoveSign);

		if (useVectorTargetMovement)
		{
			MoveTowardsCurrentRailTarget(frameMoveDistance, distanceMoveSign);
			return;
		}

		SnapToCurrentSegment();
	}

	private float CalculateFrameMoveDistance(float deltaTime)
	{
		if (acceleration <= 0f)
		{
			currentMoveSpeed = moveSpeed;
		}
		else
		{
			currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, moveSpeed, acceleration * deltaTime);
		}

		return currentMoveSpeed * deltaTime;
	}

	private void MoveTowardsCurrentRailTarget(float maxMoveDistance, int distanceMoveSign)
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

		if (!IsSegmentUsable(segment))
		{
			return;
		}

		float targetDistance = distanceOnSegment;

		if (targetLeadDistance > 0f && distanceMoveSign != 0)
		{
			targetDistance += distanceMoveSign * targetLeadDistance;
			targetDistance = Mathf.Clamp(targetDistance, 0f, segment.Length);
		}

		Vector2 targetPosition = segment.GetPointByDistance(targetDistance);
		Vector2 originPosition = GetCurrentBodyPosition();
		Vector2 toTarget = targetPosition - originPosition;

		if (toTarget.sqrMagnitude <= targetArriveEpsilon * targetArriveEpsilon)
		{
			MoveBodyPosition(targetPosition);
			return;
		}

		float distanceToTarget = toTarget.magnitude;
		Vector2 direction = toTarget / distanceToTarget;
		Vector2 displacement = direction * Mathf.Min(maxMoveDistance, distanceToTarget);
		Vector2 nextPosition = originPosition + displacement;

		MoveBodyPosition(nextPosition);
	}

	private Vector2 GetCurrentBodyPosition()
	{
		if (Application.isPlaying && rb != null)
		{
			return rb.position;
		}

		Vector3 position = transform.position;
		return new Vector2(position.x, position.y);
	}

	private void MoveBodyPosition(Vector2 position)
	{
		if (Application.isPlaying && rb != null)
		{
			rb.MovePosition(position);
			return;
		}

		transform.position = new Vector3(position.x, position.y, transform.position.z);
	}

	private void ResolveSegmentBoundary(RailSegment2D segment, int horizontalSign, int distanceMoveSign)
	{
		bool hasVerticalBranchChoice =
			bufferedVerticalChoice == RailExitChoice2D.Up ||
			bufferedVerticalChoice == RailExitChoice2D.Down;

		if (distanceOnSegment <= nodeArriveEpsilon)
		{
			bool wantsLeaveStart =
				distanceOnSegment < 0f ||
				distanceMoveSign < 0 ||
				hasVerticalBranchChoice;

			if (wantsLeaveStart)
			{
				float overflowDistance = Mathf.Max(0f, -distanceOnSegment);
				TrySwitchAtNode(segment.startNodeId, horizontalSign, RailEndpoint2D.Start, overflowDistance);
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
				distanceMoveSign > 0 ||
				hasVerticalBranchChoice;

			if (wantsLeaveEnd)
			{
				float overflowDistance = Mathf.Max(0f, distanceOnSegment - segment.Length);
				TrySwitchAtNode(segment.endNodeId, horizontalSign, RailEndpoint2D.End, overflowDistance);
			}
			else
			{
				distanceOnSegment = Mathf.Min(segment.Length, distanceOnSegment);
			}
		}
	}

	private void TrySwitchAtNode(
		int nodeId,
		int horizontalSign,
		RailEndpoint2D arrivedEndpoint,
		float overflowDistance)
	{
		int fromSegmentId = currentSegmentId;
		RailExitChoice2D horizontalChoice = ToHorizontalChoice(horizontalSign);

		bool hasExit = railMap.TryResolveBranchExit(
			nodeId,
			fromSegmentId,
			bufferedVerticalChoice,
			horizontalChoice,
			out RailExit2D exit);

		if (!hasExit && IsBufferedVerticalChoiceActive())
		{
			bool wantUp = bufferedVerticalChoice == RailExitChoice2D.Up;
			hasExit = TryInferVerticalExitByWorldY(nodeId, wantUp, out exit);
		}

		if (!hasExit && autoContinueThroughConnectedSegments)
		{
			hasExit = TryInferConnectedExit(nodeId, horizontalSign, out exit);
		}

		if (!hasExit)
		{
			LogRailWarning(
				$"Node {nodeId} has no exit. " +
				$"fromSegmentId={fromSegmentId}, " +
				$"verticalChoice={bufferedVerticalChoice}, " +
				$"horizontalChoice={horizontalChoice}, " +
				$"currentSegmentId={currentSegmentId}.");

			ClampToCurrentSegmentEnd(arrivedEndpoint);
			return;
		}

		if (!railMap.TryGetSegment(exit.segmentId, out RailSegment2D nextSegment))
		{
			LogRailWarning($"Node {nodeId} resolved exit to segment {exit.segmentId}, but that segment does not exist.");
			ClampToCurrentSegmentEnd(arrivedEndpoint);
			return;
		}

		EnsureSegmentLengthTable(nextSegment);

		if (!IsSegmentUsable(nextSegment))
		{
			LogRailWarning($"Node {nodeId} resolved exit to segment {exit.segmentId}, but that segment is not usable.");
			ClampToCurrentSegmentEnd(arrivedEndpoint);
			return;
		}

		currentSegmentId = exit.segmentId;

		if (exit.enterFrom == RailEndpoint2D.Start)
		{
			distanceOnSegment = overflowDistance;
		}
		else
		{
			distanceOnSegment = nextSegment.Length - overflowDistance;
		}

		distanceOnSegment = Mathf.Clamp(distanceOnSegment, 0f, nextSegment.Length);

		bufferedVerticalChoice = RailExitChoice2D.None;
		bufferedVerticalTimer = 0f;
	}

	private bool IsBufferedVerticalChoiceActive()
	{
		return bufferedVerticalChoice == RailExitChoice2D.Up ||
			   bufferedVerticalChoice == RailExitChoice2D.Down;
	}

	private bool TryInferVerticalExitByWorldY(int nodeId, bool wantUp, out RailExit2D exit)
	{
		exit = null;

		if (railMap == null || railMap.segments == null)
		{
			return false;
		}

		RailSegment2D bestSegment = null;
		RailEndpoint2D bestEnterFrom = RailEndpoint2D.Start;
		float bestScore = float.NegativeInfinity;

		for (int i = 0; i < railMap.segments.Count; i++)
		{
			RailSegment2D candidate = railMap.segments[i];

			if (!IsSegmentUsable(candidate))
			{
				continue;
			}

			if (candidate.segmentId == currentSegmentId)
			{
				continue;
			}

			bool connectedToStart = candidate.startNodeId == nodeId;
			bool connectedToEnd = candidate.endNodeId == nodeId;

			if (!connectedToStart && !connectedToEnd)
			{
				continue;
			}

			RailEndpoint2D enterFrom = connectedToStart ? RailEndpoint2D.Start : RailEndpoint2D.End;
			Vector2 leaveDirection = GetLeaveDirectionFromEndpoint(candidate, enterFrom);

			if (leaveDirection.sqrMagnitude <= Mathf.Epsilon)
			{
				continue;
			}

			Vector2 normalizedDirection = leaveDirection.normalized;
			float score = wantUp ? normalizedDirection.y : -normalizedDirection.y;

			if (score > bestScore)
			{
				bestScore = score;
				bestSegment = candidate;
				bestEnterFrom = enterFrom;
			}
		}

		if (bestSegment == null)
		{
			return false;
		}

		exit = new RailExit2D
		{
			choice = wantUp ? RailExitChoice2D.Up : RailExitChoice2D.Down,
			segmentId = bestSegment.segmentId,
			enterFrom = bestEnterFrom,
			fromSegmentId = currentSegmentId,
			priority = 0
		};

		return true;
	}

	private bool TryInferConnectedExit(int nodeId, int horizontalSign, out RailExit2D exit)
	{
		exit = null;

		if (railMap == null || railMap.segments == null)
		{
			return false;
		}

		RailSegment2D bestSegment = null;
		RailEndpoint2D bestEnterFrom = RailEndpoint2D.Start;
		float bestScore = float.NegativeInfinity;

		for (int i = 0; i < railMap.segments.Count; i++)
		{
			RailSegment2D candidate = railMap.segments[i];

			if (!IsSegmentUsable(candidate))
			{
				continue;
			}

			if (candidate.segmentId == currentSegmentId)
			{
				continue;
			}

			bool connectedToStart = candidate.startNodeId == nodeId;
			bool connectedToEnd = candidate.endNodeId == nodeId;

			if (!connectedToStart && !connectedToEnd)
			{
				continue;
			}

			RailEndpoint2D enterFrom = connectedToStart ? RailEndpoint2D.Start : RailEndpoint2D.End;
			Vector2 leaveDirection = GetLeaveDirectionFromEndpoint(candidate, enterFrom);
			float score = ScoreInferredExit(leaveDirection, horizontalSign);

			if (score > bestScore)
			{
				bestScore = score;
				bestSegment = candidate;
				bestEnterFrom = enterFrom;
			}
		}

		if (bestSegment == null)
		{
			return false;
		}

		exit = new RailExit2D
		{
			choice = RailExitChoice2D.Auto,
			segmentId = bestSegment.segmentId,
			enterFrom = bestEnterFrom,
			fromSegmentId = currentSegmentId,
			priority = 0
		};

		return true;
	}

	private static Vector2 GetLeaveDirectionFromEndpoint(RailSegment2D segment, RailEndpoint2D enterFrom)
	{
		if (segment == null || segment.bakedPoints == null || segment.bakedPoints.Length < 2)
		{
			return Vector2.zero;
		}

		if (enterFrom == RailEndpoint2D.Start)
		{
			return segment.bakedPoints[1] - segment.bakedPoints[0];
		}

		int lastIndex = segment.bakedPoints.Length - 1;
		return segment.bakedPoints[lastIndex - 1] - segment.bakedPoints[lastIndex];
	}

	private static float ScoreInferredExit(Vector2 leaveDirection, int horizontalSign)
	{
		if (leaveDirection.sqrMagnitude <= Mathf.Epsilon)
		{
			return -1000f;
		}

		Vector2 normalizedDirection = leaveDirection.normalized;

		if (horizontalSign > 0)
		{
			return normalizedDirection.x;
		}

		if (horizontalSign < 0)
		{
			return -normalizedDirection.x;
		}

		return 0f;
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
		distanceOnSegment = arrivedEndpoint == RailEndpoint2D.Start ? 0f : segment.Length;
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
		MoveBodyPosition(targetPosition);
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

	private void LogRailWarning(string message)
	{
		if (!logRailDebug)
		{
			return;
		}

		Debug.LogWarning($"{nameof(RailWalker2D)} [{name}]: {message}", this);
	}
}