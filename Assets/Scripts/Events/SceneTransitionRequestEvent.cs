using UnityEngine;

namespace Events
{
	/// <summary>
	/// 场景切换请求事件。
	///
	/// 这个事件由场景触发盒发布，由 SceneTransitionManager 监听。
	/// 它只负责携带场景切换需要的数据，不直接执行切场景逻辑。
	/// </summary>
	public readonly struct SceneTransitionRequestEvent
	{
		/// <summary>
		/// 要加载的目标场景名称。
		/// 这个名称必须和 Unity Build Settings 中的场景名称一致。
		/// </summary>
		public readonly string TargetSceneName;

		/// <summary>
		/// 触发场景切换的玩家对象。
		/// 当前版本主要用于调试和后续扩展，例如新场景出生点定位。
		/// </summary>
		public readonly GameObject Player;

		/// <summary>
		/// 黑屏淡出时间，单位是秒。
		/// 淡出是指画面从正常显示逐渐变成黑屏。
		/// </summary>
		public readonly float FadeOutDuration;

		/// <summary>
		/// 黑屏淡入时间，单位是秒。
		/// 淡入是指画面从黑屏逐渐恢复到正常显示。
		/// </summary>
		public readonly float FadeInDuration;

		/// <summary>
		/// 创建一个场景切换请求事件。
		/// </summary>
		/// <param name="targetSceneName">
		/// 目标场景名称。
		/// 必须填写已经加入 Build Settings 的场景名称。
		/// </param>
		/// <param name="player">
		/// 触发切换的玩家 GameObject。
		/// 用于后续扩展和排查问题。
		/// </param>
		/// <param name="fadeOutDuration">
		/// 黑屏淡出时间，单位是秒。
		/// 数值越大，画面变黑越慢。
		/// </param>
		/// <param name="fadeInDuration">
		/// 黑屏淡入时间，单位是秒。
		/// 数值越大，黑屏恢复正常画面越慢。
		/// </param>
		public SceneTransitionRequestEvent(
			string targetSceneName,
			GameObject player,
			float fadeOutDuration,
			float fadeInDuration)
		{
			TargetSceneName = targetSceneName;
			Player = player;
			FadeOutDuration = fadeOutDuration;
			FadeInDuration = fadeInDuration;
		}
	}
}
