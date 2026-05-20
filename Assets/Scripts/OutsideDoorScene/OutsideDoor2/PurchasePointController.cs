using System.Collections;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideDoor2
{
	public class PurchasePointController : MonoBehaviour
	{
		public GameObject purchasePoint;

		public void OnClickPurchasePoint()
		{
			purchasePoint.SetActive(!purchasePoint.activeSelf);
		}
	}
}
