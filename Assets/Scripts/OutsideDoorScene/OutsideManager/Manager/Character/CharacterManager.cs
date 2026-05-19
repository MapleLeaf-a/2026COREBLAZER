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
			if (playerCache == null)
			{
				if (playerPref == null)
				{
					Debug.LogError("CharacterManager 缺少 playerPref，无法创建 Player。");
					return;
				}

				playerCache = GameObject.Instantiate(playerPref).GetComponent<OutsideDoorCharacterController>();

				if (playerCache == null)
				{
					Debug.LogError("playerPref 上缺少 OutsideDoorCharacterController。");
					return;
				}

				Debug.Log("Player has been created");
			}

			playerCache.ResetRailMapAndSpawnAtNode(
				createPlayerEvent.rail,
				createPlayerEvent.spawnNodeKey,
				createPlayerEvent.fallbackPos,
				createPlayerEvent.preferredExitChoice,
				createPlayerEvent.facingSign > 0f);

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