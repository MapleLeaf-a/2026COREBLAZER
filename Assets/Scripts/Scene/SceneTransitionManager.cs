using Assets.Scripts.OutsideDoorScene.OutsideManager.Manager;
using Events;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameScene
{
	/// <summary>
	/// 当前场景的场景切换管理器。
	///
	/// 每个场景都应该有自己的 SceneTransitionManager。
	/// 它不跨场景保留。
	///
	/// 当前场景切出时：
	/// 1. 监听 SceneTransitionRequestEvent。
	/// 2. 执行当前场景 ScreenFader.FadeBlackIn。
	/// 3. 当前场景完全黑屏后，调用 Unity SceneManager 加载目标场景。
	///
	/// 目标场景切入时：
	/// 由目标场景自己的 ScreenFader 在 Start 中自动执行 FadeBlackOut。
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class SceneTransitionManager : MonoBehaviour
	{
		/// <summary>
		/// 当前场景里的黑幕控制器。
		///
		/// 必须绑定当前场景自己的 ScreenFader。
		/// 不要绑定其他场景的对象。
		/// </summary>
		[SerializeField]
		private ScreenFader screenFader;

		/// <summary>
		/// 默认切出场景时的黑屏淡入时间。
		///
		/// 如果事件里没有设置有效时间，就使用这个默认值。
		/// </summary>
		[SerializeField]
		private float defaultExitFadeInDuration = 0.35f;

		/// <summary>
		/// 是否正在切换场景。
		///
		/// 用于防止玩家连续触发多个出口，
		/// 导致重复执行场景加载。
		/// </summary>
		private bool isTransitioning;

		private void OnEnable()
		{
			// 注册场景切换请求事件。
			// 当前场景中的任意触发盒发布事件后，这里都会收到。
			EventBus.Subscribe<SceneTransitionRequestEvent>(HandleSceneTransitionRequest);
		}

		private void OnDisable()
		{
			// 取消事件注册。
			// 避免当前场景对象销毁后，EventBus 仍然保存旧回调。
			EventBus.Unsubscribe<SceneTransitionRequestEvent>(HandleSceneTransitionRequest);
		}

		private void HandleSceneTransitionRequest(SceneTransitionRequestEvent eventData)
		{
			if (isTransitioning)
			{
				return;
			}

			SceneTransitionContext.RecordTransition(eventData.TargetSceneName);

			StartCoroutine(TransitionOutCoroutine(eventData));
		}

		private IEnumerator TransitionOutCoroutine(SceneTransitionRequestEvent eventData)
		{
			isTransitioning = true;

			if (string.IsNullOrWhiteSpace(eventData.TargetSceneName))
			{
				Debug.LogError("Scene transition failed: target scene name is empty.");
				isTransitioning = false;
				yield break;
			}

			float fadeInDuration = eventData.ExitFadeInDuration > 0f
				? eventData.ExitFadeInDuration
				: defaultExitFadeInDuration;

			if (screenFader != null)
			{
				// 切出当前场景时，当前场景自己的黑幕负责淡入。
				// 也就是从正常画面逐渐变成全黑。
				yield return screenFader.FadeBlackIn(fadeInDuration);
			}

			// LoadSceneAsync 是 Unity 的异步场景加载接口。
			// 异步加载不会让主线程直接卡死，更适合配合黑屏过渡。
			AsyncOperation loadOperation = SceneManager.LoadSceneAsync(eventData.TargetSceneName);

			if (loadOperation == null)
			{
				Debug.LogError($"Scene transition failed: cannot load scene {eventData.TargetSceneName}.");
				isTransitioning = false;
				yield break;
			}

			while (!loadOperation.isDone)
			{
				yield return null;
			}
		}
	}
}