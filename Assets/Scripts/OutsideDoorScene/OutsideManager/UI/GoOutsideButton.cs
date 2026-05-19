using Assets.Scripts.OutsideDoorScene.OutsideManager.Manager;
using Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.UI
{
	internal class GoOutsideButton : WorldPlaceButton
	{
		public override void OnClick()
		{
			EventBus.Publish(new GoToOutsideEvent()
			{
				targetSceneName = "OutsideDoor_1"
			});
		}
	}
}