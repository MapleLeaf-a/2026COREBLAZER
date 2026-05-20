using Assets.Scripts.OutsideDoorScene.OutsideManager.Manager;
using Assets.Scripts.OutsideDoorScene.OutsideManager.UI;
using Assets.Scripts.Tools.Unity;
using Events;
using UnityEngine;

namespace Assets.Scripts.Tools.SlideInjector
{
	/// <summary>
	/// 为了轻量化嵌入，就只使用单例完成系统管理
	/// </summary>
	public class ProjectFlowInjector : MonoBehaviour
	{
		public void OnEnable()
		{
			if (OutsideUI.Instance == null)
				return;
			OutsideUI.Instance.Close();
			Debug.Log("进入室内关闭户外UI");
		}

		public void OnDisable()
		{
			if (OutsideUI.Instance == null)
				return;
			OutsideUI.Instance.Open();
			Debug.Log("进入室外开启户外UI");
		}

		public void Awake()
		{
			//基本工具
			EventBus.Instance.Init();

			//场景流程管理
			GameSceneManager.Instance.Init();

			//户外UI管理注册
			OutsideUIPanelManager.Instance.Init();

			//户外能量注册
			OutsideDoorCharacterPowerManager.Instance.Init();
		}
	}
}