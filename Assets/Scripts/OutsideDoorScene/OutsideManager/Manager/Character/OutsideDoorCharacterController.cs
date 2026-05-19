using UnityEngine;

/// <summary>
/// OutsideDoor 场景角色控制器。
///
/// 作用：
/// 1. 读取 Unity 老 Input 系统。
/// 2. 根据横向输入控制角色左右朝向。
/// 3. 把移动输入传给 RailWalker2D。
/// 4. 把移动输入传给 RobotProceduralAnimator2D。
/// 5. 通过按键触发 Scan 动画。
/// </summary>
[DisallowMultipleComponent]
public sealed class OutsideDoorCharacterController : MonoBehaviour
{
	[Header("Rail Movement")]

	/// <summary>
	/// 路径移动组件。
	/// 真正的位置移动由它处理。
	/// </summary>
	[SerializeField]
	private RailWalker2D railWalker;

	[Header("Old Input System")]

	/// <summary>
	/// Unity 老 Input 系统里的横向轴名称。
	/// 默认 Horizontal 通常对应 A/D 和左右方向键。
	/// </summary>
	[SerializeField]
	private string horizontalAxisName = "Horizontal";

	/// <summary>
	/// Unity 老 Input 系统里的纵向轴名称。
	/// 默认 Vertical 通常对应 W/S 和上下方向键。
	/// </summary>
	[SerializeField]
	private string verticalAxisName = "Vertical";

	[Header("Visual Facing")]

	/// <summary>
	/// 角色 SpriteRenderer。
	/// 用于根据左右输入翻转角色朝向。
	/// </summary>
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	/// <summary>
	/// 是否根据左右输入翻转 SpriteRenderer.flipX。
	/// </summary>
	[SerializeField]
	private bool flipSpriteByMoveDirection = true;

	[Header("Scan Animator")]

	/// <summary>
	/// Sprite 节点上的 Animator。
	/// 当前只负责 Scan 动画。
	/// 这里保留字段是为了兼容已有 Inspector 绑定。
	/// 实际 Scan 触发由 RobotProceduralAnimator2D 处理。
	/// </summary>
	[SerializeField]
	private Animator animator;

	[Header("Robot Procedural Animation")]

	/// <summary>
	/// Robot 程序动画控制器。
	/// 它负责上下浮动、Z 轴旋转、停止缓冲、Idle 轻微浮动和 Scan 触发。
	/// </summary>
	[SerializeField]
	private RobotProceduralAnimator2D robotProceduralAnimator;

	/// <summary>
	/// 触发 Scan 动画的按键。
	/// 默认 E 键。
	/// </summary>
	[SerializeField]
	private KeyCode scanKey = KeyCode.E;

	private float cachedHorizontalInput;
	private float cachedVerticalInput;
	private int facingSign = 1;

	/// <summary>
	/// 暴露给 Editor 工具使用。
	/// Editor 可以通过这个属性找到 RailWalker2D 并设置出生路径。
	/// </summary>
	public RailWalker2D Walker
	{
		get { return railWalker; }
	}

	private void Awake()
	{
		if (railWalker == null)
		{
			railWalker = GetComponent<RailWalker2D>();
		}

		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		}

		if (animator == null)
		{
			animator = GetComponentInChildren<Animator>();
		}

