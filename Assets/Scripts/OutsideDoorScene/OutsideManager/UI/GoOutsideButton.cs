using Assets.Scripts.OutsideDoorScene.OutsideManager.Manager;
using Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.UI
{
	internal class GoOutsideButton : WorldPlaceButton
	{
		public override void OnClick()
		{
			Debug.Log("场景切换按钮触发");
			EventBus.Publish(new SceneTransitionRequestEvent("OutsideDoor_1", null, 0.35f));
		}
	}
}