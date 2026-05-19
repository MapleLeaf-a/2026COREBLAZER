using Events;
using UnityEngine;

namespace GameScene
{
	/// <summary>
	/// 2D 场景切换触发盒。
	///
	/// 这个脚本只负责检测玩家和发布事件。
	/// 它不直接加载场景。
	///
	/// 这样可以让触发逻辑和场景切换逻辑解耦。
	/// 后续如果要加入音效、存档、出生点、加载界面，只需要扩展 SceneTransitionManager。
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Collider2D))]
	public sealed class SceneTransitionTrigger2D : MonoBehaviour
	{
		[Header("Target Scene")]

		/// <summary>
		/// 要切换到的目标场景名称。
		///
		/// 必须和 Unity Build Settings 中的场景名称完全一致。
		/// </summary>
		[SerializeField]
		private string targetSceneName;

		[Header("Player Filter")]

		/// <summary>
		/// 玩家所在的 Layer。
		///
		/// LayerMask 是 Unity 的层级过滤器。
		/// 它可以让触发盒只响应 Player 层，忽略 NPC、道具、背景碰撞体。
		/// </summary>
		[SerializeField]
		private LayerMask playerLayerMask;

		[Header("Direction Rule")]

		/// <summary>
		/// 允许触发场景切换的移动方向。
		///
		/// None 表示不检查方向。
		/// Left 表示玩家必须向左移动。
		/// Right 表示玩家必须向右移动。
		/// Up 表示玩家必须向上移动。
		/// Down 表示玩家必须向下移动。
		/// </summary>
		[SerializeField]
		private SceneTransitionDirection requiredDirection = SceneTransitionDirection.None;

		/// <summary>
		/// 最小移动距离。
		///
		/// 如果玩家两次检测之间的位置变化小于这个值，
		/// 就认为玩家没有真正移动。
		///
		/// 这个字段用于避免玩家站在触发盒内不动时误触发。
		/// </summary>
		[SerializeField]
		private float minMoveDistance = 0.01f;

		/// <summary>
		/// 方向匹配阈值。
		///
		/// 这里使用 Dot 点积判断玩家移动方向是否接近配置方向。
		///
		/// Dot 点积可以简单理解成方向相似度：
		/// 1 表示完全同向。
		/// 0 表示垂直。
		/// -1 表示完全反向。
		///
		/// 默认 0.65 代表玩家大致朝配置方向移动即可。
		/// </summary>
		[SerializeField]
		[Range(0f, 1f)]
		private float directionDotThreshold = 0.65f;

		[Header("Fade")]

		/// <summary>
		/// 切出当前场景时的黑屏淡入时间。
		///
		/// 当前场景离开前，黑幕会从透明变成全黑。
		/// </summary>
		[SerializeField]
		private float exitFadeInDuration = 0.35f;

		/// <summary>
		/// 是否已经触发过场景切换。
		///
		/// 用于防止玩家停留在触发盒中时连续发布多次场景切换事件。
		/// </summary>
		private bool hasTriggered;

		/// <summary>
		/// 玩家上一次检测时的位置。
		///
		/// 当前位置减去上一次位置，就能得到玩家移动方向。
		/// </summary>
		private Vector2 lastPlayerPosition;

		/// <summary>
		/// 是否已经记录过玩家位置。
		///
		/// 第一次进入触发盒时只记录位置，不立刻触发。
		/// 这样可以避免玩家刚进入触发范围时因为方向数据不准确而误触发。
		/// </summary>
		private bool hasLastPlayerPosition;

		private void Reset()
		{
			// Reset 在脚本首次挂到对象上，或者手动点击 Reset 时执行。
			// 这里自动把 Collider2D 设置成 Trigger，减少手动配置错误。
			Collider2D triggerCollider = GetComponent<Collider2D>();
			triggerCollider.isTrigger = true;
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (!IsPlayer(other.gameObject))
			{
				return;
			}

			// 玩家刚进入触发盒时，只记录当前位置。
			// 不在 Enter 阶段直接切场景，是为了避免玩家站在边缘时误触发。
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

			// 更新上一帧位置。
			// 下一次 OnTriggerStay2D 才能继续计算新的移动方向。
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

			// 触发盒只发布事件，不直接切场景。
			// 真正的黑屏和 SceneManager.LoadSceneAsync 由 SceneTransitionManager 负责。
			EventBus.Publish(new SceneTransitionRequestEvent(
				targetSceneName,
				player,
				exitFadeInDuration));
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			if (!IsPlayer(other.gameObject))
			{
				return;
			}

			// 玩家离开触发盒后，允许下次重新触发。
			hasTriggered = false;
			hasLastPlayerPosition = false;
		}

		private bool IsPlayer(GameObject target)
		{
			// target.layer 是对象所在层级编号。
			// 1 << target.layer 会把层级编号转换成对应的二进制位。
			// 与 playerLayerMask 做按位与后不为 0，代表该层级被允许。
			return (playerLayerMask.value & (1 << target.layer)) != 0;
		}

		private bool IsMovingEnough(Vector2 moveDelta)
		{
			// sqrMagnitude 是向量长度的平方。
			// 使用平方比较可以避免开方计算，比 magnitude 更省性能。
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
