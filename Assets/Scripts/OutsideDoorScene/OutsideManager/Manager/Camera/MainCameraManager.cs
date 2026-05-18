using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.Manager
{
	/// <summary>
	/// 让正交相机自动适配一组 SpriteRenderer 的显示范围。
	///
	/// 适用场景：
	/// 地图已经从 Canvas Image 转成 SpriteRenderer，
	/// 现在需要让 Main Camera 自动看到整张地图。
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public sealed class FitCameraToSpriteRenderers2D : MonoBehaviour
	{
		[Header("Target")]

		/// <summary>
		/// 地图根节点。
		///
		/// 例如：
		/// WorldRoot / MapRoot
		///
		/// 脚本会扫描它下面所有 SpriteRenderer，计算整张地图的 Bounds。
		/// Bounds 是包围盒，表示一组物体整体占据的世界范围。
		/// </summary>
		[SerializeField]
		private Transform mapRoot;

		[Header("Padding")]

		/// <summary>
		/// 相机边缘额外留白。
		///
		/// 0 表示刚好贴住地图边缘。
		/// 1 表示上下左右多留 1 个世界单位。
		/// </summary>
		[SerializeField]
		[Min(0f)]
		private float padding = 0.5f;

		[Header("Apply")]

		/// <summary>
		/// 是否在 Start 时自动适配。
		///
		/// true 表示进入运行后自动调整相机。
		/// false 表示只通过右键菜单手动执行。
		/// </summary>
		[SerializeField]
		private bool fitOnStart = true;

		/// <summary>
		/// 当前相机。
		/// </summary>
		private Camera targetCamera;

		/// <summary>
		/// Unity 唤醒回调。
		///
		/// 用于缓存 Camera 组件，并确保相机是正交模式。
		/// </summary>
		private void Awake()
		{
			targetCamera = GetComponent<Camera>();
			targetCamera.orthographic = true;
		}

		/// <summary>
		/// Unity 启动回调。
		///
		/// 如果 fitOnStart 为 true，就自动适配地图。
		/// </summary>
		private void Start()
		{
			if (fitOnStart)
			{
				Fit();
			}
		}

		/// <summary>
		/// 在 Inspector 右键菜单中手动执行适配。
		///
		/// 使用方式：
		/// 组件右上角菜单 / Fit Camera To Map
		/// </summary>
		[ContextMenu("Fit Camera To Map")]
		public void Fit()
		{
			if (targetCamera == null)
			{
				targetCamera = GetComponent<Camera>();
			}

			if (mapRoot == null)
			{
				Debug.LogWarning("FitCameraToSpriteRenderers2D: mapRoot is null.");
				return;
			}

			SpriteRenderer[] renderers = mapRoot.GetComponentsInChildren<SpriteRenderer>(true);

			if (renderers == null || renderers.Length == 0)
			{
				Debug.LogWarning("FitCameraToSpriteRenderers2D: no SpriteRenderer found under mapRoot.");
				return;
			}

			Bounds bounds = renderers[0].bounds;

			for (int i = 1; i < renderers.Length; i++)
			{
				bounds.Encapsulate(renderers[i].bounds);
			}

			Vector3 center = bounds.center;

			transform.position = new Vector3(
				center.x,
				center.y,
				transform.position.z);

			float mapHeight = bounds.size.y + padding * 2f;
			float mapWidth = bounds.size.x + padding * 2f;

			float sizeByHeight = mapHeight * 0.5f;
			float sizeByWidth = mapWidth / (2f * targetCamera.aspect);

			targetCamera.orthographicSize = Mathf.Max(
				sizeByHeight,
				sizeByWidth);
		}
	}
}