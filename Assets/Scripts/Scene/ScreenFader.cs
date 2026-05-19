using System.Collections;
using UnityEngine;

namespace GameScene
{
	/// <summary>
	/// 屏幕黑屏淡入淡出组件。
	///
	/// 这个脚本应该挂在全屏黑色 UI 节点上。
	/// 它通过 CanvasGroup.alpha 控制黑色遮罩的透明度。
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CanvasGroup))]
	public sealed class ScreenFader : MonoBehaviour
	{
		/// <summary>
		/// 控制 UI 透明度和交互阻挡的组件。
		/// alpha 为 0 表示完全透明，alpha 为 1 表示完全黑屏。
		/// </summary>
		private CanvasGroup canvasGroup;

		private void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();

			// 默认进入游戏时不显示黑屏。
			canvasGroup.alpha = 0f;

			// 没有黑屏时不阻挡鼠标或触摸输入。
			canvasGroup.blocksRaycasts = false;
		}

		/// <summary>
		/// 淡出到黑屏。
		/// </summary>
		/// <param name="duration">
		/// 淡出持续时间，单位是秒。
		/// </param>
		public IEnumerator FadeOut(float duration)
		{
			yield return FadeTo(1f, duration);
		}

		/// <summary>
		/// 从黑屏淡入到正常画面。
		/// </summary>
		/// <param name="duration">
		/// 淡入持续时间，单位是秒。
		/// </param>
		public IEnumerator FadeIn(float duration)
		{
			yield return FadeTo(0f, duration);
		}

		private IEnumerator FadeTo(float targetAlpha, float duration)
		{
			float startAlpha = canvasGroup.alpha;
			float elapsedTime = 0f;

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
				// 即使游戏暂停或慢动作，黑屏动画也可以正常播放。
				elapsedTime += Time.unscaledDeltaTime;

				float progress = Mathf.Clamp01(elapsedTime / duration);
				canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

				yield return null;
			}

			canvasGroup.alpha = targetAlpha;
			canvasGroup.blocksRaycasts = targetAlpha > 0f;
		}
	}
}
