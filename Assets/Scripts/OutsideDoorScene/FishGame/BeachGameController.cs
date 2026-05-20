using Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.FishGame
{
	internal class BeachGameController : MonoBehaviour
	{
		public void OnClickBeachGame()
		{
			EventBus.Publish(new SceneTransitionRequestEvent("MiniGame_Dig", null, 0.35f));
		}

		public void OnClickFishGame()
		{
			EventBus.Publish(new SceneTransitionRequestEvent("MiniGame_Music", null, 0.35f));
		}
	}
}