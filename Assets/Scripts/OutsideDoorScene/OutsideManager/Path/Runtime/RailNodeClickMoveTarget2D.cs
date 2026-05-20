using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 点击 2D 碰撞体后，让 RailWalker2D 自动移动到指定 NodeKey。
/// 自动移动完成后，只有 Player Collider2D 与当前对象 Collider2D 重合时，才触发 InteractionPoint2D。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(InteractionPoint2D))]
public sealed class RailNodeClickMoveTarget2D : MonoBehaviour
{
	[Header("Target Node")]

	/// <summary>
	/// 目标节点查询名。
	///
	/// 这个值必须等于 RailMap2DAsset.nodes 里某个 RailNode2D.nodeKey。
	/// 示例：
	/// Spawn_Door_Left
	/// Fork_OutsideDoor_01
	/// </summary>
	[SerializeField]
	private string nodeKey;

	/// <summary>
	/// RailMap 覆盖引用。
	///
	/// 为空时：
	/// 使用 targetWalker.RailMap，也就是角色当前正在使用的场景 RailMap。
	///
	/// 不为空时：
	/// 会先把这个 RailMap 设置给 targetWalker，再发起自动移动。
	/// </summary>
	[SerializeField]
	private RailMap2DAsset railMapOverride;

	[Header("Walker")]

	/// <summary>
	/// 目标 RailWalker2D。
	///
	/// 一般拖 Player 根节点上的 RailWalker2D。
	/// 如果为空，并且 autoFindWalkerWhenMissing 为 true，
	/// Awake 时会自动在场景里查找一个 RailWalker2D。
	/// </summary>
	[SerializeField]
	private RailWalker2D targetWalker;

	/// <summary>
	/// 当 targetWalker 没有手动绑定时，是否自动查找场景中的 RailWalker2D。
	/// </summary>
	[SerializeField]
	private bool autoFindWalkerWhenMissing = true;

	[Header("Overlap Check")]

	/// <summary>
	/// Player 的碰撞盒。
	///
	/// 推荐在 Inspector 里手动绑定。
	/// 如果 Player 有多个 Collider2D，必须手动指定正确的那个。
	/// </summary>
	[SerializeField]
	private Collider2D playerCollider;

	/// <summary>
	/// 自动移动完成后等待碰撞重合的最长时间。
	///
	/// 小于等于 0 表示不启用超时。
	/// </summary>
	[SerializeField]
	[Min(0f)]
	private float overlapWaitTimeout = 1f;

	[Header("Click")]

	/// <summary>
	/// 鼠标点在 UI 上时是否忽略本次点击。
	/// true 可以避免点击背包、按钮等 UI 时误触场景中的节点碰撞体。
	/// </summary>
	[SerializeField]
	private bool ignoreClickWhenPointerOverUI = true;

	/// <summary>
	/// 是否打印点击移动日志。
	/// </summary>
	[SerializeField]
	private bool logClickDebug = true;

	private Collider2D targetCollider;
	private InteractionPoint2D interactionPoint;
	private RailWalker2D subscribedWalker;

	private bool pendingInteraction;
	private bool waitingForAutoMoveFinished;
	private bool waitingForOverlap;
	private int pendingTargetNodeId = -1;
	private float overlapWaitTimer;

	private void Awake()
	{
		targetCollider = GetComponent<Collider2D>();
		interactionPoint = GetComponent<InteractionPoint2D>();
		ResolveTargetWalker();
		ResolvePlayerCollider();
	}

	private void OnEnable()
	{
		ResolveTargetWalker();
		SubscribeWalkerEvent();
	}

	private void OnDisable()
	{
		UnsubscribeWalkerEvent();
		ClearPendingInteraction();
	}

	private void FixedUpdate()
	{
		if (!pendingInteraction || !waitingForOverlap)
		{
			return;
		}

		if (overlapWaitTimeout > 0f)
		{
			overlapWaitTimer += Time.fixedDeltaTime;

			if (overlapWaitTimer > overlapWaitTimeout)
			{
				LogClickWarning("等待 Player 碰撞盒重合超时，取消本次交互触发。");
				ClearPendingInteraction();
				return;
			}
		}

		TryTriggerPendingInteractionIfOverlapped();
	}

	private void OnMouseDown()
	{
		if (ShouldIgnoreClick())
		{
			return;
		}

		TryRequestAutoMoveAndInteraction();
	}

