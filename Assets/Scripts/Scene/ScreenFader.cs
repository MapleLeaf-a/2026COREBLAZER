using System.Collections;
using UnityEngine;

namespace GameScene
{
	/// <summary>
	/// 屏幕黑幕控制器。
	///
	/// 该组件需要挂在全屏黑色 UI 图片所在对象上。
	/// 同一个对象上还需要挂 CanvasGroup。
	///
	/// CanvasGroup 是 Unity UI 的透明度控制组件。
	/// alpha = 0 表示完全透明。
	/// alpha = 1 表示完全黑屏。
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CanvasGroup))]
	public sealed class ScreenFader : MonoBehaviour
	{
		/// <summary>
		/// 场景刚加载出来时的黑幕透明度。
		///
		/// 这里默认设置为 1。
		/// 这代表新场景一加载出来时先保持全黑。
		/// 这样可以避免玩家看到场景加载完成瞬间的画面闪烁。
		/// </summary>
		[SerializeField]
		private float startAlpha = 1f;

		/// <summary>
		/// 是否在当前场景启动时自动执行黑屏淡出。
		///
		/// true 表示当前场景加载完成后，黑幕自动从全黑变成透明。
		/// false 表示当前场景不会自动淡出，需要其他脚本手动调用 FadeBlackOut。
		/// </summary>
		[SerializeField]
		private bool autoFadeBlackOutOnStart = true;

		/// <summary>
		/// 切入当前场景时的黑屏淡出时间。
		///
		/// 黑屏淡出是指：
		/// 黑幕从 alpha = 1 逐渐变成 alpha = 0。
		/// 也就是从黑屏恢复到正常画面。
		/// </summary>
		[SerializeField]
		private float enterFadeOutDuration = 0.35f;

		/// <summary>
		/// 当前对象上的 CanvasGroup。
		/// 用于控制整张黑幕 UI 的透明度和射线阻挡状态。
		/// </summary>
		private CanvasGroup canvasGroup;

		private void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();

			// 当前场景刚加载时，直接设置黑幕初始透明度。
			// startAlpha 默认为 1，因此场景刚加载出来会先保持黑屏。
			canvasGroup.alpha = startAlpha;

			// 黑幕可见时阻挡点击。
			// 这样可以避免玩家在黑屏过程中误点按钮或误触发交互。
			canvasGroup.blocksRaycasts = startAlpha > 0f;
		}

		private IEnumerator Start()
		{
			if (!autoFadeBlackOutOnStart)
			{
				yield break;
			}

			// 切入当前场景时，当前场景自己的黑幕负责淡出。
			// 也就是从全黑恢复到正常画面。
			yield return FadeBlackOut(enterFadeOutDuration);
		}

		/// <summary>
		/// 黑屏淡入。
		///
		/// 用于切出当前场景。
		/// 当前场景离开前，黑幕从透明变成全黑。
		/// </summary>
		/// <param name="duration">
		/// 淡入黑屏所需时间，单位是秒。
		/// 数值越大，变黑越慢。
		/// </param>
		public IEnumerator FadeBlackIn(float duration)
		{
			yield return FadeTo(1f, duration);
		}

		/// <summary>
		/// 黑屏淡出。
		///
		/// 用于切入目标场景。
		/// 目标场景加载后，黑幕从全黑变成透明。
		/// </summary>
		/// <param name="duration">
		/// 淡出黑屏所需时间，单位是秒。
		/// 数值越大，恢复画面越慢。
		/// </param>
		public IEnumerator FadeBlackOut(float duration)
		{
			yield return FadeTo(0f, duration);
		}

		private IEnumerator FadeTo(float targetAlpha, float duration)
		{
			float beginAlpha = canvasGroup.alpha;
			float elapsedTime = 0f;

			// 黑幕动画播放期间阻挡点击。
			// 这能防止玩家在切场景过程中继续操作 UI。
			canvasGroup.blocksRaycasts = true;

			if (duration <= 0f)
			{
				canvasGroup.alpha = targetAlpha;
				canvasGroup.blocksRaycasts = targetAlpha > 0f;
				yield break;
			}

			while (elapsedTime < duration)
			{
				// unscaledDeltaTime 不受 Time.timeScale 影响。
				// 即使游戏暂停或时间缩放，黑屏动画也能正常播放。
				elapsedTime += Time.unscaledDeltaTime;

				// progress 表示动画进度。
				// 0 表示刚开始，1 表示完成。
				float progress = Mathf.Clamp01(elapsedTime / duration);

				// Lerp 是线性插值。
				// 它会根据 progress 在 beginAlpha 和 targetAlpha 之间取一个中间值。
				canvasGroup.alpha = Mathf.Lerp(beginAlpha, targetAlpha, progress);

				yield return null;
			}

			canvasGroup.alpha = targetAlpha;

			// 黑幕完全透明后，不再阻挡点击。
			// 黑幕不透明时，继续阻挡点击。
			canvasGroup.blocksRaycasts = targetAlpha > 0f;
		}
	}
}
