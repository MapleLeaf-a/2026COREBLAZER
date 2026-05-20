using Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MainMenu
{
	/// <summary>
	/// 主菜单UI管理器。
	///
	/// 负责管理主菜单界面中各个按钮的交互逻辑。
	/// 提供公共方法供按钮的OnClick事件调用。
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class MainMenuUIManager : MonoBehaviour
	{
		/// <summary>
		/// 开始游戏按钮。
		/// </summary>
		[SerializeField]
		private Button startButton;

		/// <summary>
		/// 设置按钮。
		/// </summary>
		[SerializeField]
		private Button settingButton;

		/// <summary>
		/// 退出游戏按钮。
		/// </summary>
		[SerializeField]
		private Button quitButton;

		/// <summary>
		/// 开始游戏索引UI。
		/// </summary>
		[SerializeField]
		private GameObject indexStart;

		/// <summary>
		/// 设置索引UI。
		/// </summary>
		[SerializeField]
		private GameObject indexSetting;

		/// <summary>
		/// 退出游戏索引UI。
		/// </summary>
		[SerializeField]
		private GameObject indexQuit;

		/// <summary>
		/// 设置面板游戏对象。
		/// 用于在点击设置按钮时激活设置面板。
		/// </summary>
		[SerializeField]
		private GameObject settingPanel;

		/// <summary>
		/// 当前激活的索引UI对象。
		/// 用于实现留存效果：鼠标离开按钮时保留当前Index显示。
		/// </summary>
		private GameObject currentActiveIndex;

		/// <summary>
		/// 开始游戏按钮的公共访问器。
		/// </summary>
		public Button StartButton => startButton;

		/// <summary>
		/// 设置按钮的公共访问器。
		/// </summary>
		public Button SettingButton => settingButton;

		/// <summary>
		/// 退出游戏按钮的公共访问器。
		/// </summary>
		public Button QuitButton => quitButton;

		/// <summary>
		/// 开始游戏索引UI的公共访问器。
		/// </summary>
		public GameObject IndexStart => indexStart;

		/// <summary>
		/// 设置索引UI的公共访问器。
		/// </summary>
		public GameObject IndexSetting => indexSetting;

		/// <summary>
		/// 退出游戏索引UI的公共访问器。
		/// </summary>
		public GameObject IndexQuit => indexQuit;

		private void Awake()
		{
			// 确保所有按钮引用都已设置
			ValidateButtonReferences();

			// 设置按钮点击事件监听器
			SetupButtonListeners();

			// 设置鼠标悬停事件
			SetupPointerEvents();

			// 初始化默认激活的Index（如果有的话）
			InitializeDefaultIndex();
		}

		/// <summary>
		/// 初始化默认激活的Index。
		/// 如果设置了indexStart，则默认激活它。
		/// </summary>
		private void InitializeDefaultIndex()
		{
			if (indexStart != null)
			{
				indexStart.SetActive(true);
				currentActiveIndex = indexStart;
			}
		}

		/// <summary>
		/// 设置按钮点击事件监听器。
		/// </summary>
		private void SetupButtonListeners()
		{
			if (startButton != null)
			{
				startButton.onClick.AddListener(OnStartButtonClicked);
			}

			if (settingButton != null)
			{
				settingButton.onClick.AddListener(OnSettingButtonClicked);
			}

			if (quitButton != null)
			{
				quitButton.onClick.AddListener(OnQuitButtonClicked);
			}
		}

		/// <summary>
		/// 设置鼠标悬停事件。
		/// </summary>
		private void SetupPointerEvents()
		{
			SetupButtonPointerEvents(startButton, indexStart);
			SetupButtonPointerEvents(settingButton, indexSetting);
			SetupButtonPointerEvents(quitButton, indexQuit);
		}

		/// <summary>
		/// 为按钮设置鼠标进入和离开事件。
		/// </summary>
		/// <param name="button">按钮组件。</param>
		/// <param name="indexObject">对应的索引UI对象。</param>
		private void SetupButtonPointerEvents(Button button, GameObject indexObject)
		{
			if (button == null || indexObject == null)
			{
				return;
			}

			// 获取或添加EventTrigger组件
			EventTrigger trigger = button.GetComponent<EventTrigger>();
			if (trigger == null)
			{
				trigger = button.gameObject.AddComponent<EventTrigger>();
			}

			// 创建PointerEnter事件条目
			EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
			pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
			pointerEnterEntry.callback.AddListener((data) => OnPointerEnter(indexObject));

			// 添加事件到EventTrigger
			trigger.triggers.Add(pointerEnterEntry);
		}

		/// <summary>
		/// 鼠标进入事件处理方法。
		/// 实现留存效果：隐藏当前激活的Index，激活新的Index，并更新当前激活的Index。
		/// </summary>
		/// <param name="indexObject">要激活的索引UI对象。</param>
		private void OnPointerEnter(GameObject indexObject)
		{
			if (indexObject == null)
			{
				return;
			}

			// 如果当前有激活的Index，且不是要激活的Index，则隐藏它
			if (currentActiveIndex != null && currentActiveIndex != indexObject)
			{
				currentActiveIndex.SetActive(false);
			}

			// 激活新的Index
			indexObject.SetActive(true);

			// 更新当前激活的Index
			currentActiveIndex = indexObject;
		}

		/// <summary>
		/// 验证按钮引用是否有效。
		/// </summary>
		private void ValidateButtonReferences()
		{
			if (startButton == null)
			{
				Debug.LogError("MainMenuUIManager: StartButton 引用未设置！", this);
			}

			if (settingButton == null)
			{
				Debug.LogError("MainMenuUIManager: SettingButton 引用未设置！", this);
			}

			if (quitButton == null)
			{
				Debug.LogError("MainMenuUIManager: QuitButton 引用未设置！", this);
			}
		}

		/// <summary>
		/// 开始游戏按钮点击事件处理方法。
		///
		/// 此方法应绑定到StartButton的OnClick事件。
		/// </summary>
		public void OnStartButtonClicked()
		{
			Debug.Log("开始游戏按钮被点击");

			EventBus.Publish(new SceneTransitionRequestEvent("MainGameWindow", null, 0.35f));
		}

		/// <summary>
		/// 设置按钮点击事件处理方法。
		///
		/// 此方法应绑定到SettingButton的OnClick事件。
		/// </summary>
		public void OnSettingButtonClicked()
		{
			Debug.Log("设置按钮被点击");
			// 激活设置面板
			if (settingPanel != null)
			{
				settingPanel.SetActive(true);
			}
			else
			{
				Debug.LogError("MainMenuUIManager: SettingPanel 引用未设置！", this);
			}
		}

		/// <summary>
		/// 退出游戏按钮点击事件处理方法。
		///
		/// 此方法应绑定到QuitButton的OnClick事件。
		/// </summary>
		public void OnQuitButtonClicked()
		{
			Debug.Log("退出游戏按钮被点击");
			// TODO: 实现退出游戏逻辑
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}