using Assets.Scripts.OutsideDoorScene.OutsideManager.UI;
using Events;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainMenu
{
	/// <summary>
	/// 设置面板管理器。
	///
	/// 独立于主界面脚本，方便复用。
	/// 提供公开的Action事件和Slider接口。
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class SettingPanelManager : MonoBehaviour
	{
		/// <summary>
		/// 返回主菜单按钮。
		/// </summary>
		[SerializeField]
		private Button backMenuButton;

		/// <summary>
		/// 返回游戏按钮。
		/// </summary>
		[SerializeField]
		private Button backGameButton;

		/// <summary>
		/// 音量滑块。
		/// </summary>
		[SerializeField]
		private Slider volumnSlider;

		/// <summary>
		/// 返回主菜单按钮点击事件。
		/// 可在外部订阅以替换默认行为。
		/// </summary>
		public event Action OnBackMenuButtonClicked;

		/// <summary>
		/// 返回游戏按钮点击事件。
		/// 可在外部订阅以替换默认行为。
		/// </summary>
		public event Action OnBackGameButtonClicked;

		/// <summary>
		/// 返回主菜单按钮的公共访问器。
		/// </summary>
		public Button BackMenuButton => backMenuButton;

		/// <summary>
		/// 返回游戏按钮的公共访问器。
		/// </summary>
		public Button BackGameButton => backGameButton;

		/// <summary>
		/// 音量滑块的公共访问器。
		/// 可用于外部音频系统读取或设置音量值。
		/// </summary>
		public Slider VolumnSlider => volumnSlider;

		private bool isFlag = true;

		public void Awake()
		{
			// 验证引用
			ValidateReferences();

			// 设置按钮点击事件
			SetupButtonListeners();
		}

		private void OnEnable()
		{
			if (isFlag)
			{
				return;
			}
			if (backMenuButton != null)
			{
				backMenuButton.onClick.AddListener(HandleBackMenuButtonClicked);
			}

			if (backGameButton != null)
			{
				backGameButton.onClick.AddListener(HandleBackGameButtonClicked);
			}
		}

		private void OnDisable()
		{
			SetupButtonListeners();
			isFlag = false;
		}

		/// <summary>
		/// 验证引用是否有效。
		/// </summary>
		private void ValidateReferences()
		{
			if (backMenuButton == null)
			{
				Debug.LogError("SettingPanelManager: BackMenuButton 引用未设置！", this);
			}

			if (backGameButton == null)
			{
				Debug.LogError("SettingPanelManager: BackGameButton 引用未设置！", this);
			}

			if (volumnSlider == null)
			{
				Debug.LogError("SettingPanelManager: VolumnSlider 引用未设置！", this);
			}
		}

		/// <summary>
		/// 设置按钮点击事件监听器。
		/// </summary>
		private void SetupButtonListeners()
		{
			if (backMenuButton != null)
			{
				backMenuButton.onClick.AddListener(HandleBackMenuButtonClicked);
			}

			if (backGameButton != null)
			{
				backGameButton.onClick.AddListener(HandleBackGameButtonClicked);
			}
		}

		/// <summary>
		/// 处理返回主菜单按钮点击事件。
		/// </summary>
		private void HandleBackMenuButtonClicked()
		{
			// 如果有外部订阅者，则调用外部订阅者
			if (OnBackMenuButtonClicked != null)
			{
				OnBackMenuButtonClicked.Invoke();
			}
			else
			{
				// 默认行为：关闭设置面板
				if (SceneManager.GetActiveScene().name == "MainMenu")
				{
					Debug.Log("已经在主界面");
				}
				else
				{
					Debug.Log("切换回主界面");
					EventBus.Publish(new SceneTransitionRequestEvent("MainMenu", null, 0.35f));
					OutsideUI.Instance.Close();
				}
				Hide();
			}
		}

		/// <summary>
		/// 处理返回游戏按钮点击事件。
		/// </summary>
		private void HandleBackGameButtonClicked()
		{
			// 如果有外部订阅者，则调用外部订阅者
			if (OnBackGameButtonClicked != null)
			{
				OnBackGameButtonClicked.Invoke();
			}
			else
			{
				Hide();
			}
		}

		/// <summary>
		/// 显示设置面板。
		/// </summary>
		public void Show()
		{
			gameObject.SetActive(true);
		}

		/// <summary>
		/// 隐藏设置面板。
		/// </summary>
		public void Hide()
		{
			gameObject.SetActive(false);
		}

		/// <summary>
		/// 切换设置面板的显示状态。
		/// </summary>
		public void Toggle()
		{
			gameObject.SetActive(!gameObject.activeSelf);
		}
	}
}
