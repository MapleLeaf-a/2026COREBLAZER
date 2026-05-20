using Assets.Scripts.OutsideDoorScene.OutsideManager.Manager;
using Assets.Scripts.Tools.Common;
using Events;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Tools.Unity
{
	/// <summary>
	/// 事件驱动版场景管理器。
	///
	/// 它不要求外部直接调用 LoadScene 方法。
	/// 外部只需要通过 EventBus 发布 SceneLoadRequestEvent。
	/// </summary>
	public sealed class GameSceneManager : MonoSingleton<GameSceneManager>
	{
		/// <summary>
		/// 当前等待手动激活的异步加载操作。
		///
		/// 当 SceneLoadRequestEvent.AllowSceneActivation 为 false 时，
		/// Unity 会把加载进度停在 90% 左右。
		/// 这时需要保存 AsyncOperation，
		/// 等收到 SceneActivateRequestEvent 后再允许它激活。
		/// </summary>
		private AsyncOperation _pendingActivationOperation;

		/// <summary>
		/// 当前等待激活的请求编号。
		///
		/// 用来判断 SceneActivateRequestEvent 是否对应当前这次加载。
		/// </summary>
		private string _pendingActivationRequestId;

		/// <summary>
		/// 当前正在运行的加载协程。
		///
		/// Coroutine：
		/// Unity 协程对象。
		/// 用来把一个耗时流程拆成多帧执行，避免一帧卡死。
		/// </summary>
		private Coroutine _loadingCoroutine;

		private void Awake()
		{
			// 让场景管理器在切换场景时不被销毁。
			// 否则 Single 模式加载新场景时，这个物体会被卸载。
			DontDestroyOnLoad(gameObject);
		}

		private void OnEnable()
		{
			// 注册场景加载请求事件。
			// 外部发布 SceneLoadRequestEvent 后，会调用 OnSceneLoadRequest。
			EventBus.Subscribe<SceneLoadRequestEvent>(OnSceneLoadRequest);

			// 注册手动激活场景请求事件。
			// 外部发布 SceneActivateRequestEvent 后，会调用 OnSceneActivateRequest。
			EventBus.Subscribe<SceneActivateRequestEvent>(OnSceneActivateRequest);

			// 注册场景卸载请求事件。
			// 外部发布 SceneUnloadRequestEvent 后，会调用 OnSceneUnloadRequest。
			EventBus.Subscribe<SceneUnloadRequestEvent>(OnSceneUnloadRequest);
		}

		private void OnDisable()
		{
			// 取消注册场景加载请求事件。
			// 必须和 OnEnable 中注册的方法保持一致。
			EventBus.Unsubscribe<SceneLoadRequestEvent>(OnSceneLoadRequest);

			// 取消注册手动激活场景请求事件。
			EventBus.Unsubscribe<SceneActivateRequestEvent>(OnSceneActivateRequest);

			// 取消注册场景卸载请求事件。
			EventBus.Unsubscribe<SceneUnloadRequestEvent>(OnSceneUnloadRequest);
		}

		/// <summary>
		/// 接收场景加载请求。
		/// </summary>
		/// <param name="eventData">
		/// 场景加载请求事件。
		/// 包含目标场景名、加载模式、是否使用 Loading 场景等信息。
		/// </param>
		private void OnSceneLoadRequest(SceneLoadRequestEvent eventData)
		{
			if (string.IsNullOrWhiteSpace(eventData.TargetSceneName))
			{
				PublishLoadFailed(
					eventData.RequestId,
					eventData.TargetSceneName,
					"目标场景名为空。");
				return;
			}

			if (!Application.CanStreamedLevelBeLoaded(eventData.TargetSceneName))
			{
				PublishLoadFailed(
					eventData.RequestId,
					eventData.TargetSceneName,
					$"目标场景没有加入 Build Settings：{eventData.TargetSceneName}");
				return;
			}

			if (eventData.UseLoadingScene &&
				!Application.CanStreamedLevelBeLoaded(eventData.LoadingSceneName))
			{
				PublishLoadFailed(
					eventData.RequestId,
					eventData.TargetSceneName,
					$"Loading 场景没有加入 Build Settings：{eventData.LoadingSceneName}");
				return;
			}

			_loadingCoroutine = StartCoroutine(LoadSceneRoutine(eventData));
		}

		/// <summary>
		/// 接收手动激活场景请求。
		/// </summary>
		/// <param name="eventData">
		/// 手动激活场景事件。
		/// RequestId 应该和之前的 SceneLoadRequestEvent.RequestId 一致。
		/// </param>
		private void OnSceneActivateRequest(SceneActivateRequestEvent eventData)
		{
			if (_pendingActivationOperation == null)
			{
				Debug.LogWarning("[GameSceneManager] 当前没有等待激活的场景。");
				return;
			}

			if (_pendingActivationRequestId != eventData.RequestId)
			{
				Debug.LogWarning(
					$"[GameSceneManager] 激活请求编号不匹配。当前等待：{_pendingActivationRequestId}，收到：{eventData.RequestId}");

				return;
			}

			// 允许 Unity 激活已经加载到 90% 的场景。
			_pendingActivationOperation.allowSceneActivation = true;
		}

		/// <summary>
		/// 接收场景卸载请求。
		/// </summary>
		/// <param name="eventData">
		/// 场景卸载请求事件。
		/// 包含要卸载的场景名。
		/// </param>
		private void OnSceneUnloadRequest(SceneUnloadRequestEvent eventData)
		{
			if (string.IsNullOrWhiteSpace(eventData.SceneName))
			{
				Debug.LogError("[GameSceneManager] 卸载失败：场景名为空。");
				return;
			}

			Scene scene =
				SceneManager.GetSceneByName(eventData.SceneName);

			if (!scene.IsValid() || !scene.isLoaded)
			{
				Debug.LogWarning($"[GameSceneManager] 卸载失败，场景未加载：{eventData.SceneName}");
				return;
			}

			if (SceneManager.sceneCount <= 1)
			{
				Debug.LogError($"[GameSceneManager] 不能卸载最后一个场景：{eventData.SceneName}");
				return;
			}

			StartCoroutine(UnloadSceneRoutine(eventData));
		}

		/// <summary>
		/// 场景加载主流程。
		/// </summary>
		/// <param name="request">
		/// 场景加载请求事件。
		/// </param>
		private IEnumerator LoadSceneRoutine(SceneLoadRequestEvent request)
		{
			if (IsSceneLoaded(request.TargetSceneName))
			{
				bool shouldContinue = HandleDuplicateScene(request);

				if (!shouldContinue)
				{
					_loadingCoroutine = null;
					yield break;
				}
			}

			if (request.UseLoadingScene)
			{
				yield return LoadSceneInternalRoutine(
					requestId: request.RequestId,
					sceneName: request.LoadingSceneName,
					loadMode: GameSceneLoadMode.Single,
					setActiveSceneAfterLoaded: true,
					allowSceneActivation: true);

				// 等一帧，保证 Loading 场景里的 UI 有机会执行 Awake / OnEnable / Start。
				yield return null;
			}

			yield return LoadSceneInternalRoutine(
				requestId: request.RequestId,
				sceneName: request.TargetSceneName,
				loadMode: request.LoadMode,
				setActiveSceneAfterLoaded: request.SetActiveSceneAfterLoaded,
				allowSceneActivation: request.AllowSceneActivation);

			_loadingCoroutine = null;
			_pendingActivationOperation = null;
			_pendingActivationRequestId = null;

			var completedEvent = new SceneLoadCompletedEvent(
				requestId: request.RequestId,
				sceneName: request.TargetSceneName);

			// 使用 TryPublish，避免没有完成事件监听者时抛异常。
			EventBus.Publish(in completedEvent);
		}

		/// <summary>
		/// 执行具体的 Unity 场景异步加载。
		/// </summary>
		/// <param name="requestId">
		/// 请求编号。
		/// 用于发布进度事件、完成事件、失败事件。
		/// </param>
		/// <param name="sceneName">
		/// 要加载的场景名。
		/// 必须存在于 Build Settings 中。
		/// </param>
		/// <param name="loadMode">
		/// 加载模式。
		/// Single 替换当前场景。
		/// Additive 叠加新场景。
		/// </param>
		/// <param name="setActiveSceneAfterLoaded">
		/// 加载完成后是否把目标场景设为 Active Scene。
		/// </param>
		/// <param name="allowSceneActivation">
		/// 是否允许加载完成后自动激活场景。
		/// false 时需要等待 SceneActivateRequestEvent。
		/// </param>
		private IEnumerator LoadSceneInternalRoutine(
			string requestId,
			string sceneName,
			GameSceneLoadMode loadMode,
			bool setActiveSceneAfterLoaded,
			bool allowSceneActivation)
		{
			LoadSceneMode unityLoadMode = loadMode == GameSceneLoadMode.Single
				? LoadSceneMode.Single
				: LoadSceneMode.Additive;

			AsyncOperation operation =
				SceneManager.LoadSceneAsync(sceneName, unityLoadMode);

			if (operation == null)
			{
				PublishLoadFailed(
					requestId,
					sceneName,
					$"LoadSceneAsync 返回 null：{sceneName}");

				yield break;
			}

			operation.allowSceneActivation = allowSceneActivation;

			while (!operation.isDone)
			{
				float progress = Mathf.Clamp01(operation.progress / 0.9f);

				var progressEvent = new SceneLoadProgressEvent(
					requestId: requestId,
					sceneName: sceneName,
					progress: progress);

				EventBus.Publish(in progressEvent);

				if (!allowSceneActivation && operation.progress >= 0.9f)
				{
					_pendingActivationOperation = operation;
					_pendingActivationRequestId = requestId;
				}

				yield return null;
			}

			var finalProgressEvent = new SceneLoadProgressEvent(
				requestId: requestId,
				sceneName: sceneName,
				progress: 1f);

			EventBus.Publish(in finalProgressEvent);

			if (setActiveSceneAfterLoaded)
			{
				UnityEngine.SceneManagement.Scene loadedScene =
					SceneManager.GetSceneByName(sceneName);

				if (loadedScene.IsValid() && loadedScene.isLoaded)
				{
					SceneManager.SetActiveScene(loadedScene);
				}
				else
				{
					Debug.LogWarning($"[GameSceneManager] 无法设置 Active Scene：{sceneName}");
				}
			}
		}

		/// <summary>
		/// 场景卸载流程。
		/// </summary>
		/// <param name="request">
		/// 场景卸载请求事件。
		/// </param>
		private IEnumerator UnloadSceneRoutine(SceneUnloadRequestEvent request)
		{
			AsyncOperation operation =
				SceneManager.UnloadSceneAsync(request.SceneName);

			if (operation == null)
			{
				Debug.LogError($"[GameSceneManager] UnloadSceneAsync 返回 null：{request.SceneName}");
				yield break;
			}

			while (!operation.isDone)
			{
				yield return null;
			}

			var completedEvent = new SceneUnloadCompletedEvent(
				requestId: request.RequestId,
				sceneName: request.SceneName);

			EventBus.Publish(in completedEvent);
		}

		/// <summary>
		/// 处理重复加载场景。
		/// </summary>
		/// <param name="request">
		/// 当前加载请求。
		/// </param>
		/// <returns>
		/// true：继续执行加载流程。
		/// false：终止加载流程。
		/// </returns>
		private bool HandleDuplicateScene(SceneLoadRequestEvent request)
		{
			switch (request.DuplicatePolicy)
			{
				case DuplicateScenePolicy.Ignore:
					{
						var completedEvent = new SceneLoadCompletedEvent(
							requestId: request.RequestId,
							sceneName: request.TargetSceneName);

						EventBus.Publish(in completedEvent);
						return false;
					}

				case DuplicateScenePolicy.Reload:
					{
						return true;
					}

				case DuplicateScenePolicy.Fail:
					{
						PublishLoadFailed(
							request.RequestId,
							request.TargetSceneName,
							$"目标场景已经加载：{request.TargetSceneName}");

						return false;
					}

				default:
					{
						PublishLoadFailed(
							request.RequestId,
							request.TargetSceneName,
							$"未知重复加载策略：{request.DuplicatePolicy}");

						return false;
					}
			}
		}

		/// <summary>
		/// 判断场景是否已经加载。
		/// </summary>
		/// <param name="sceneName">
		/// 要检查的场景名。
		/// </param>
		/// <returns>
		/// true：场景已经加载。
		/// false：场景没有加载。
		/// </returns>
		private bool IsSceneLoaded(string sceneName)
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				UnityEngine.SceneManagement.Scene scene =
					SceneManager.GetSceneAt(i);

				if (scene.name == sceneName && scene.isLoaded)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 发布场景加载失败事件。
		/// </summary>
		/// <param name="requestId">
		/// 请求编号。
		/// </param>
		/// <param name="sceneName">
		/// 加载失败的场景名。
		/// </param>
		/// <param name="errorMessage">
		/// 失败原因。
		/// </param>
		private void PublishLoadFailed(
			string requestId,
			string sceneName,
			string errorMessage)
		{
			Debug.LogError($"[GameSceneManager] {errorMessage}");

			var failedEvent = new SceneLoadFailedEvent(
				requestId: requestId,
				sceneName: sceneName,
				errorMessage: errorMessage);

			EventBus.Publish(in failedEvent);
		}
	}
}