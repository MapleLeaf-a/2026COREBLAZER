using Events;
using UnityEngine;

namespace GameScene
{
	/// <summary>
	/// 2D 场景切换触发盒。
	///
	/// 这个脚本挂在带 Collider2D 的触发盒对象上。
	/// 它只负责检测玩家是否满足切换条件，不直接加载场景。
	/// 真正的场景加载由 SceneTransitionManager 处理。
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Collider2D))]
	public sealed class SceneTransitionTrigger2D : MonoBehaviour
	{
		[Header("Target Scene")]

		/// <summary>
		/// 要切换到的目标场景名称。
		/// 必须和 Unity Build Settings 中的场景名称一致。
		/// </summary>
		[SerializeField]
		private string targetSceneName;

		[Header("Player Filter")]

		/// <summary>
		/// 玩家层级过滤器。
		/// 只有处于这些 Layer 中的对象，才会被视为玩家。
		/// </summary>
		[SerializeField]
		private LayerMask playerLayerMask;

		[Header("Direction Rule")]

		/// <summary>
		/// 允许触发切换的移动方向。
		/// 例如配置为 Left 时，玩家必须向左移动才会触发切换。
		/// </summary>
		[SerializeField]
		private SceneTransitionDirection requiredDirection = SceneTransitionDirection.None;

		/// <summary>
		/// 最小移动距离。
		/// 小于该距离时认为玩家没有真正移动，用于避免站着不动时误触发。
		/// </summary>
		[SerializeField]
		private float minMoveDistance = 0.01f;

		/// <summary>
		/// 方向匹配阈值。
		/// 取值范围是 0 到 1。
		/// 越接近 1，要求玩家移动方向越精准。
		/// </summary>
		[SerializeField]
		[Range(0f, 1f)]
		private float directionDotThreshold = 0.65f;

		[Header("Fade")]

		/// <summary>
		/// 黑屏淡出持续时间，单位是秒。
		/// </summary>
		[SerializeField]
		private float fadeOutDuration = 0.35f;

		/// <summary>
		/// 黑屏淡入持续时间，单位是秒。
		/// </summary>
		[SerializeField]
		private float fadeInDuration = 0.35f;

		/// <summary>
		/// 是否已经触发过。
		/// 用于避免玩家停留在触发盒内时重复发布切换事件。
		/// </summary>
		private bool hasTriggered;

		/// <summary>
		/// 玩家上一帧的位置。
		/// 当前帧位置减去上一帧位置，就能得到玩家实际移动方向。
		/// </summary>
		private Vector2 lastPlayerPosition;

		/// <summary>
		/// 是否已经记录过玩家上一帧位置。
		/// 玩家刚进入触发盒时只记录位置，不立刻触发切换。
		/// </summary>
		private bool hasLastPlayerPosition;

		private void Reset()
		{
			// Reset 在脚本刚挂到 GameObject 上时执行。
			// 自动设置成 Trigger，减少手动配置出错的可能。
			Collider2D triggerCollider = GetComponent<Collider2D>();
			triggerCollider.isTrigger = true;
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (!IsPlayer(other.gameObject))
			{
				return;
			}

			// 玩家刚进入触发盒时只记录位置。
			// 不在 Enter 中直接触发，是为了避免玩家刚好出生在触发盒里导致误切场景。
			lastPlayerPosition = other.transform.position;
			hasLastPlayerPosition = true;
		}

		private void OnTriggerStay2D(Collider2D other)
		{
			if (hasTriggered)
			{
				return;
			}

			GameObject player = other.gameObject;

			if (!IsPlayer(player))
			{
				return;
			}

			Vector2 currentPlayerPosition = player.transform.position;

			if (!hasLastPlayerPosition)
			{
				lastPlayerPosition = currentPlayerPosition;
				hasLastPlayerPosition = true;
				return;
			}

			Vector2 moveDelta = currentPlayerPosition - lastPlayerPosition;

			// 先更新上一帧位置。
			// 即使本帧没有触发，下次检测也能基于最新位置继续判断。
			lastPlayerPosition = currentPlayerPosition;

			if (!IsMovingEnough(moveDelta))
			{
				return;
			}

			if (!IsDirectionMatched(moveDelta))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(targetSceneName))
			{
				Debug.LogWarning($"{nameof(SceneTransitionTrigger2D)} on {name} has no target scene name.");
				return;
			}

			hasTriggered = true;

			EventBus.Publish(new SceneTransitionRequestEvent(
				targetSceneName,
				player,
				fadeOutDuration,
				fadeInDuration));
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			if (!IsPlayer(other.gameObject))
			{
				return;
			}

			// 玩家离开触发盒后，允许下一次重新触发。
			hasTriggered = false;
			hasLastPlayerPosition = false;
		}

		private bool IsPlayer(GameObject target)
		{
			// target.layer 是对象当前所在的 Layer 编号。
			// 1 << target.layer 会把 Layer 编号转换成二进制位。
			// 和 playerLayerMask 做按位与后不为 0，表示该对象属于允许的玩家层级。
			return (playerLayerMask.value & (1 << target.layer)) != 0;
		}

		private bool IsMovingEnough(Vector2 moveDelta)
		{
			// sqrMagnitude 是向量长度的平方。
			// 使用平方比较可以避免开方计算，比 magnitude 更适合频繁检测。
			return moveDelta.sqrMagnitude >= minMoveDistance * minMoveDistance;
		}

		private bool IsDirectionMatched(Vector2 moveDelta)
		{
			if (requiredDirection == SceneTransitionDirection.None)
			{
				return true;
			}

			Vector2 requiredVector = GetRequiredDirectionVector(requiredDirection);
			Vector2 actualMoveDirection = moveDelta.normalized;

			// 点积越接近 1，说明实际移动方向越接近配置方向。
			float dot = Vector2.Dot(actualMoveDirection, requiredVector);

			return dot >= directionDotThreshold;
		}

		private static Vector2 GetRequiredDirectionVector(SceneTransitionDirection direction)
		{
			switch (direction)
			{
				case SceneTransitionDirection.Left:
					return Vector2.left;

				case SceneTransitionDirection.Right:
					return Vector2.right;

				case SceneTransitionDirection.Up:
					return Vector2.up;

				case SceneTransitionDirection.Down:
					return Vector2.down;

				default:
					return Vector2.zero;
			}
		}
	}
}
