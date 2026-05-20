using Assets.Scripts.OutsideDoorScene.OutsideManager.Manager;
using Assets.Scripts.OutsideDoorScene.OutsideManager.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideDoor2
{
	internal class PurchasePointButton : WorldPlaceButton
	{
		public GameObject purchasePoint;

		public void OnClickPurchasePoint()
		{
			purchasePoint.SetActive(!purchasePoint.activeSelf);
		}

		public override void OnClick()
		{
			OnClickPurchasePoint();
		}
	}
}
