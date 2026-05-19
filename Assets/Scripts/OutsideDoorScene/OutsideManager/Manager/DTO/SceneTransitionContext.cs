using UnityEngine.SceneManagement;

namespace Assets.Scripts.OutsideDoorScene.OutsideManager.Manager
{
	/// <summary>
	/// 场景切换上下文。
	///
	/// 作用：
	/// 1. 记录 Player 是从哪个场景进入当前场景的。
	/// 2. 让新场景可以用"上一个场景名"去当前 RailMap2DAsset 中查找入口节点。
	/// 3. 避免把上一个场景名存在即将销毁的场景物体上。
	/// </summary>
	public static class SceneTransitionContext
	{
		/// <summary>
		/// 上一个场景名称。
		///
		/// 用途：
		/// 新场景会用它作为 RailNode2D.nodeKey 查询出生节点。
		/// </summary>
		public static string PreviousSceneName { get; private set; }

		/// <summary>
		/// 目标场景名称。
		///
		/// 用途：
		/// 用于调试和校验，判断当前加载完成的场景是否是本次切换的目标场景。
		/// </summary>
		public static string TargetSceneName { get; private set; }

		/// <summary>
		/// 是否存在有效切换上下文。
		///
		/// true：说明当前场景是通过切场景流程进入的，可以使用 PreviousSceneName 查出生节点。
		/// false：说明可能是直接从 Unity Editor 播放当前场景，需要走默认出生点。
		/// </summary>
		public static bool HasValidContext { get; private set; }

		/// <summary>
		/// 记录一次场景切换。
		/// </summary>
		/// <param name="targetSceneName">
		/// 目标场景名。
		/// 这个名字必须和 Unity Build Settings 中登记的场景名一致。
		/// </param>
		public static void RecordTransition(string targetSceneName)
		{
			// 在 LoadScene 之前调用 GetActiveScene，可以拿到当前还未卸载的旧场景名。
			PreviousSceneName = SceneManager.GetActiveScene().name;

			// 保存目标场景名，方便新场景调试校验。
			TargetSceneName = targetSceneName;

			// 只有旧场景名和目标场景名都有效，才认为这次切换上下文可用。
			HasValidContext = !string.IsNullOrWhiteSpace(PreviousSceneName)
			                  && !string.IsNullOrWhiteSpace(TargetSceneName);
		}

		/// <summary>
		/// 清理当前切换上下文。
		/// </summary>
		public static void Clear()
		{
			PreviousSceneName = string.Empty;
			TargetSceneName = string.Empty;
			HasValidContext = false;
		}
	}
}
