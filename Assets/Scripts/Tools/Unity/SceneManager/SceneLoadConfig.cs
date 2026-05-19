using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Tools.Unity
{
	/// <summary>
	/// 游戏场景加载模式。
	/// </summary>
	public enum GameSceneLoadMode
	{
		/// <summary>
		/// 单场景模式。
		/// 加载目标场景时会卸载当前场景。
		/// 适合：主菜单 -> 游戏场景、游戏场景 -> 结算场景。
		/// </summary>
		Single,

		/// <summary>
		/// 叠加模式。
		/// 加载目标场景时不会卸载当前场景。
		/// 适合：主场景 + UI 场景、主世界 + 副本场景、基础服务场景 + 玩法场景。
		/// </summary>
		Additive
	}

	/// <summary>
	/// 重复加载策略。
	/// 当目标场景已经被加载时，用这个枚举决定如何处理。
	/// </summary>
	public enum DuplicateScenePolicy
	{
		/// <summary>
		/// 忽略重复加载请求。
		/// 如果场景已经存在，就直接认为加载成功。
		/// </summary>
		Ignore,

		/// <summary>
		/// 重新加载场景。
		/// 适合需要重置当前关卡的情况。
		/// </summary>
		Reload,

		/// <summary>
		/// 抛出错误日志并终止加载。
		/// 适合开发期严格检查错误调用。
		/// </summary>
		Fail
	}

	/// <summary>
	/// 场景加载请求参数。
	/// 用一个类封装参数，避免 LoadScene 方法参数过多。
	/// </summary>
	[Serializable]
	public sealed class SceneLoadRequest
	{
		/// <summary>
		/// 目标场景名。
		/// 必须和 Unity Build Settings 里的场景名一致。
		/// 例如："MainMenu"、"BattleScene"、"LobbyScene"。
		/// </summary>
		public string TargetSceneName;

		/// <summary>
		/// 场景加载模式。
		/// Single 会替换当前场景。
		/// Additive 会叠加加载目标场景。
		/// </summary>
		public GameSceneLoadMode LoadMode = GameSceneLoadMode.Single;

		/// <summary>
		/// 是否在加载完成后把目标场景设置为 Active Scene。
		/// Additive 模式下尤其重要。
		/// 因为 Additive 加载后，Unity 不一定自动把新场景设为 Active Scene。
		/// </summary>
		public bool SetActiveSceneAfterLoaded = true;

		/// <summary>
		/// 是否允许加载完成后自动激活场景。
		/// true：加载完成后立刻进入目标场景。
		/// false：加载进度会停在 90%，需要手动调用 ActivatePendingScene() 才会进入目标场景。
		/// </summary>
		public bool AllowSceneActivation = true;

		/// <summary>
		/// 是否使用 Loading 场景中转。
		/// true：先进入 Loading 场景，再加载目标场景。
		/// false：直接加载目标场景。
		/// </summary>
		public bool UseLoadingScene = false;

		/// <summary>
		/// Loading 场景名。
		/// UseLoadingScene 为 true 时才会使用。
		/// 这个场景也必须加入 Unity Build Settings。
		/// </summary>
		public string LoadingSceneName = "Loading";

		/// <summary>
		/// 当目标场景已经加载时的处理策略。
		/// 默认 Ignore，可以防止重复点击按钮导致重复加载。
		/// </summary>
		public DuplicateScenePolicy DuplicatePolicy = DuplicateScenePolicy.Ignore;

		/// <summary>
		/// 加载进度回调。
		/// 参数 progress 的范围是 0 到 1。
		/// 0 表示刚开始加载，1 表示加载完成。
		/// 可用于更新 Loading UI 的进度条。
		/// </summary>
		public Action<float> OnProgress;

		/// <summary>
		/// 加载完成回调。
		/// 目标场景加载完成并激活后触发。
		/// </summary>
		public Action OnCompleted;

		/// <summary>
		/// 加载失败回调。
		/// 参数 errorMessage 表示失败原因。
		/// 可用于弹窗、日志上报或回退到安全场景。
		/// </summary>
		public Action<string> OnFailed;
	}
}