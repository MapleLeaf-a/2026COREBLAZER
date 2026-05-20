using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.UI
{
	/// <summary>
	/// 鼠标悬浮显示控制器。
	///
	/// 作用：
	/// 1. 当鼠标悬浮在当前 Collider 对象上时，激活指定的目标对象。
	/// 2. 当鼠标离开时，关闭指定的目标对象。
	///
	/// 使用方式：
	/// 1. 挂载到带有 Collider 或 Collider2D 的 GameObject 上。
	/// 2. 在 Inspector 中将 targetObject 拖入需要显示/隐藏的对象。
	///
	/// 注意事项：
	/// - 2D 碰撞体需要 Collider2D 组件。
	/// - 3D 碰撞体需要 Collider 组件。
	/// - 鼠标检测需要场景中有 Camera 组件（MainCamera）。
	/// </summary>
	public class OnMouseOverView : MonoBehaviour
	{
		/// <summary>
		/// 鼠标悬浮时需要显示的目标对象。
		///
		/// 参数作用：
		/// - 拖入需要在鼠标悬浮时显示的 GameObject。
		/// - 鼠标进入碰撞体时激活此对象。
		/// - 鼠标离开碰撞体时关闭此对象。
		/// </summary>
		[SerializeField]
		private GameObject targetObject;

		/// <summary>
		/// 目标对象的初始状态。
		///
		/// 参数作用：
		/// - true：目标对象初始为隐藏状态，鼠标悬浮时才显示。
		/// - false：目标对象初始为显示状态，鼠标悬浮时会保持显示。
		/// </summary>
		[SerializeField]
		private bool hideOnStart = true;

		private void Start()
		{
			// 如果设置为初始隐藏，则在启动时关闭目标对象。
			if (hideOnStart && targetObject != null)
			{
				targetObject.SetActive(false);
			}
		}

		/// <summary>
		/// 鼠标进入碰撞体时调用。
		///
		/// 作用：
		/// - 激活目标对象，使其可见。
		/// </summary>
		private void OnMouseEnter()
		{
			if (targetObject != null)
			{
				targetObject.SetActive(true);
			}
		}

		/// <summary>
		/// 鼠标离开碰撞体时调用。
		///
		/// 作用：
		/// - 关闭目标对象，使其隐藏。
		/// </summary>
		private void OnMouseExit()
		{
			if (targetObject != null)
			{
				targetObject.SetActive(false);
			}
		}
	}
}
