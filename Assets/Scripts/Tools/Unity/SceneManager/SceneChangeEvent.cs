using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Tools.Unity
{
	/// <summary>
	/// 场景加载请求事件。
	///
	/// 外部系统发布这个事件后，
	/// GameSceneManager 会接收到请求并开始加载场景。
	/// </summary>
	public readonly struct SceneLoadRequestEvent
	{
		/// <summary>
		/// 请求编号。
		///
		/// 作用：
		/// 用来区分不同加载请求。
		/// 例如 UI 收到 SceneLoadProgressEvent 时，可以知道这个进度属于哪一次加载。
		///
		/// 可以传：
		/// "MainMenuToGame"
		/// "ReloadBattle"
		/// "LobbyToCharacterSelect"
		/// </summary>
		public readonly string RequestId;

		/// <summary>
		/// 目标场景名。
		///
		/// 必须和 Unity Build Settings 里的场景名一致。
		/// 例如：
		/// "MainMenu"
		/// "Loading"
		/// "BattleScene"
		/// </summary>
		public readonly string TargetSceneName;

		/// <summary>
		/// 加载模式。
		/// Single 表示替换当前场景。
		/// Additive 表示叠加加载场景。
		/// </summary>
		public readonly GameSceneLoadMode LoadMode;

		/// <summary>
		/// 是否加载完成后设置为 Active Scene。
		///
		/// Active Scene：
		/// Unity 当前激活场景。
		/// 之后 Instantiate 创建出来的 GameObject 默认会进入 Active Scene。
		/// </summary>
		public readonly bool SetActiveSceneAfterLoaded;

		/// <summary>
		/// 是否允许场景加载完成后自动激活。
		///
		/// true：
		/// 加载完成后直接进入目标场景。
		///
		/// false：
		/// 加载进度会停在 90% 左右，
		/// 直到外部发布 SceneActivateRequestEvent 才进入目标场景。
		/// </summary>
		public readonly bool AllowSceneActivation;

		/// <summary>
		/// 是否使用 Loading 场景中转。
		///
		/// true：
		/// 先进入 Loading 场景，再加载目标场景。
		///
		/// false：
		/// 直接加载目标场景。
		/// </summary>
		public readonly bool UseLoadingScene;

		/// <summary>
		/// Loading 场景名。
		///
		/// 只有 UseLoadingScene 为 true 时才会使用。
		/// 这个场景也必须加入 Unity Build Settings。
		/// </summary>
		public readonly string LoadingSceneName;

		/// <summary>
		/// 目标场景已经加载时的处理策略。
		/// </summary>
		public readonly DuplicateScenePolicy DuplicatePolicy;

		/// <summary>
		/// 构造场景加载请求事件。
		/// </summary>
		/// <param name="requestId">
		/// 请求编号。
		/// 用于区分是哪一次加载请求。
		/// 如果项目暂时不需要区分，可以传固定值，例如 "Default"。
		/// </param>
		/// <param name="targetSceneName">
		/// 目标场景名。
		/// 必须和 Build Settings 里的场景名一致。
		/// </param>
		/// <param name="loadMode">
		/// 加载模式。
		/// Single 替换当前场景。
		/// Additive 叠加新场景。
		/// </param>
		/// <param name="setActiveSceneAfterLoaded">
		/// 加载完成后是否把目标场景设为 Active Scene。
		/// Additive 模式下通常建议设为 true。
		/// </param>
		/// <param name="allowSceneActivation">
		/// 是否允许加载完成后自动激活场景。
		/// 如果为 false，需要之后发布 SceneActivateRequestEvent。
		/// </param>
		/// <param name="useLoadingScene">
		/// 是否使用 Loading 场景中转。
		/// </param>
		/// <param name="loadingSceneName">
		/// Loading 场景名。
		/// useLoadingScene 为 true 时有效。
		/// </param>
		/// <param name="duplicatePolicy">
		/// 重复加载策略。
		/// 用于处理目标场景已经加载的情况。
		/// </param>
		public SceneLoadRequestEvent(
			string requestId,
			string targetSceneName,
			GameSceneLoadMode loadMode = GameSceneLoadMode.Single,
			bool setActiveSceneAfterLoaded = true,
			bool allowSceneActivation = true,
			bool useLoadingScene = false,
			string loadingSceneName = "Loading",
			DuplicateScenePolicy duplicatePolicy = DuplicateScenePolicy.Ignore)
		{
			RequestId = requestId;
			TargetSceneName = targetSceneName;
			LoadMode = loadMode;
			SetActiveSceneAfterLoaded = setActiveSceneAfterLoaded;
			AllowSceneActivation = allowSceneActivation;
			UseLoadingScene = useLoadingScene;
			LoadingSceneName = loadingSceneName;
			DuplicatePolicy = duplicatePolicy;
		}
	}

	/// <summary>
	/// 手动激活场景请求事件。
	///
	/// 当 SceneLoadRequestEvent.AllowSceneActivation 为 false 时，
	/// 需要发布这个事件来真正进入目标场景。
	/// </summary>
	public readonly struct SceneActivateRequestEvent
	{
		/// <summary>
		/// 请求编号。
		/// 用来匹配之前的 SceneLoadRequestEvent.RequestId。
		/// </summary>
		public readonly string RequestId;

		/// <summary>
		/// 构造手动激活场景事件。
		/// </summary>
		/// <param name="requestId">
		/// 请求编号。
		/// 应该和之前的加载请求编号一致。
		/// </param>
		public SceneActivateRequestEvent(string requestId)
		{
			RequestId = requestId;
		}
	}

	/// <summary>
	/// 场景卸载请求事件。
	///
	/// 外部系统发布这个事件后，
	/// GameSceneManager 会尝试卸载对应场景。
	/// </summary>
	public readonly struct SceneUnloadRequestEvent
	{
		/// <summary>
		/// 请求编号。
		/// 用来区分是哪一次卸载请求。
		/// </summary>
		public readonly string RequestId;

		/// <summary>
		/// 要卸载的场景名。
		/// 该场景必须已经被加载。
		/// </summary>
		public readonly string SceneName;

		/// <summary>
		/// 构造场景卸载请求事件。
		/// </summary>
		/// <param name="requestId">
		/// 请求编号。
		/// 用于区分不同卸载请求。
		/// </param>
		/// <param name="sceneName">
		/// 要卸载的场景名。
		/// </param>
		public SceneUnloadRequestEvent(string requestId, string sceneName)
		{
			RequestId = requestId;
			SceneName = sceneName;
		}
	}

	/// <summary>
	/// 场景加载进度事件。
	///
	/// GameSceneManager 在加载过程中发布这个事件。
	/// UI 可以订阅它来更新进度条。
	/// </summary>
	public readonly struct SceneLoadProgressEvent
	{
		/// <summary>
		/// 请求编号。
		/// 用来知道当前进度属于哪一次加载请求。
		/// </summary>
		public readonly string RequestId;

		/// <summary>
		/// 正在加载的场景名。
		/// </summary>
		public readonly string SceneName;

		/// <summary>
		/// 加载进度。
		/// 范围是 0 到 1。
		/// 0 表示刚开始。
		/// 1 表示加载完成。
		/// </summary>
		public readonly float Progress;

		/// <summary>
		/// 构造场景加载进度事件。
		/// </summary>
		/// <param name="requestId">
		/// 请求编号。
		/// </param>
		/// <param name="sceneName">
		/// 正在加载的场景名。
		/// </param>
		/// <param name="progress">
		/// 加载进度。
		/// 范围是 0 到 1。
		/// </param>
		public SceneLoadProgressEvent(string requestId, string sceneName, float progress)
		{
			RequestId = requestId;
			SceneName = sceneName;
			Progress = progress;
		}
	}

	/// <summary>
	/// 场景加载完成事件。
	///
	/// GameSceneManager 在目标场景加载并激活后发布这个事件。
	/// </summary>
	public readonly struct SceneLoadCompletedEvent
	{
		/// <summary>
		/// 请求编号。
		/// 用来知道是哪一次加载完成了。
		/// </summary>
		public readonly string RequestId;

		/// <summary>
		/// 已经加载完成的场景名。
		/// </summary>
		public readonly string SceneName;

		/// <summary>
		/// 构造场景加载完成事件。
		/// </summary>
		/// <param name="requestId">
		/// 请求编号。
		/// </param>
		/// <param name="sceneName">
		/// 已经加载完成的场景名。
		/// </param>
		public SceneLoadCompletedEvent(string requestId, string sceneName)
		{
			RequestId = requestId;
			SceneName = sceneName;
		}
	}

	/// <summary>
	/// 场景加载失败事件。
	///
	/// GameSceneManager 在加载失败时发布这个事件。
	/// </summary>
	public readonly struct SceneLoadFailedEvent
	{
		/// <summary>
		/// 请求编号。
		/// 用来知道是哪一次加载失败了。
		/// </summary>
		public readonly string RequestId;

		/// <summary>
		/// 加载失败的场景名。
		/// </summary>
		public readonly string SceneName;

		/// <summary>
		/// 错误信息。
		/// 用来告诉 UI 或日志系统失败原因。
		/// </summary>
		public readonly string ErrorMessage;

		/// <summary>
		/// 构造场景加载失败事件。
		/// </summary>
		/// <param name="requestId">
		/// 请求编号。
		/// </param>
		/// <param name="sceneName">
		/// 加载失败的场景名。
		/// </param>
		/// <param name="errorMessage">
		/// 错误信息。
		/// </param>
		public SceneLoadFailedEvent(
			string requestId,
			string sceneName,
			string errorMessage)
		{
			RequestId = requestId;
			SceneName = sceneName;
			ErrorMessage = errorMessage;
		}
	}

	/// <summary>
	/// 场景卸载完成事件。
	/// </summary>
	public readonly struct SceneUnloadCompletedEvent
	{
		/// <summary>
		/// 请求编号。
		/// 用来知道是哪一次卸载完成了。
		/// </summary>
		public readonly string RequestId;

		/// <summary>
		/// 已经卸载完成的场景名。
		/// </summary>
		public readonly string SceneName;

		/// <summary>
		/// 构造场景卸载完成事件。
		/// </summary>
		/// <param name="requestId">
		/// 请求编号。
		/// </param>
		/// <param name="sceneName">
		/// 已经卸载完成的场景名。
		/// </param>
		public SceneUnloadCompletedEvent(string requestId, string sceneName)
		{
			RequestId = requestId;
			SceneName = sceneName;
		}
	}
}