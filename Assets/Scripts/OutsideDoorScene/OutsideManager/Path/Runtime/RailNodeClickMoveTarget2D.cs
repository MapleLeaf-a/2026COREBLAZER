using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 点击 2D 碰撞体后，让 RailWalker2D 自动移动到指定 NodeKey。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
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

	private void Start()
	{
		ResolveTargetWalker();
	}

	private void OnMouseDown()
	{
		if (ShouldIgnoreClick())
		{
			return;
		}

		TryRequestAutoMove();
	}

	/// <summary>
	/// 尝试发起自动移动请求。
	/// </summary>
	/// <returns>
	/// true 表示成功向 RailWalker2D 发起自动移动。
	/// false 表示配置不完整或目标节点不存在。
	/// </returns>
	public bool TryRequestAutoMove()
	{
		ResolveTargetWalker();

		if (targetWalker == null)
		{
			LogClickWarning("没有绑定 RailWalker2D，无法发起自动移动。");
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
			LogClickWarning("当前 RailMap2DAsset 为空，无法查找目标节点。");
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

		bool started = targetWalker.TryAutoMoveToNodeKey(targetNode.nodeKey);

		if (started)
		{
			LogClick($"已请求角色自动移动到节点：{targetNode.nodeKey}");
		}

		return started;
	}

	/// <summary>
	/// 在必要时自动查找 RailWalker2D。
	/// </summary>
	private void ResolveTargetWalker()
	{
		if (targetWalker != null)
		{
			return;
		}

		if (!autoFindWalkerWhenMissing)
		{
			return;
		}

		targetWalker = FindObjectOfType<RailWalker2D>();
	}

	/// <summary>
	/// 判断当前点击是否应该被忽略。
	/// </summary>
	/// <returns>
	/// true 表示本次点击不应该触发自动移动。
	/// false 表示可以继续处理点击。
	/// </returns>
	private bool ShouldIgnoreClick()
	{
		if (!ignoreClickWhenPointerOverUI)
		{
			return false;
		}

		if (EventSystem.current == null)
		{
			return false;
		}

		return EventSystem.current.IsPointerOverGameObject();
	}

	/// <summary>
	/// 打印普通点击日志。
	/// </summary>
	/// <param name="message">
	/// message：
	/// 需要输出的日志内容。
	/// </param>
	private void LogClick(string message)
	{
		if (!logClickDebug)
		{
			return;
		}

		Debug.Log($"[RailNodeClickMoveTarget2D] {message}", this);
	}

	/// <summary>
	/// 打印点击警告日志。
	/// </summary>
	/// <param name="message">
	/// message：
	/// 需要输出的警告内容。
	/// </param>
	private void LogClickWarning(string message)
	{
		if (!logClickDebug)
		{
			return;
		}

		Debug.LogWarning($"[RailNodeClickMoveTarget2D] {message}", this);
	}
}