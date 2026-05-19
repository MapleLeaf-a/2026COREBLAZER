using Assets.Scripts.Tools.Common;
using Events;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.Manager
{
	/// <summary>
	/// 角色管理器。
	///
	/// 作用：
	/// 1. 接收 CreatePlayerEvent。
	/// 2. 根据 Resources 路径加载角色预制体。
	/// 3. 用 characterResourcePath 作为 key 缓存已经生成好的角色对象。
	/// 4. 关闭角色时不 Destroy，只 SetActive(false)。
	/// 5. 下次生成同 key 角色时直接复用。
	/// </summary>
	internal sealed class CharacterManager : MonoSingleton<CharacterManager>
	{
		/// <summary>
		/// 是否切换场景时保留 CharacterManager。
		///
		/// 作用：
		/// true 表示 CharacterManager 不会因为 LoadScene 被销毁。
		/// 这样角色缓存字典才能跨场景复用。
		/// </summary>
		protected override bool DontDestroyOnLoad => true;

		/// <summary>
		/// 已经实例化过的角色缓存。
		///
		/// key：
		/// CreatePlayerEvent.characterResourcePath。
		///
		/// value：
		/// 已经 Instantiate 出来的角色控制器。
		/// </summary>
		private readonly Dictionary<string, OutsideDoorCharacterController> characterCache =
			new Dictionary<string, OutsideDoorCharacterController>();

		/// <summary>
		/// 当前正在激活的角色缓存 key。
		///
		/// 作用：
		/// ClosePlayerEvent 没有指定 characterResourcePath 时，
		/// 用它关闭当前正在使用的角色。
		/// </summary>
		private string activeCharacterResourcePath;

		/// <summary>
		/// 当前正在激活的角色控制器。
		///
		/// 作用：
		/// 1. 生成新角色前关闭旧角色。
		/// 2. ClosePlayerEvent 没有指定 key 时关闭当前角色。
		/// </summary>
		private OutsideDoorCharacterController activeCharacter;

		public void Start()
		{
			EventBus.Subscribe<CreatePlayerEvent>(a => OnCreatePlayer(a));
			EventBus.Subscribe<ClosePlayerEvent>(a => OnClosePlayerView(a));
		}

		public void OnDestroy()
		{
			EventBus.Unsubscribe<CreatePlayerEvent>(a => OnCreatePlayer(a));
			EventBus.Unsubscribe<ClosePlayerEvent>(a => OnClosePlayerView(a));
		}

		/// <summary>
		/// 创建或复用角色。
		/// </summary>
		/// <param name="createPlayerEvent">
		/// 角色创建事件。
		///
		/// characterResourcePath：
		/// 角色预制体在 Resources 目录下的路径，也是缓存 key。
		///
		/// rail：
		/// 当前场景的 RailMap2DAsset。
		///
		/// spawnNodeKey：
		/// 当前场景 RailMap 中的出生节点 key。
		///
		/// fallbackPos：
		/// 找不到出生节点时使用的兜底坐标。
		///
		/// preferredExitChoice：
		/// 出生节点连接多条路径时的优先出口。
		/// </param>
		public void OnCreatePlayer(in CreatePlayerEvent createPlayerEvent)
		{
			if (string.IsNullOrWhiteSpace(createPlayerEvent.characterResourcePath))
			{
				Debug.LogError("CreatePlayerEvent.characterResourcePath 为空，无法创建角色。");
				return;
			}

			OutsideDoorCharacterController targetCharacter =
				GetOrCreateCharacter(createPlayerEvent.characterResourcePath);

			if (targetCharacter == null)
			{
				return;
			}

			SwitchActiveCharacter(
				createPlayerEvent.characterResourcePath,
				targetCharacter);

			targetCharacter.ResetRailMapAndSpawnAtNode(
				createPlayerEvent.rail,
				createPlayerEvent.spawnNodeKey,
				createPlayerEvent.fallbackPos,
				createPlayerEvent.preferredExitChoice,
				createPlayerEvent.facingSign > 0f);

			targetCharacter.enabled = true;
		}

		/// <summary>
		/// 关闭角色显示与逻辑。
		/// </summary>
		/// <param name="closePlayerEvent">
		/// 关闭角色事件。
		///
		/// characterResourcePath：
		/// 如果不为空，则关闭指定 key 对应的缓存角色。
		/// 如果为空，则关闭当前正在激活的角色。
		/// </param>
		public void OnClosePlayerView(in ClosePlayerEvent closePlayerEvent)
		{
			if (!string.IsNullOrWhiteSpace(closePlayerEvent.characterResourcePath))
			{
				CloseCharacterByKey(closePlayerEvent.characterResourcePath);
				return;
			}

			CloseActiveCharacter();
		}

		/// <summary>
		/// 根据 Resources 路径获取或创建角色。
		/// </summary>
		/// <param name="characterResourcePath">
		/// 角色预制体在 Resources 目录下的路径。
		/// 同时也是 characterCache 的 key。
		/// </param>
		/// <returns>
		/// 如果成功，返回角色控制器。
		/// 如果加载失败、预制体为空、预制体缺组件，则返回 null。
		/// </returns>
		private OutsideDoorCharacterController GetOrCreateCharacter(string characterResourcePath)
		{
			if (characterCache.TryGetValue(characterResourcePath, out OutsideDoorCharacterController cachedCharacter))
			{
				if (cachedCharacter != null)
				{
					return cachedCharacter;
				}

				characterCache.Remove(characterResourcePath);
			}

			GameObject prefab = Resources.Load<GameObject>(characterResourcePath);

			if (prefab == null)
			{
				Debug.LogError(
					$"无法从 Resources 加载角色预制体：{characterResourcePath}。" +
					"请确认预制体位于 Assets/Resources 下，并且路径不包含 Resources 和 .prefab。");

				return null;
			}

			GameObject instance = Instantiate(prefab);
			instance.name = $"CachedCharacter_{characterResourcePath.Replace('/', '_')}";

			DontDestroyOnLoad(instance);

			OutsideDoorCharacterController controller =
				instance.GetComponent<OutsideDoorCharacterController>();

			if (controller == null)
			{
				Debug.LogError(
					$"角色预制体 {characterResourcePath} 上缺少 OutsideDoorCharacterController，无法作为户外角色使用。");

				Destroy(instance);
				return null;
			}

			instance.SetActive(false);
			controller.enabled = false;

			characterCache.Add(characterResourcePath, controller);

			return controller;
		}

		/// <summary>
		/// 切换当前激活角色。
		/// </summary>
		/// <param name="characterResourcePath">
		/// 即将激活的角色缓存 key。
		/// </param>
		/// <param name="targetCharacter">
		/// 即将激活的角色控制器。
		/// </param>
		private void SwitchActiveCharacter(
			string characterResourcePath,
			OutsideDoorCharacterController targetCharacter)
		{
			if (activeCharacter != null && activeCharacter != targetCharacter)
			{
				activeCharacter.enabled = false;
				activeCharacter.gameObject.SetActive(false);
			}

			activeCharacterResourcePath = characterResourcePath;
			activeCharacter = targetCharacter;

			targetCharacter.gameObject.SetActive(true);
		}

		/// <summary>
		/// 关闭当前激活角色。
		/// </summary>
		private void CloseActiveCharacter()
		{
			if (activeCharacter == null)
			{
				Debug.LogWarning("当前没有正在激活的角色，不需要关闭。");
				return;
			}

			activeCharacter.enabled = false;
			activeCharacter.gameObject.SetActive(false);

			activeCharacter = null;
			activeCharacterResourcePath = string.Empty;
		}

		/// <summary>
		/// 根据缓存 key 关闭指定角色。
		/// </summary>
		/// <param name="characterResourcePath">
		/// 要关闭的角色缓存 key。
		/// </param>
		private void CloseCharacterByKey(string characterResourcePath)
		{
			if (!characterCache.TryGetValue(characterResourcePath, out OutsideDoorCharacterController targetCharacter))
			{
				Debug.LogWarning($"角色缓存中不存在 key：{characterResourcePath}。");
				return;
			}

			if (targetCharacter == null)
			{
				characterCache.Remove(characterResourcePath);
				return;
			}

			targetCharacter.enabled = false;
			targetCharacter.gameObject.SetActive(false);

			if (activeCharacter == targetCharacter)
			{
				activeCharacter = null;
				activeCharacterResourcePath = string.Empty;
			}
		}
	}
}