	/// <summary>
	/// 发起"点击目标 -> 自动移动 -> 等待碰撞重合 -> 触发交互"的完整流程。
	/// </summary>
	public bool TryRequestAutoMoveAndInteraction()
	{
		ResolveTargetWalker();
		ResolvePlayerCollider();
		SubscribeWalkerEvent();

		if (targetWalker == null || targetCollider == null || interactionPoint == null || playerCollider == null)
		{
			LogClickWarning("点击自动移动交互配置不完整。");
			return false;
		}

		if (string.IsNullOrWhiteSpace(nodeKey))
		{
			LogClickWarning("nodeKey 为空，无法发起自动移动。");
			return false;
		}

		RailMap2DAsset activeRailMap = railMapOverride != null
			? railMapOverride
			: targetWalker.RailMap;

		if (activeRailMap == null)
		{
			LogClickWarning("当前 RailMap2DAsset 为空，无法查找目标节点。请确保 CreatePlayerEvent 已触发或 RailWalker2D 的 RailMap 已在 Inspector 中设置。");
			return false;
		}

		// 检查 targetWalker 是否已初始化（currentSegmentId 应 >= 0）
		if (targetWalker.CurrentSegmentId < 0)
		{
			LogClickWarning("RailWalker2D 尚未初始化（CurrentSegmentId < 0）。请确保 CreatePlayerEvent 已触发。");
			return false;
		}

		if (!activeRailMap.TryGetNodeByKey(nodeKey, out RailNode2D targetNode))
		{
			LogClickWarning($"RailMap 中找不到目标 nodeKey：{nodeKey}");
			return false;
		}

		if (railMapOverride != null)
		{
			targetWalker.ResetMapData(railMapOverride);
		}

		// 先设置等待状态，再调用 TryAutoMoveToNodeKey。
		// 原因：如果 Player 已经在目标节点附近，RailWalker2D 可能同步发出完成事件。
		pendingInteraction = true;
		waitingForAutoMoveFinished = true;
		waitingForOverlap = false;
		pendingTargetNodeId = targetNode.nodeId;
		overlapWaitTimer = 0f;

		bool started = targetWalker.TryAutoMoveToNodeKey(targetNode.nodeKey);

		if (!started)
		{
			LogClickWarning($"自动移动无法开始。nodeKey={targetNode.nodeKey}");
			ClearPendingInteraction();
			return false;
		}

		LogClick($"已请求角色自动移动到节点：{targetNode.nodeKey}");
		return true;
	}

	private void OnAutoNodeMovementFinished(bool reachedTarget, int targetNodeId)
	{
		if (!pendingInteraction || !waitingForAutoMoveFinished)
		{
			return;
		}

		if (targetNodeId != pendingTargetNodeId)
		{
			return;
		}

		waitingForAutoMoveFinished = false;

		if (!reachedTarget)
		{
			LogClickWarning("自动移动没有到达目标，取消本次交互触发。");
			ClearPendingInteraction();
			return;
		}

		waitingForOverlap = true;
		overlapWaitTimer = 0f;

		// 自动移动完成的这一帧可能已经重合，因此立即检测一次。
		TryTriggerPendingInteractionIfOverlapped();
	}

	private bool TryTriggerPendingInteractionIfOverlapped()
	{
		if (!IsPlayerOverlappingTarget())
		{
			return false;
		}

		TriggerInteractionPoint();
		ClearPendingInteraction();
		return true;
	}

	private bool IsPlayerOverlappingTarget()
	{
		if (targetCollider == null || playerCollider == null)
		{
			return false;
		}

		// ColliderDistance2D 会基于 Collider2D 的真实形状判断是否重合。
		ColliderDistance2D distance = targetCollider.Distance(playerCollider);
		return distance.isOverlapped;
	}

	private void TriggerInteractionPoint()
	{
		LogClick("Player 已与目标碰撞盒重合，触发 InteractionPoint2D。");
		interactionPoint.TriggerInteractionFromClickMove(playerCollider.gameObject);
	}

	private void ResolveTargetWalker()
	{
		if (targetWalker == null && autoFindWalkerWhenMissing)
		{
			targetWalker = FindObjectOfType<RailWalker2D>();
		}

		SubscribeWalkerEvent();
	}

	private void ResolvePlayerCollider()
	{
		if (playerCollider != null || targetWalker == null)
		{
			return;
		}

		// 如果 Player 有多个 Collider2D，必须在 Inspector 中手动指定正确的 playerCollider。
		playerCollider = targetWalker.GetComponentInChildren<Collider2D>();

		if (playerCollider == null)
		{
			LogClickWarning("无法从 targetWalker 上获取 Player Collider2D。请在 Inspector 中手动绑定 playerCollider。");
		}
	}

	private void SubscribeWalkerEvent()
	{
		if (targetWalker == null || subscribedWalker == targetWalker)
		{
			return;
		}

		UnsubscribeWalkerEvent();
		subscribedWalker = targetWalker;
		subscribedWalker.AutoNodeMovementFinished += OnAutoNodeMovementFinished;
	}

	private void UnsubscribeWalkerEvent()
	{
		if (subscribedWalker == null)
		{
			return;
		}

		subscribedWalker.AutoNodeMovementFinished -= OnAutoNodeMovementFinished;
		subscribedWalker = null;
	}

	private bool ShouldIgnoreClick()
	{
		if (!ignoreClickWhenPointerOverUI || EventSystem.current == null)
		{
			return false;
		}

		return EventSystem.current.IsPointerOverGameObject();
	}

	private void ClearPendingInteraction()
	{
		pendingInteraction = false;
		waitingForAutoMoveFinished = false;
		waitingForOverlap = false;
		pendingTargetNodeId = -1;
		overlapWaitTimer = 0f;
	}

	private void LogClick(string message)
	{
		if (!logClickDebug)
		{
			return;
		}

		Debug.Log($"[RailNodeClickMoveTarget2D] {message}", this);
	}

	private void LogClickWarning(string message)
	{
		if (!logClickDebug)
		{
			return;
		}

		Debug.LogWarning($"[RailNodeClickMoveTarget2D] {message}", this);
	}
}