		if (robotProceduralAnimator == null)
		{
			robotProceduralAnimator = GetComponentInChildren<RobotProceduralAnimator2D>();
		}
	}

	private void OnEnable()
	{
		// 如果项目里必须使用自定义 InputManager 上下文，可以在这里打开。
		// InputManager.instance.SetContext(InputContext.CHARACTER);
	}

	private void Update()
	{
		ReadOldInput();

		UpdateFacingByInput(cachedHorizontalInput);

		UpdateRobotProceduralAnimation(
			cachedHorizontalInput,
			cachedVerticalInput);

		UpdateScanInput();
	}

	private void FixedUpdate()
	{
		if (railWalker == null)
		{
			return;
		}

		railWalker.TickMove(
			cachedHorizontalInput,
			cachedVerticalInput,
			Time.fixedDeltaTime);
	}

	public void ResetPlayerTransform(
		Vector3 worldPosition,
		bool faceRight,
		bool resetInput = true)
	{
		// 直接设置 Player 根节点的世界坐标。
		// 因为 OutsideDoorCharacterController 挂在 Player 根节点上，
		// 所以这里使用 transform.position 即可。
		transform.position = worldPosition;
		Debug.Log($"当前角色的位置 :{transform.position}");

		// 根据需要清空输入缓存。
		// 如果不清空，角色可能在重置后的下一帧继续沿着旧输入移动。
		if (resetInput)
		{
			cachedHorizontalInput = 0f;
			cachedVerticalInput = 0f;
		}

		// 更新内部朝向标记。
		// 你的原代码里：
		// facingSign < 0 时，spriteRenderer.flipX = true。
		// 所以这里保持同一套规则。
		facingSign = faceRight ? -1 : 1;

		// 如果有 SpriteRenderer，就立刻刷新显示朝向。
		if (spriteRenderer != null)
		{
			spriteRenderer.flipX = facingSign < 0;
		}

		// 重置程序动画姿态。
		// 让显示节点回到初始位置和旋转。
		if (robotProceduralAnimator != null)
		{
			robotProceduralAnimator.ResetProceduralPose();
		}
	}

	public void ResetRailMap2D(RailMap2DAsset rail)
	{
		railWalker.ResetMapData(rail);
	}

	/// <summary>
	/// 重置角色的 RailMap，并把角色接入指定出生节点。
	/// </summary>
	/// <param name="rail">
	/// 当前场景的 RailMap2DAsset。
	/// 作用是提供当前场景的节点、路径段和出口规则。
	/// </param>
	/// <param name="spawnNodeKey">
	/// 出生节点查询名。
	/// 作用是让 RailWalker2D 在 rail 中查找对应 RailNode2D。
	/// </param>
	/// <param name="fallbackPosition">
	/// 兜底世界坐标。
	/// 当 spawnNodeKey 查不到，或者节点无法接入路径时使用。
	/// </param>
	/// <param name="preferredExitChoice">
	/// 优先出口选择。
	/// 当出生节点连接多条路径时，用它决定优先接入哪条路径。
	/// </param>
	/// <param name="faceRight">
	/// 是否朝右。
	/// true 表示朝右，false 表示朝左。
	/// </param>
	public void ResetRailMapAndSpawnAtNode(
		RailMap2DAsset rail,
		string spawnNodeKey,
		Vector2 fallbackPosition,
		RailExitChoice2D preferredExitChoice,
		bool faceRight)
	{
		if (railWalker == null)
		{
			Debug.LogError("Player 缺少 RailWalker2D，无法接入当前场景 RailMap。");
			ResetPlayerTransform(fallbackPosition, faceRight);
			return;
		}

		if (rail == null)
		{
			Debug.LogError("传入的 RailMap2DAsset 为空，无法接入当前场景路径。");
			ResetPlayerTransform(fallbackPosition, faceRight);
			return;
		}

		// 先把当前场景的 RailMap 导入给角色。
		// 这样 TrySetStartAtNode 才会在当前场景的节点列表中查找。
		railWalker.ResetMapData(rail);

		bool hasSpawnedOnRail = false;

		if (!string.IsNullOrWhiteSpace(spawnNodeKey))
		{
			// 让 RailWalker2D 自己根据节点接入 Segment。
			// 这一步会同步 currentSegmentId 和 distanceOnSegment，
			// 比单纯设置 transform.position 更安全。
			hasSpawnedOnRail = railWalker.TrySetStartAtNode(
				spawnNodeKey,
				preferredExitChoice,
				true);
		}

		if (!hasSpawnedOnRail)
		{
			// 如果没有成功接入指定节点，就退回兜底坐标。
			ResetPlayerTransform(fallbackPosition, faceRight);

			// 尝试把兜底坐标吸附到最近的 Rail 上。
			// 这样即使节点配置错了，角色也尽量还能继续沿路径移动。
			railWalker.TryAttachToNearestRail(fallbackPosition, true);
			return;
		}

		// 成功按节点接入路径后，只刷新朝向和输入缓存。
		// transform.position 使用当前 railWalker 已经移动到的位置。
		ResetPlayerTransform(fallbackPosition, faceRight);
	}

	private void ReadOldInput()
	{
		cachedHorizontalInput = Input.GetAxisRaw(horizontalAxisName);
		cachedVerticalInput = Input.GetAxisRaw(verticalAxisName);
		//cachedVerticalInput = 0;
		//cachedHorizontalInput = 0;
		//if (InputManager.instance.GetKey("MoveUp"))
		//{
		//	cachedVerticalInput += 1;
		//}
		//if (InputManager.instance.GetKey("MoveDown"))
		//{
		//	cachedVerticalInput += -1;
		//}
		//if (InputManager.instance.GetKey("MoveLeft"))
		//{
		//	cachedHorizontalInput += -1;
		//}
		//if (InputManager.instance.GetKey("MoveRight"))
		//{
		//	cachedHorizontalInput += 1;
		//}
	}

	private void UpdateFacingByInput(float horizontalInput)
	{
		if (!flipSpriteByMoveDirection || spriteRenderer == null)
		{
			return;
		}

		if (horizontalInput > 0.01f)
		{
			facingSign = -1;
		}
		else if (horizontalInput < -0.01f)
		{
			facingSign = 1;
		}

		spriteRenderer.flipX = facingSign < 0;
	}

	/// <summary>
	/// 根据移动输入更新 Robot 程序动画。
	/// </summary>
	/// <param name="horizontalInput">
	/// horizontalInput：
	/// 横向输入。
	/// 大于 0 表示向右。
	/// 小于 0 表示向左。
	/// 等于 0 表示没有横向输入。
	/// </param>
	/// <param name="verticalInput">
	/// verticalInput：
	/// 纵向输入。
	/// 大于 0 表示向上。
	/// 小于 0 表示向下。
	/// 等于 0 表示没有纵向输入。
	/// </param>
	private void UpdateRobotProceduralAnimation(
		float horizontalInput,
		float verticalInput)
	{
		if (robotProceduralAnimator == null)
		{
			return;
		}

		robotProceduralAnimator.SetMoveInput(
			horizontalInput,
			verticalInput);
	}

	/// <summary>
	/// 检查 Scan 输入。
	/// </summary>
	private void UpdateScanInput()
	{
		if (robotProceduralAnimator == null)
		{
			return;
		}

		// GetKeyDown：
		// 只在按键按下的那一帧返回 true。
		// Scan 是一次性触发动作，所以不能用 GetKey。
		if (Input.GetKeyDown(scanKey))
		{
			robotProceduralAnimator.PlayScan();
		}
	}
}