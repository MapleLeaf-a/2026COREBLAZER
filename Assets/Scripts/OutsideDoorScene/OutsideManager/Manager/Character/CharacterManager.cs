using Assets.Scripts.Tools.Common;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.Manager
{
	/// <summary>
	/// 我们就不搞角色池了，一个角色用到底
	/// </summary>
	internal class CharacterManager : MonoSingleton<CharacterManager>
	{
		public GameObject playerPref;

		//全场景只有一个Player
		public OutsideDoorCharacterController playerCache;

		public void OnCreatePlayer(in CreatePlayerEvent createPlayerEvent)
		{
			if (playerCache != null)
			{
				playerCache.ResetPlayerTransform(createPlayerEvent.pos, createPlayerEvent.facingSign > 0);
				playerCache.ResetRailMap2D(createPlayerEvent.rail);
				playerCache.enabled = true;
				return;
			}

			playerCache = GameObject.Instantiate(playerPref).GetComponent<OutsideDoorCharacterController>();
			playerCache.ResetPlayerTransform(createPlayerEvent.pos, createPlayerEvent.facingSign > 0);
			playerCache.ResetRailMap2D(createPlayerEvent.rail);
			Debug.Log("Player has been created");
			playerCache.enabled = true;
		}

		public void OnClosePlayerView(in ClosePlayerEvent createPlayerEvent)
		{
			if (playerCache != null)
			{
				playerCache.enabled = false;
			}
			Debug.LogError("Player is Null");
		}
	}
}