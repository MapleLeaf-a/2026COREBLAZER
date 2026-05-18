using UnityEngine;

/// <summary>
/// OutsideDoor 场景角色控制器。
/// 它负责读取 Unity 老 Input 系统、更新朝向和动画，再把输入交给 RailWalker2D。
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

	[Header("Visual")]

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

	/// <summary>
	/// 角色 Animator。
	/// 如果暂时没有动画，可以留空。
	/// </summary>
	[SerializeField]
	private Animator animator;

	/// <summary>
	/// Animator 中用于表示移动速度的 Float 参数名。
	/// </summary>
	[SerializeField]
	private string animatorMoveSpeedParameter = "MoveSpeed";

	/// <summary>
	/// Animator 中用于表示是否移动的 Bool 参数名。
	/// </summary>
	[SerializeField]
	private string animatorIsMovingParameter = "IsMoving";

	/// <summary>
	/// 是否写入 Animator 参数。
	/// 如果 Animator 没有对应参数，应该关闭。
	/// </summary>
	[SerializeField]
	private bool updateAnimatorParameters = false;

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
	}

	private void OnEnable()
	{
		// 如果项目里必须使用自定义 InputManager 上下文，可以在这里打开。
		// InputManager.instance.SetContext(InputContext.CHARACTER);
		// InputManager.instance.SetContext(InputContext.CHARACTER);
	}

	private void Update()
	{
		ReadOldInput();
		UpdateFacingByInput(cachedHorizontalInput);
		UpdateAnimatorByInput(cachedHorizontalInput);
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

	private void ReadOldInput()
	{
		cachedHorizontalInput = Input.GetAxisRaw(horizontalAxisName);
		cachedVerticalInput = Input.GetAxisRaw(verticalAxisName);

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

	private void UpdateAnimatorByInput(float horizontalInput)
	{
		if (!updateAnimatorParameters || animator == null)
		{
			return;
		}

		float moveAmount = Mathf.Abs(horizontalInput);
		bool isMoving = moveAmount > 0.01f;

		animator.SetFloat(animatorMoveSpeedParameter, moveAmount);
		animator.SetBool(animatorIsMovingParameter, isMoving);
	}
}