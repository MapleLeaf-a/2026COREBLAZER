using Assets.Scripts.Tools.Common;
using Events;
using System;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.Manager
{
	/// <summary>
	/// 天数管理器。
	///
	/// 作用：
	/// 1. 管理游戏内天数计数。
	/// 2. 响应 AdvanceDayEvent 推进天数。
	/// 3. 响应 ResetDaysEvent 重置天数。
	/// 4. 天数变化时发布 SyncDaysDisplay 通知 UI 更新。
	///
	/// 使用方式：
	/// - 挂载到场景中的常驻 GameObject 上。
	/// - 其他系统通过 EventBus.Publish(new AdvanceDayEvent()) 推进天数。
	/// - UI 层订阅 SyncDaysDisplay 获取最新天数。
	///
	/// 设计说明：
	/// - 遵循 OutsideDoorCharacterPowerManager 的事件驱动模式。
	/// - 天数逻辑集中在 Manager，UI 只负责显示，不负责计算。
	/// </summary>
	public class DaysManager : MonoSingleton<DaysManager>
	{
		/// <summary>
		/// 当前天数。
		///
		/// 默认从 1 开始，表示游戏第一天。
		/// </summary>
		[Header("Days Settings")]
		[SerializeField]
		private int currentDay = 1;

		/// <summary>
		/// 最大天数限制。
		///
		/// 作用：
		/// - 防止天数无限增长。
		/// - 设为 0 表示无限制。
		/// </summary>
		[SerializeField]
		private int maxDay = 0;

		/// <summary>
		/// 初始天数。
		///
		/// 作用：
		/// - 重置天数时恢复到此值。
		/// - 新游戏开始时的天数。
		/// </summary>
		[SerializeField]
		private int initialDay = 1;

		/// <summary>
		/// 获取当前天数（只读）。
		/// </summary>
		public int CurrentDay => currentDay;

		/// <summary>
		/// 获取最大天数限制（只读）。
		/// 0 表示无限制。
		/// </summary>
		public int MaxDay => maxDay;

		private void OnEnable()
		{
			EventBus.Subscribe<AdvanceDayEvent>(HandleAdvanceDay);
			EventBus.Subscribe<ResetDaysEvent>(HandleResetDays);

			// 初始化时同步一次 UI，确保 UI 显示正确。
			SyncUI();
		}

		private void OnDisable()
		{
			EventBus.Unsubscribe<AdvanceDayEvent>(HandleAdvanceDay);
			EventBus.Unsubscribe<ResetDaysEvent>(HandleResetDays);
		}

		/// <summary>
		/// 处理推进天数事件。
		///
		/// 作用：
		/// - 将当前天数加 1。
		/// - 如果设置了最大天数限制，则不超过限制。
		/// - 发布 SyncDaysDisplay 通知 UI 更新。
		/// </summary>
		private void HandleAdvanceDay(AdvanceDayEvent eventData)
		{
			AdvanceDay();
		}

		/// <summary>
		/// 处理重置天数事件。
		///
		/// 作用：
		/// - 将当前天数重置为初始值。
		/// - 发布 SyncDaysDisplay 通知 UI 更新。
		/// </summary>
		private void HandleResetDays(ResetDaysEvent eventData)
		{
			ResetDays();
		}

		/// <summary>
		/// 推进天数。
		///
		/// 作用：
		/// - 当前天数加 1。
		/// - 如果达到最大天数限制，则不再增加。
		/// - 发布 SyncDaysDisplay 通知 UI 更新。
		///
		/// 调用方式：
		/// - 通过 EventBus.Publish(new AdvanceDayEvent()) 触发。
		/// - 也可以直接调用 DaysManager.Instance.AdvanceDay()。
		/// </summary>
		public void AdvanceDay()
		{
			if (maxDay > 0 && currentDay >= maxDay)
			{
				Debug.Log($"[DaysManager] 已达到最大天数限制：{maxDay}");
				return;
			}

			currentDay++;
			//同步角色能量
			EventBus.Publish(new ResetCharacterPowerEvent());
			Debug.Log($"[DaysManager] 天数推进至：{currentDay}");
			SyncUI();
		}

		/// <summary>
		/// 重置天数。
		///
		/// 作用：
		/// - 将当前天数重置为 initialDay。
		/// - 发布 SyncDaysDisplay 通知 UI 更新。
		///
		/// 调用方式：
		/// - 通过 EventBus.Publish(new ResetDaysEvent()) 触发。
		/// - 也可以直接调用 DaysManager.Instance.ResetDays()。
		/// </summary>
		public void ResetDays()
		{
			currentDay = initialDay;
			Debug.Log($"[DaysManager] 天数重置为：{currentDay}");
			SyncUI();
		}

		/// <summary>
		/// 设置指定天数。
		///
		/// 作用：
		/// - 直接设置当前天数为指定值。
		/// - 用于读取存档、调试等场景。
		/// - 发布 SyncDaysDisplay 通知 UI 更新。
		///
		/// 参数：
		/// - day：目标天数，不能小于 1。
		/// </summary>
		/// <param name="day">目标天数。</param>
		public void SetDay(int day)
		{
			if (day < 1)
			{
				Debug.LogWarning("[DaysManager] 天数不能小于 1，已自动修正为 1。");
				day = 1;
			}

			if (maxDay > 0 && day > maxDay)
			{
				Debug.LogWarning($"[DaysManager] 天数不能超过最大限制 {maxDay}，已自动修正。");
				day = maxDay;
			}

			currentDay = day;
			Debug.Log($"[DaysManager] 天数设置为：{currentDay}");
			SyncUI();
		}

		/// <summary>
		/// 同步 UI 显示。
		///
		/// 作用：
		/// - 发布 SyncDaysDisplay 事件，携带当前天数。
		/// - UI 层订阅此事件即可更新天数文字、图标等。
		/// </summary>
		private void SyncUI()
		{
			EventBus.Publish(new SyncDaysDisplay { currentDay = currentDay });
		}
	}
}