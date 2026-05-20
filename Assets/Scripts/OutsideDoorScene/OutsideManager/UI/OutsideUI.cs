using Assets.Scripts.Tools.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.UI
{
	/// <summary>
	/// 跨场景持久化 UI 根节点。
	///
	/// 作用：
	/// 1. 通过 DontDestroyOnLoad 保持 UI 在场景切换时不被销毁。
	/// 2. 场景加载完成后，重新绑定 Canvas 相机，避免旧相机引用丢失。
	/// 3. 确保 GraphicRaycaster 组件可用，避免 UI 射线检测失效。
	///
	/// 注意：
	/// - 不要让 EventSystem 跨场景持久化，每个场景应有自己的 EventSystem。
	/// - 如果 UI Canvas 使用 ScreenSpaceCamera 模式，切场景后必须重新绑定相机。
	/// </summary>
	public class OutsideUI : MonoSingleton<OutsideUI>
	{
		public GameObject m_uiPanel;

		public void OnEnable()
		{
			DontDestroyOnLoad(this);
			SceneManager.sceneLoaded += OnSceneLoaded;
			this.Open();
		}

		public void OnDisable()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			this.Close();
		}

		/// <summary>
		/// 场景加载完成回调。
		///
		/// 作用：
		/// - 重新绑定 Canvas 相机，解决 ScreenSpaceCamera 模式下旧相机被销毁的问题。
		/// - 确保 GraphicRaycaster 启用，避免 UI 射线检测失效。
		/// </summary>
		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			RebindCanvasCameras();
			EnsureGraphicRaycastersEnabled();
		}

		/// <summary>
		/// 重新绑定所有 ScreenSpaceCamera 模式 Canvas 的相机。
		///
		/// 原理：
		/// - ScreenSpaceCamera 模式的 Canvas 需要 worldCamera 才能正确进行射线检测。
		/// - 切场景后旧相机被销毁，worldCamera 变为 null，导致射线检测异常。
		/// - 重新绑定 Camera.main 可修复此问题。
		///
		/// 注意：
		/// - ScreenSpaceOverlay 模式的 Canvas 不需要相机，无需处理。
		/// - WorldSpace 模式的 Canvas 有自己的相机逻辑，也无需处理。
		/// </summary>
		private void RebindCanvasCameras()
		{
			Camera mainCamera = Camera.main;

			if (mainCamera == null)
			{
				Debug.LogWarning("[OutsideUI] 当前场景没有 MainCamera，无法重新绑定 Canvas 相机。");
				return;
			}

			Canvas[] canvases = GetComponentsInChildren<Canvas>(true);

			foreach (Canvas canvas in canvases)
			{
				if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
				{
					canvas.worldCamera = mainCamera;
				}
			}
		}

		/// <summary>
		/// 确保所有 GraphicRaycaster 组件启用。
		///
		/// 原理：
		/// - GraphicRaycaster 是 UI 接收鼠标点击和触摸的射线检测组件。
		/// - 如果 GraphicRaycaster 被禁用，该 Canvas 下的 UI 将无法接收点击。
		/// - 场景切换后，某些情况下组件可能意外被禁用，这里做兜底处理。
		/// </summary>
		private void EnsureGraphicRaycastersEnabled()
		{
			GraphicRaycaster[] raycasters = GetComponentsInChildren<GraphicRaycaster>(true);

			foreach (GraphicRaycaster raycaster in raycasters)
			{
				if (!raycaster.enabled)
				{
					raycaster.enabled = true;
				}
			}
		}

		public void Close()
		{
			m_uiPanel.SetActive(false);
		}

		public void Open()
		{
			m_uiPanel.SetActive(true);
		}
	}
}