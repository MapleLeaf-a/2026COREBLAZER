namespace GameScene
{
	/// <summary>
	/// 场景切换触发方向。
	///
	/// None 表示不检查方向，只要玩家进入触发盒并发生有效移动就可以触发。
	/// Left / Right / Up / Down 表示玩家必须朝对应方向移动才可以触发。
	/// </summary>
	public enum SceneTransitionDirection
	{
		None = 0,
		Left = 1,
		Right = 2,
		Up = 3,
		Down = 4
	}
}
