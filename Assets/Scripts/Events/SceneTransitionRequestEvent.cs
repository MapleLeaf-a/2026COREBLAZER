using UnityEngine;

namespace Events
{
	/// <summary>
	/// 场景切换请求事件。
	///
	/// 这个事件由场景出口触发盒发布。
	/// SceneTransitionManager 监听该事件后，负责执行黑屏和场景加载。
	///
	/// 当前项目的 EventBus 使用 struct 事件。
	/// 所以这里使用 readonly struct，避免事件数据在传递过程中被误修改。
	/// </summary>
	public readonly struct SceneTransitionRequestEvent
	{
		/// <summary>
		/// 目标场景名称。
		///
		/// 这个名称必须和 Unity Build Settings 中的场景名称一致。
		/// 例如场景文件是 OutsideDoor_2.unity，那么这里填写 OutsideDoor_2。
		/// </summary>
		public readonly string TargetSceneName;

		/// <summary>
		/// 触发场景切换的玩家对象。
		///
		/// 当前版本中它主要用于保留上下文。
		/// 后续如果要做出生点定位、玩家状态保存、玩家朝向恢复，可以继续使用该字段扩展。
		/// </summary>
		public readonly GameObject Player;

		/// <summary>
		/// 切出当前场景时的黑屏淡入时间。
		///
		/// 黑屏淡入是指：
		/// 当前场景离开前，黑幕从透明逐渐变成全黑。
		/// </summary>
		public readonly float ExitFadeInDuration;

		/// <summary>
		/// 创建场景切换请求事件。
		/// </summary>
		/// <param name="targetSceneName">
		/// 目标场景名称。
		/// 该名称必须已经加入 Unity Build Settings。
		/// </param>
		/// <param name="player">
		/// 触发场景切换的玩家对象。
		/// 通常传入进入触发盒的玩家 GameObject。
		/// </param>
		/// <param name="exitFadeInDuration">
		/// 切出当前场景时的黑屏淡入时间，单位是秒。
		/// 数值越大，变黑过程越慢。
		/// </param>
		public SceneTransitionRequestEvent(
			string targetSceneName,
			GameObject player,
			float exitFadeInDuration)
		{
			TargetSceneName = targetSceneName;
			Player = player;
			ExitFadeInDuration = exitFadeInDuration;
		}
	}
}
