using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.Manager
{
	public struct GoToOutsideEvent
	{
		public string targetSceneName;
	}

	public struct ResetCharacterPowerEvent
	{
	}

	public struct SyncPowerSlider
	{
		public float currentCharacterPower;
	}

	public struct ForceReturnMainScene
	{
	}

	public struct CloseOutsideUIEvent
	{
	}

	public struct CreatePlayerEvent
	{
		public Vector2 pos;

		public float facingSign;

		public RailMap2DAsset rail;
	}

	public struct ClosePlayerEvent
	{
	}

	public struct OpenOutsideUIEvent
	{
	}

	public struct OpenPowerUseUpTipsEvent
	{
	}
}