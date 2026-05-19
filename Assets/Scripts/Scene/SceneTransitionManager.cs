using Events;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameScene
{
	/// <summary>
	/// 全局场景切换管理器。
	///
	/// 它监听 SceneTransitionRequestEvent。
	/// 收到事件后，先黑屏淡出，再加载目标场景，最后黑屏淡入。
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class SceneTransitionManager : MonoBehaviour
	{
		/// <summary>
		/// 黑屏淡入淡出组件。
		/// 需要在 Inspector 中绑定场景里的 ScreenFader。
		/// </summary>
		[SerializeField]
		private ScreenFader screenFader;

		/// <summary>
		/// 是否在切换场景后保留当前管理器对象。
		/// 如果这个管理器作为全局对象使用，建议开启。
		/// </summary>
		[SerializeField]
		private bool dontDestroyOnLoad = true;

		/// <summary>
		/// 当前是否正在切换场景。
		/// 用于防止多个触发盒在短时间内重复请求切换。
		/// </summary>
		private bool isTransitioning;

		private void Awake()
		{
			if (dontDestroyOnLoad)
			{
				DontDestroyOnLoad(gameObject);
			}
		}

		private void OnEnable()
		{
			// 注册事件监听。
			// 当 SceneTransitionTrigger2D 发布请求时，会调用 HandleSceneTransitionRequest。
			EventBus.Subscribe<SceneTransitionRequestEvent>(HandleSceneTransitionRequest);
		}

		private void OnDisable()
		{
			// 取消事件监听。
			// 避免对象销毁后，EventBus 里还保留旧回调。
			EventBus.Unsubscribe<SceneTransitionRequestEvent>(HandleSceneTransitionRequest);
		}

		private void HandleSceneTransitionRequest(SceneTransitionRequestEvent eventData)
		{
			if (isTransitioning)
			{
				return;
			}

			StartCoroutine(TransitionCoroutine(eventData));
		}

		private IEnumerator TransitionCoroutine(SceneTransitionRequestEvent eventData)
		{
			isTransitioning = true;

			if (screenFader != null)
			{
				yield return screenFader.FadeOut(eventData.FadeOutDuration);
			}

			AsyncOperation loadOperation = new AsyncOperation();
			try
			{
				loadOperation = SceneManager.LoadSceneAsync(eventData.TargetSceneName);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			if (loadOperation == null)
			{
				Debug.LogError($"Failed to load scene: {eventData.TargetSceneName}");
				isTransitioning = false;
				yield break;
			}

			while (!loadOperation.isDone)
			{
				yield return null;
			}

			if (screenFader != null)
			{
				yield return screenFader.FadeIn(eventData.FadeInDuration);
			}

			isTransitioning = false;
		}
	}
}