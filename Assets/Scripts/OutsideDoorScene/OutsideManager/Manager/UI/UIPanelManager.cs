using Assets.Scripts.OutsideDoorScene.OutsideManager.Manager;
using Assets.Scripts.Tools.Common;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.UI
{
	/// <summary>
	/// 外出场景 UI 面板管理器。
	///
	/// 作用：
	/// 1. 接收 OutsideDoorCharacterPowerManager 发出的 SyncPowerSlider 事件。
	/// 2. 把角色当前外出能量同步到右侧能量条 UI。
	/// 3. 接收 OpenOutsideUIEvent / CloseOutsideUIEvent，统一控制外出 UI 显示和隐藏。
	///
	/// 注意：
	/// 这个脚本不要挂在会被 outsideUIPanelRoot.SetActive(false) 关闭的同一个对象上。
	/// 否则 UI 被关闭后，本脚本也会被禁用，后续 OpenOutsideUIEvent 就收不到了。
	/// 推荐挂在 OutsideUIManager 这类常驻对象上，然后把真正的 UI 面板拖给 outsideUIPanelRoot。
	/// </summary>
	internal sealed class OutsideUIPanelManager : MonoSingleton<OutsideUIPanelManager>
	{
		[Header("Panel")]

		/// <summary>
		/// 外出 UI 面板根节点。
		///
		/// 参数作用：
		/// - 这里拖入真正要显示/隐藏的 UI 面板 GameObject。
		/// - CloseOutsideUIEvent 到来时会把它 SetActive(false)。
		/// - OpenOutsideUIEvent 到来时会把它 SetActive(true)。
		///
		/// GameObject：
		/// Unity 场景中的基础对象，角色、UI、相机都属于 GameObject。
		/// SetActive：
		/// Unity 用来控制对象是否启用的方法，false 表示隐藏并停止其子对象脚本生命周期。
		/// </summary>
		[SerializeField]
		private GameObject outsideUIPanelRoot;

		[Header("Power Bar")]

		/// <summary>
		/// 可行动能量 Slider。
		///
		/// 参数作用：
		/// - 如果你的能量条是 Unity UI Slider，就把 Slider 组件拖到这里。
		/// - 本脚本会把 Slider 的范围设置为 0 ~ maxCharacterPower。
		/// - 当前能量变化时，会把 powerSlider.value 设置为当前能量。
		///
		/// Slider：
		/// Unity UI 里的滑动条组件，常用于血条、能量条、进度条。
		/// value：
		/// Slider 当前值。比如范围是 0~100，value=75 就显示 75%。
		/// </summary>
		[SerializeField]
		private Slider powerSlider;

		/// <summary>
		/// 可行动能量填充图片。
		///
		/// 参数作用：
		/// - 如果你的能量条不是 Slider，而是 Image 填充条，就把 Image 组件拖到这里。
		/// - 本脚本会把 fillAmount 设置为 当前能量 / 最大能量。
		/// - 如果你已经使用 Slider，可以不填这个字段。
		///
		/// Image：
		/// Unity UI 里的图片组件。
		/// fillAmount：
		/// Image 在 Filled 模式下的填充比例，范围是 0~1。
		/// 0 表示完全空，1 表示完全填满。
		/// </summary>
		[SerializeField]
		private Image powerFillImage;

		/// <summary>
		/// 可行动能量百分比文字。
		///
		/// 参数作用：
		/// - 如果 UI 上需要显示 “100% / 75% / 0%” 这种文字，就把 TMP_Text 拖到这里。
		/// - 如果不需要文字，可以不填。
		///
		/// TMP_Text：
		/// TextMeshPro 的文字组件。
		/// TextMeshPro：
		/// Unity 常用的高质量文字系统，比老版 UI Text 显示效果更好。
		/// </summary>
		[SerializeField]
		private TMP_Text powerPercentText;

		/// <summary>
		/// 角色外出能量最大值。
		///
		/// 参数作用：
		/// - 当前 OutsideDoorCharacterPowerManager 使用 100 作为满能量。
		/// - UI 这里也保持 100，方便把 currentCharacterPower 当成百分比显示。
		/// - 如果以后升级系统把最大能量改成 150，这里也可以同步改成 150。
		/// </summary>
		[SerializeField]
		private float maxCharacterPower = 100f;

		/// <summary>
		/// 是否在启动时自动配置 Slider 范围。
		///
		/// 参数作用：
		/// - true：脚本自动设置 powerSlider.minValue = 0，powerSlider.maxValue = maxCharacterPower。
		/// - false：保留 Inspector 里手动配置的 Slider 范围。
		/// </summary>
		[SerializeField]
		private bool autoConfigureSliderRange = true;

		/// <summary>
		/// Unity 生命周期：Awake 会在脚本启用前调用。
		///
		/// 这里的必要逻辑：
		/// - 先调用 base.Awake()，保证 MonoSingleton 能正确记录单例实例。
		/// - 再初始化能量条范围，避免第一次事件到来前 Slider 范围不正确。
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			ConfigurePowerSlider();
		}

		/// <summary>
		/// Unity 生命周期：OnEnable 会在对象启用时调用。
		///
		/// 这里的必要逻辑：
		/// - 订阅 SyncPowerSlider，接收能量管理器传来的当前能量。
		/// - 订阅 CloseOutsideUIEvent，用事件关闭外出 UI。
		/// - 订阅 OpenOutsideUIEvent，用事件打开外出 UI。
		/// - 最后主动刷新一次满能量 UI，避免 UI 初始显示为空。
		///
		/// Subscribe：
		/// 订阅事件，也就是告诉 EventBus：
		/// “以后这个事件发生时，请调用我这个方法。”
		/// </summary>
		private void OnEnable()
		{
			EventBus.Subscribe<SyncPowerSlider>(HandleSyncPowerSlider);
			EventBus.Subscribe<CloseOutsideUIEvent>(HandleCloseOutsideUI);
			EventBus.Subscribe<OpenOutsideUIEvent>(HandleOpenOutsideUI);

			RefreshPowerUI(maxCharacterPower);
		}

		/// <summary>
		/// Unity 生命周期：OnDisable 会在对象禁用时调用。
		///
		/// 这里的必要逻辑：
		/// - 取消订阅，避免对象被禁用后，EventBus 还继续调用这个对象的方法。
		/// - 如果不取消订阅，之后可能出现空引用、重复刷新、旧对象残留等问题。
		///
		/// Unsubscribe：
		/// 取消订阅事件，也就是把之前注册到 EventBus 的方法移除。
		/// </summary>
		private void OnDisable()
		{
			EventBus.Unsubscribe<SyncPowerSlider>(HandleSyncPowerSlider);
			EventBus.Unsubscribe<CloseOutsideUIEvent>(HandleCloseOutsideUI);
			EventBus.Unsubscribe<OpenOutsideUIEvent>(HandleOpenOutsideUI);
		}

		/// <summary>
		/// 关闭外出 UI。
		///
		/// 参数说明：
		/// - 无参数版本保留下来，方便你在 Unity Inspector 的 Button OnClick 里直接绑定。
		///
		/// 必要逻辑：
		/// - 只负责 UI 显示关闭。
		/// - 不在这里修改角色能量。
		/// - 不在这里切换场景。
		/// </summary>
		public void OnDisableOutsideUI()
		{
			SetOutsideUIPanelActive(false);
		}

		/// <summary>
		/// 打开外出 UI。
		///
		/// 参数说明：
		/// - 无参数版本保留下来，方便你在 Unity Inspector 的 Button OnClick 里直接绑定。
		///
		/// 必要逻辑：
		/// - 只负责 UI 显示打开。
		/// - 能量数值仍然由 SyncPowerSlider 事件同步。
		/// </summary>
		public void OnEnableOutsideUI()
		{
			SetOutsideUIPanelActive(true);
		}

		/// <summary>
		/// 专用事件处理：同步能量条。
		///
		/// 参数说明：
		/// - eventData：事件数据。
		/// - eventData.currentCharacterPower：当前角色外出能量。
		///
		/// 必要逻辑：
		/// - OutsideDoorCharacterPowerManager 每次扣能量后会发布 SyncPowerSlider。
		/// - UI 不主动计算能量，只接收管理器给出的最终数据。
		/// - 这样能避免 UI 和能量系统各算各的，造成数值不同步。
		/// </summary>
		private void HandleSyncPowerSlider(SyncPowerSlider eventData)
		{
			RefreshPowerUI(eventData.currentCharacterPower);
		}

		/// <summary>
		/// 专用事件处理：关闭外出 UI。
		///
		/// 参数说明：
		/// - eventData：关闭 UI 事件本身。
		/// - 目前该事件没有字段，只代表“应该关闭外出 UI”这个动作。
		/// </summary>
		private void HandleCloseOutsideUI(CloseOutsideUIEvent eventData)
		{
			OnDisableOutsideUI();
		}

		/// <summary>
		/// 专用事件处理：打开外出 UI。
		///
		/// 参数说明：
		/// - eventData：打开 UI 事件本身。
		/// - 目前该事件没有字段，只代表“应该打开外出 UI”这个动作。
		/// </summary>
		private void HandleOpenOutsideUI(OpenOutsideUIEvent eventData)
		{
			OnEnableOutsideUI();
		}

		/// <summary>
		/// 配置 Slider 的显示范围。
		///
		/// 必要逻辑：
		/// - 能量管理器传来的 currentCharacterPower 是 0~100。
		/// - 所以 Slider 也应该使用 0~100，避免还要额外换算。
		/// - interactable 设置为 false，避免玩家拖动 UI 条改变显示值。
		///
		/// interactable：
		/// Unity UI 组件是否允许玩家交互。
		/// false 表示这个 Slider 只做显示，不接受鼠标拖动。
		/// </summary>
		private void ConfigurePowerSlider()
		{
			if (powerSlider == null)
			{
				return;
			}

			if (autoConfigureSliderRange)
			{
				powerSlider.minValue = 0f;
				powerSlider.maxValue = maxCharacterPower;
			}

			powerSlider.interactable = false;
		}

		/// <summary>
		/// 刷新能量 UI。
		///
		/// 参数说明：
		/// - currentCharacterPower：当前角色外出能量。
		///
		/// 必要逻辑：
		/// - 先 Clamp 到 0~maxCharacterPower，防止 UI 出现负数或超过满值。
		/// - 再同步 Slider。
		/// - 再同步 Image 填充条。
		/// - 最后同步百分比文字。
		///
		/// Clamp：
		/// 数值限制。
		/// 例如 Clamp(120, 0, 100) 会得到 100；
		/// Clamp(-5, 0, 100) 会得到 0。
		/// </summary>
		private void RefreshPowerUI(float currentCharacterPower)
		{
			float safeMaxPower = Mathf.Max(1f, maxCharacterPower);
			float clampedPower = Mathf.Clamp(currentCharacterPower, 0f, safeMaxPower);
			float normalizedPower = clampedPower / safeMaxPower;

			if (powerSlider != null)
			{
				powerSlider.value = clampedPower;
			}

			if (powerFillImage != null)
			{
				powerFillImage.fillAmount = normalizedPower;
			}

			if (powerPercentText != null)
			{
				powerPercentText.text = $"{Mathf.CeilToInt(normalizedPower * 100f)}%";
			}
		}

		/// <summary>
		/// 设置外出 UI 面板显隐。
		///
		/// 参数说明：
		/// - isActive：是否显示 UI。
		///   true 表示显示。
		///   false 表示隐藏。
		///
		/// 必要逻辑：
		/// - outsideUIPanelRoot 没有绑定时不直接报错崩溃，只给出警告。
		/// - 这样在你还没拖 UI 引用时，能从 Console 看到明确问题。
		/// </summary>
		private void SetOutsideUIPanelActive(bool isActive)
		{
			if (outsideUIPanelRoot == null)
			{
				Debug.LogWarning("[OutsideUIPanelManager] outsideUIPanelRoot 未绑定，无法切换外出 UI 显隐。");
				return;
			}

			outsideUIPanelRoot.SetActive(isActive);
		}
	}
}