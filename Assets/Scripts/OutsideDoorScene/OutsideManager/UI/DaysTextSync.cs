using Assets.Scripts.OutsideDoorScene.OutsideManager.Manager;
using Events;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.UI
{
	/// <summary>
	/// 天数显示 UI 控制器。
	///
	/// 作用：
	/// 1. 订阅 DaysManager 发出的 SyncDaysDisplay 事件。
	/// 2. 将当前天数同步到指定的 TMP_Text 组件。
	///
	/// 使用方式：
	/// 1. 将此脚本挂载到 Days 对象或其父对象上。
	/// 2. 在 Inspector 中将 dayText 字段拖入 nums (TMP_Text) 组件。
	/// 3. 如果不手动拖入，脚本会自动在子对象中查找 TMP_Text 组件。
	///
	/// 设计说明：
	/// - 遵循 PowerSlider 的事件驱动模式。
	/// - UI 只负责显示，不负责计算天数逻辑。
	/// - 天数逻辑集中在 DaysManager，UI 通过订阅事件获取最新值。
	/// </summary>
	public class DaysTextSync : MonoBehaviour
	{
		/// <summary>
		/// 天数文字组件。
		///
		/// 参数作用：
		/// - 拖入场景中用于显示天数的 TMP_Text 组件。
		/// - 例如 Days/nums 对象上的 TextMeshProUGUI 组件。
		/// - 脚本会在天数变化时更新此组件的文字内容。
		/// </summary>
		[SerializeField]
		private TMP_Text dayText;

		/// <summary>
		/// 天数文字格式。
		///
		/// 参数作用：
		/// - 使用 C# 字符串格式化语法。
		/// - {0} 会被替换为当前天数。
		/// - 默认格式为 "Day {0}"，显示为 "Day 1"、"Day 2" 等。
		/// - 如果只需要显示数字，可以设置为 "{0}"。
		/// </summary>
		[SerializeField]
		private string dayFormat = "Day {0}";

		private void Awake()
		{
			// 如果 Inspector 中未手动拖入，自动在子对象中查找 TMP_Text 组件。
			if (dayText == null)
			{
				dayText = GetComponentInChildren<TMP_Text>(true);
			}
		}

		private void OnEnable()
		{
			EventBus.Subscribe<SyncDaysDisplay>(HandleSyncDaysDisplay);

			// 初始化时同步一次，确保 UI 显示正确。
			// 如果 DaysManager 已存在，立即获取当前天数。
			if (DaysManager.Instance != null)
			{
				UpdateDayText(DaysManager.Instance.CurrentDay);
			}
		}

		private void OnDisable()
		{
			EventBus.Unsubscribe<SyncDaysDisplay>(HandleSyncDaysDisplay);
		}

		/// <summary>
		/// 处理天数同步事件。
		///
		/// 作用：
		/// - 接收 DaysManager 发布的新天数。
		/// - 更新 TMP_Text 组件的文字内容。
		/// </summary>
		private void HandleSyncDaysDisplay(SyncDaysDisplay eventData)
		{
			UpdateDayText(eventData.currentDay);
		}

		/// <summary>
		/// 更新天数文字显示。
		///
		/// 参数：
		/// - day：要显示的天数。
		///
		/// 作用：
		/// - 将天数格式化为指定格式。
		/// - 更新 TMP_Text 组件的文字内容。
		/// </summary>
		private void UpdateDayText(int day)
		{
			if (dayText == null)
			{
				return;
			}

			dayText.text = string.Format(dayFormat, day);
		}
	}
}