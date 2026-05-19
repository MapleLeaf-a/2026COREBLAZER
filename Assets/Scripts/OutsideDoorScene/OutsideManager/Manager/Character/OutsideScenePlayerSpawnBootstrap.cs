using Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.Manager
{
	/// <summary>
	/// 户外场景 Player 出生初始化器。
	///
	/// 作用：
	/// 1. 当前场景加载后，读取当前场景自己的 RailMap2DAsset。
	/// 2. 读取上一个场景名。
	/// 3. 用上一个场景名作为 nodeKey 查找当前 RailMap 中的入口节点。
	/// 4. 发布 CreatePlayerEvent，让 CharacterManager 创建或重置 Player。
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class OutsideScenePlayerSpawnBootstrap : MonoBehaviour
	{
		[Header("Character Resource")]

		/// <summary>
		/// 角色预制体在 Resources 目录下的路径。
		///
		/// 作用：
		/// 1. 作为 CharacterManager 的缓存 key。
		/// 2. 作为 Resources.Load<GameObject>() 的加载路径。
		///
		/// 填写示例：
		/// 如果预制体路径是：
		/// Assets/Resources/Characters/OutsidePlayer.prefab
		///
		/// 这里应该填写：
		/// Characters/OutsidePlayer
		///
		/// 注意：
		/// 不要填写 Assets/Resources。
		/// 不要填写 .prefab 后缀。
		/// </summary>
		[SerializeField]
		private string characterResourcePath = "Characters/OutsidePlayer";

		[Header("Current Scene Rail Map")]

		/// <summary>
		/// 当前场景使用的 RailMap2DAsset。
		///
		/// 作用：
		/// 1. 用上一个场景名查出生节点。
		/// 2. 赋值给当前 Player 的 RailWalker2D。
		/// </summary>
		[SerializeField]
		private RailMap2DAsset currentSceneRailMap;

		[Header("Fallback Spawn")]

		/// <summary>
		/// 默认出生节点名。
		///
		/// 作用：
		/// 当不是从别的场景切进来，或者用上一个场景名查不到节点时，
		/// 使用这个节点作为兜底出生点。
		/// </summary>
		[SerializeField]
		private string defaultSpawnNodeKey = "PlayerStart";

		/// <summary>
		/// 兜底出生位置。
		///
		/// 作用：
		/// 如果 currentSceneRailMap 为空，
		/// 或者 currentSceneRailMap 里没有找到任何可用节点，
		/// 就把 Player 放到这个世界坐标。
		/// </summary>
		[SerializeField]
		private Vector2 fallbackWorldPosition = Vector2.zero;

		/// <summary>
		/// 默认朝向。
		///
		/// 作用：
		/// 大于 0 表示朝右，小于等于 0 表示朝左。
		/// </summary>
		[SerializeField]
		private float defaultFacingSign = 1f;

		/// <summary>
		/// 出生后优先接入的路径出口。
		///
		/// 作用：
		/// 当出生节点连接多条 Rail Segment 时，
		/// 决定 RailWalker2D 优先进入哪条路径。
		/// </summary>
		[SerializeField]
		private RailExitChoice2D preferredExitChoice = RailExitChoice2D.Auto;

		private void Start()
		{
			CreateOrResetPlayerForCurrentScene();
		}

		/// <summary>
		/// 根据当前场景和跨场景上下文创建或重置 Player。
		/// </summary>
		private void CreateOrResetPlayerForCurrentScene()
		{
			if (currentSceneRailMap == null)
			{
				Debug.LogError($"{nameof(OutsideScenePlayerSpawnBootstrap)} 缺少 currentSceneRailMap。");
				PublishCreatePlayerEvent(string.Empty, fallbackWorldPosition);
				return;
			}

			string spawnNodeKey = ResolveSpawnNodeKey();
			Vector2 spawnPosition = ResolveSpawnPosition(spawnNodeKey);

			PublishCreatePlayerEvent(spawnNodeKey, spawnPosition);

			// 当前场景已经消费了这次切换上下文。
			// 清理后，避免下次直接重开当前场景时误用旧场景名。
			SceneTransitionContext.Clear();
		}

		/// <summary>
		/// 解析本次应该使用哪个出生节点名。
		/// </summary>
		/// <returns>
		/// 优先返回上一个场景名。
		/// 如果没有有效切换上下文，则返回 defaultSpawnNodeKey。
		/// </returns>
		private string ResolveSpawnNodeKey()
		{
			if (SceneTransitionContext.HasValidContext)
			{
				string currentSceneName = SceneManager.GetActiveScene().name;

				if (!string.Equals(currentSceneName, SceneTransitionContext.TargetSceneName))
				{
					Debug.LogWarning(
						$"当前场景 {currentSceneName} 与记录的目标场景 {SceneTransitionContext.TargetSceneName} 不一致。");
				}

				// 需求核心：
				// 用"上一个场景名"作为当前 RailMap2DAsset 的 nodeKey。
				Debug.Log($"上一个场景名为{SceneTransitionContext.PreviousSceneName}");
				return SceneTransitionContext.PreviousSceneName;
			}

			return defaultSpawnNodeKey;
		}

		/// <summary>
		/// 解析出生世界坐标。
		/// </summary>
		/// <param name="spawnNodeKey">
		/// 出生节点查询名。
		/// 它会被传给 RailMap2DAsset.TryGetNodePositionByKey。
		/// </param>
		/// <returns>
		/// 如果查到节点，返回节点位置。
		/// 如果查不到节点，返回 fallbackWorldPosition。
		/// </returns>
		private Vector2 ResolveSpawnPosition(string spawnNodeKey)
		{
			if (!string.IsNullOrWhiteSpace(spawnNodeKey) &&
				currentSceneRailMap.TryGetNodePositionByKey(spawnNodeKey, out Vector2 position))
			{
				Debug.Log($"当前Node 的pos为{position}");
				return position;
			}

			if (!string.IsNullOrWhiteSpace(defaultSpawnNodeKey) &&
				currentSceneRailMap.TryGetNodePositionByKey(defaultSpawnNodeKey, out Vector2 defaultPosition))
			{
				Debug.LogWarning(
					$"当前 RailMap 找不到节点 {spawnNodeKey}，改用默认节点 {defaultSpawnNodeKey}。");

				return defaultPosition;
			}

			Debug.LogWarning(
				$"当前 RailMap 找不到节点 {spawnNodeKey}，也找不到默认节点 {defaultSpawnNodeKey}，改用 fallbackWorldPosition。");

			return fallbackWorldPosition;
		}

		/// <summary>
		/// 发布创建 Player 事件。
		/// </summary>
		/// <param name="spawnNodeKey">
		/// 出生节点查询名。
		/// CharacterManager 会把它继续传给 Player 的 RailWalker2D。
		/// </param>
		/// <param name="spawnPosition">
		/// 兜底出生世界坐标。
		/// 当 RailWalker2D 无法按节点接入路径时使用。
		/// </param>
		private void PublishCreatePlayerEvent(string spawnNodeKey, Vector2 spawnPosition)
		{
			if (string.IsNullOrWhiteSpace(characterResourcePath))
			{
				Debug.LogError($"{nameof(OutsideScenePlayerSpawnBootstrap)} 缺少 characterResourcePath。");
				return;
			}
			Debug.Log($"创建玩家事件的Pos{spawnPosition}");

			CreatePlayerEvent createPlayerEvent = new CreatePlayerEvent
			{
				characterResourcePath = characterResourcePath,
				fallbackPos = spawnPosition,
				facingSign = defaultFacingSign,
				rail = currentSceneRailMap,
				spawnNodeKey = spawnNodeKey,
				preferredExitChoice = preferredExitChoice
			};

			EventBus.Publish(createPlayerEvent);
		}
	}
}