using Assets.Scripts.OutsideDoorScene.OutsideManager.Manager;
using Events;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.UI
{
	/// <summary>
	/// 能量条 UI 控制器。
	///
	/// 作用：
	/// 1. 订阅 OutsideDoorCharacterPowerManager 发出的 SyncPowerSlider 事件。
	/// 2. 将当前角色能量同步到指定的 Unity UI Slider。
	/// 3. Slider 需在 Inspector 中设置 Direction 为 RightToLeft，以实现从右向左递减效果。
	///
	/// 使用方式：
	/// 1. 将此脚本挂载到 PowerUISlider 或其父对象上。
	/// 2. 在 Inspector 中将 slider 字段拖入 PowerUISlider 组件。
	/// </summary>
	public class PowerSlider : MonoBehaviour
	{
		/// <summary>
		/// 能量条 Slider 组件。
		///
		/// 参数作用：
		/// - 拖入场景中的 PowerUISlider 对象上的 Slider 组件。
		/// - 脚本会自动设置 Slider 范围为 0 ~ maxValue，并禁止玩家交互。
		/// </summary>
		[SerializeField]
		private Slider slider;

		/// <summary>
		/// 角色最大能量值。
		///
		/// 参数作用：
		/// - 与 OutsideDoorCharacterPowerManager 中的 CharacterPower 上限保持一致（默认 100）。
		/// - 用于设置 Slider 的 maxValue。
		/// </summary>
		[SerializeField]
		private float maxValue = 100f;

		private void Awake()
		{
			if (slider == null)
			{
				slider = GetComponentInChildren<Slider>(true);
			}
		}

		private void OnEnable()
		{
			EventBus.Subscribe<SyncPowerSlider>(HandleSyncPowerSlider);
			ConfigureSlider();
		}

		private void OnDisable()
		{
			EventBus.Unsubscribe<SyncPowerSlider>(HandleSyncPowerSlider);
		}

		/// <summary>
		/// 配置 Slider 范围和交互属性。
		/// </summary>
		private void ConfigureSlider()
		{
			if (slider == null)
			{
				return;
			}

			slider.minValue = 0f;
			slider.maxValue = maxValue;
			slider.interactable = false;
			slider.value = maxValue;
		}

		/// <summary>
		/// 接收能量同步事件，更新 Slider 数值。
		/// </summary>
		private void HandleSyncPowerSlider(SyncPowerSlider eventData)
		{
			if (slider == null)
			{
				return;
			}

			slider.value = Mathf.Clamp(eventData.currentCharacterPower, 0f, maxValue);
		}
	}
}
