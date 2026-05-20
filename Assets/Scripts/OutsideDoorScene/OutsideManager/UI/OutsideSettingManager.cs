using Assets.Scripts.OutsideDoorScene.OutsideManager.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.UI
{
	/// <summary>
	/// 外部场景设置管理器。
	///
	/// 处理Setting面板中的设置按钮和返回按钮。
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class OutsideSettingManager : MonoBehaviour
	{
		/// <summary>
		/// 设置按钮。
		/// </summary>
		[SerializeField]
		private Button settingButton;

		/// <summary>
		/// 返回主界面按钮。
		/// </summary>
		[SerializeField]
		private Button backButton;

		/// <summary>
		/// 设置面板游戏对象。
		/// 用于在点击设置按钮时激活设置面板。
		/// </summary>
		[SerializeField]
		private GameObject settingPanel;

		/// <summary>
		/// 设置按钮的公共访问器。
		/// </summary>
		public Button SettingButton => settingButton;

		/// <summary>
		/// 返回按钮的公共访问器。
		/// </summary>
		public Button BackButton => backButton;

		/// <summary>
		/// 设置面板的公共访问器。
		/// </summary>
		public GameObject SettingPanel => settingPanel;

		private void Awake()
		{
			// 验证引用
			ValidateReferences();

			// 设置按钮点击事件
			SetupButtonListeners();
		}

		/// <summary>
		/// 验证引用是否有效。
		/// </summary>
		private void ValidateReferences()
		{
			if (settingButton == null)
			{
				Debug.LogError("OutsideSettingManager: SettingButton 引用未设置！", this);
			}

			if (backButton == null)
			{
				Debug.LogError("OutsideSettingManager: BackButton 引用未设置！", this);
			}

			if (settingPanel == null)
			{
				Debug.LogError("OutsideSettingManager: SettingPanel 引用未设置！", this);
			}
		}

		/// <summary>
		/// 设置按钮点击事件监听器。
		/// </summary>
		private void SetupButtonListeners()
		{
			if (settingButton != null)
			{
				settingButton.onClick.AddListener(OnSettingButtonClicked);
			}

			if (backButton != null)
			{
				backButton.onClick.AddListener(OnBackButtonClicked);
			}
		}

		/// <summary>
		/// 设置按钮点击事件处理方法。
		/// </summary>
		private void OnSettingButtonClicked()
		{
			Debug.Log("设置按钮被点击");
			// 激活设置面板
			if (settingPanel != null)
			{
				settingPanel.SetActive(true);
			}
			else
			{
				Debug.LogError("OutsideSettingManager: SettingPanel 引用未设置！", this);
			}
		}

		/// <summary>
		/// 返回按钮点击事件处理方法。
		/// </summary>
		private void OnBackButtonClicked()
		{
			Debug.Log("返回按钮被点击");
			// 返回主界面逻辑
			// 这里可以调用场景切换或关闭当前UI
			// 例如：SceneManager.LoadScene("MainMenu");
			// 或者调用OutsideUI的Close方法
			if (OutsideUI.Instance != null)
			{
				OutsideUI.Instance.Close();
			}
		}
	}
}