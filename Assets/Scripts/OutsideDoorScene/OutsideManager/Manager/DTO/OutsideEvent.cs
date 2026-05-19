using UnityEngine;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.Manager
{
	public struct GoToOutsideEvent
	{
		/// <summary>
		/// 目标场景名。
		///
		/// 作用：
		/// SceneManager 根据这个名字加载目标场景。
		/// 这个名字必须和 Unity Build Settings 中登记的场景名一致。
		/// </summary>
		public string targetSceneName;
	}

	public struct ResetCharacterPowerEvent
	{
	}

	public struct SyncPowerSlider
	{
		/// <summary>
		/// 当前角色户外行动能量。
		///
		/// 作用：
		/// 用于同步 UI 能量条显示。
		/// </summary>
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
		/// 角色预制体在 Resources 目录下的加载路径。
		///
		/// 作用：
		/// 1. CharacterManager 用它作为缓存 key。
		/// 2. 如果缓存中没有该 key，则用 Resources.Load<GameObject>() 加载预制体。
		/// 3. 如果缓存中已经存在该 key，则直接复用已经 Instantiate 出来的对象。
		///
		/// 示例：
		/// 真实路径：
		/// Assets/Resources/Characters/OutsidePlayer.prefab
		///
		/// 字段值：
		/// Characters/OutsidePlayer
		///
		/// 注意：
		/// 不要填写 Assets/Resources。
		/// 不要填写 .prefab 后缀。
		/// </summary>
		public string characterResourcePath;

		/// <summary>
		/// 兜底出生位置。
		///
		/// 作用：
		/// 如果 RailMap2DAsset 没有找到 spawnNodeKey 对应节点，
		/// 就把 Player 放到这个世界坐标。
		/// </summary>
		public Vector2 fallbackPos;

		/// <summary>
		/// 角色朝向。
		///
		/// 作用：
		/// 大于 0 表示朝右。
		/// 小于等于 0 表示朝左。
		/// </summary>
		public float facingSign;

		/// <summary>
		/// 当前场景使用的路径地图。
		///
		/// 作用：
		/// 赋值给 Player 上的 RailWalker2D，
		/// 让 Player 使用当前场景的节点、路径段和分支出口数据。
		/// </summary>
		public RailMap2DAsset rail;

		/// <summary>
		/// 出生节点查询名。
		///
		/// 作用：
		/// 传给 RailWalker2D.TrySetStartAtNode。
		/// 当前设计中，它通常等于上一个场景名。
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
		/// <summary>
		/// 要关闭的角色 Resources 路径。
		///
		/// 作用：
		/// 1. 如果该字段不为空，则 CharacterManager 关闭指定 key 对应的角色对象。
		/// 2. 如果该字段为空，则 CharacterManager 关闭当前正在激活的角色对象。
		///
		/// 注意：
		/// 关闭不是 Destroy，而是 SetActive(false)，方便下次复用。
		/// </summary>
		public string characterResourcePath;
	}

	public struct OpenOutsideUIEvent
	{
	}

	public struct OpenPowerUseUpTipsEvent
	{
	}
}
