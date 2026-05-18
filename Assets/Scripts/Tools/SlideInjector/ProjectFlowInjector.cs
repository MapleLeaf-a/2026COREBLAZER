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
		public void Awake()
		{
			//基本工具
			EventBus.Instance.Init();

			//场景流程管理
			GameSceneManager.Instance.Init();
		}
	}
}