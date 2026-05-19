using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.UI
{
	public abstract class WorldPlaceButton : MonoBehaviour
	{
		protected string actionName = "InteractMouse0";

		private Camera mainCamera;

		protected virtual void Start()
		{
			mainCamera = Camera.main;
		}

		protected virtual void Update()
		{
			if (InputManager.instance.GetKeyDown(actionName) && CheckMouseClick())
			{
				OnClick();
			}
		}

		public virtual void OnClick()
		{
		}

		protected bool CheckMouseClick()
		{
			if (mainCamera == null) return false;

			Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

			RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

			if (hit.collider != null && hit.collider.gameObject == gameObject)
			{
				return true;
			}
			return false;
		}
	}
}