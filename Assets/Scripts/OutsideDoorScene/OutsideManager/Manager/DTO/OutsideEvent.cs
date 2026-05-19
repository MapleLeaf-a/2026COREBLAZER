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
		/// <summary>
		/// 兜底出生位置。
		///
		/// 作用：
		/// 如果 RailMap2DAsset 没有找到 spawnNodeKey 对应节点，
		/// 就把 Player 放到这个位置。
		/// </summary>
		public Vector2 fallbackPos;

		/// <summary>
		/// 角色朝向。
		///
		/// 作用：
		/// 大于 0 表示朝右，小于等于 0 表示朝左。
		/// </summary>
		public float facingSign;

		/// <summary>
		/// 当前场景使用的路径地图。
		///
		/// 作用：
		/// 赋值给 Player 上的 RailWalker2D，
		/// 让 Player 在当前场景的路径系统上移动。
		/// </summary>
		public RailMap2DAsset rail;

		/// <summary>
		/// 出生节点查询名。
		///
		/// 作用：
		/// 传给 RailWalker2D.TrySetStartAtNode。
		/// 本需求中它应该等于上一个场景名。
		/// </summary>
		public string spawnNodeKey;

		/// <summary>
		/// 出生后优先接入哪条出口路线。
		///
		/// 作用：
		/// 当一个出生节点连接了多条路径时，
		/// 用它决定优先选择 Auto / Left / Right / Up / Down 中的哪条路径。
		/// </summary>
		public RailExitChoice2D preferredExitChoice;
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