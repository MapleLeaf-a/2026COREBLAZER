using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.Manager
{
	using UnityEngine;

	/// <summary>
	/// 2D 相机跟随 Player。
	///
	/// 适用场景：
	/// 地图比屏幕大，玩家在地图中移动，
	/// 相机跟随玩家显示局部区域。
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public sealed class FollowPlayerCamera2D : MonoBehaviour
	{
		[Header("Target")]

		/// <summary>
		/// 要跟随的角色。
		///
		/// 一般拖 Player 根物体进来。
		/// </summary>
		[SerializeField]
		private Transform target;

		[Header("View")]

		/// <summary>
		/// 相机正交视野大小。
		///
		/// 数值越大，看得越远，角色越小。
		/// 数值越小，看得越近，角色越大。
		/// </summary>
		[SerializeField]
		[Min(0.1f)]
		private float orthographicSize = 6f;

		[Header("Follow")]

		/// <summary>
		/// 相机跟随平滑时间。
		///
		/// 0 表示立刻跟随。
		/// 0.15 表示有轻微缓动。
		/// </summary>
		[SerializeField]
		[Min(0f)]
		private float smoothTime = 0.15f;

		/// <summary>
		/// 相机相对角色的偏移。
		///
		/// 例如想让角色略偏画面左侧，可以设置 x = 1。
		/// </summary>
		[SerializeField]
		private Vector2 offset = Vector2.zero;

		private Camera targetCamera;
		private Vector3 velocity;

		private void Awake()
		{
			targetCamera = GetComponent<Camera>();
			targetCamera.orthographic = true;
			targetCamera.orthographicSize = orthographicSize;
		}

		private void LateUpdate()
		{
			if (target == null)
			{
				return;
			}

			Vector3 targetPosition = new Vector3(
				target.position.x + offset.x,
				target.position.y + offset.y,
				transform.position.z);

			if (smoothTime <= 0f)
			{
				transform.position = targetPosition;
				return;
			}

			transform.position = Vector3.SmoothDamp(
				transform.position,
				targetPosition,
				ref velocity,
				smoothTime);
		}
	}
}