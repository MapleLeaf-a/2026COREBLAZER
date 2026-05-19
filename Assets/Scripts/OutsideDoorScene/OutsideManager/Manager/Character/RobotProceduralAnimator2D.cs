using UnityEngine;

/// <summary>
/// Robot 程序动画控制器。
///
/// 作用：
/// 1. 移动时控制角色显示节点上下浮动。
/// 2. 移动时控制角色显示节点进行 Z 轴轻微循环旋转。
/// 3. 停止移动后保留缓冲过渡，不让显示节点立刻回到 Idle 效果。
/// 4. Idle 状态下保持更轻微的上下浮动。
/// 5. 按键触发 Animator 中的 Scan 状态。
///
/// 挂载位置：
/// 推荐挂在 VisualPivot 节点上。
///
/// 注意：
/// 这个脚本只应该控制显示节点，不应该控制 Player 根节点。
/// </summary>
[DisallowMultipleComponent]
public sealed class RobotProceduralAnimator2D : MonoBehaviour
{
    [Header("Visual Root")]

    [SerializeField]
    private Transform visualRoot;
    // visualRoot：
    // 被程序动画实际控制的 Transform。
    // 推荐填 VisualPivot。
    // 如果为空，Awake 中会自动使用当前脚本所在的 transform。
    //
    // 这个脚本会修改 visualRoot.localPosition 和 visualRoot.localRotation。
    // localPosition 表示相对父节点的位置。
    // localRotation 表示相对父节点的旋转。

    [Header("Move Detect")]

    [SerializeField]
    private float moveThreshold = 0.01f;
    // moveThreshold：
    // 判断是否处于移动状态的输入阈值。
    // 输入幅度小于该值时，视为没有移动。
    // 作用是避免输入值很小时，程序动画在 Move 和 Idle 之间抖动。

    [Header("Idle Bob")]

    [SerializeField]
    private float idleBobAmplitude = 0.015f;
    // idleBobAmplitude：
    // Idle 状态下的上下浮动幅度。
    // 数值越大，上下浮动越明显。
    // Robot 静止时建议较小，例如 0.01 到 0.03。

    [SerializeField]
    private float idleBobFrequency = 1.4f;
    // idleBobFrequency：
    // Idle 状态下的上下浮动频率。
    // 表示每秒循环多少次。
    // 数值越大，Idle 浮动越快。

    [SerializeField]
    private float idleRotationAmplitude = 0.2f;
    // idleRotationAmplitude：
    // Idle 状态下的 Z 轴轻微旋转幅度。
    // 单位是角度。
    // 建议非常小，只作为轻微呼吸感。

    [Header("Move Bob")]

    [SerializeField]
    private float moveBobAmplitude = 0.06f;
    // moveBobAmplitude：
    // 移动状态下的上下浮动幅度。
    // 应该明显大于 idleBobAmplitude。
    // 数值越大，移动时漂浮感越强。

    [SerializeField]
    private float moveBobFrequency = 5.5f;
    // moveBobFrequency：
    // 移动状态下的上下浮动频率。
    // 表示每秒循环多少次。
    // 数值越大，移动时上下浮动越快。

    [SerializeField]
    private float moveRotationAmplitude = 2.0f;
    // moveRotationAmplitude：
    // 移动状态下的 Z 轴循环旋转幅度。
    // 单位是角度。
    // 例如 2.0 表示最多左右旋转 2 度。

    [Header("Blend And Buffer")]

    [SerializeField]
    private float moveBlendInSpeed = 12f;
    // moveBlendInSpeed：
    // 从 Idle 动效过渡到 Move 动效的速度。
    // 数值越大，开始移动时越快进入移动浮动状态。

    [SerializeField]
    private float moveBlendOutSpeed = 3.5f;
    // moveBlendOutSpeed：
    // 从 Move 动效回到 Idle 动效的速度。
    // 数值越小，停止移动后的缓冲越明显。
    // 这是停止移动后"慢慢收住"的核心参数。

    [SerializeField]
    private float positionSmoothTime = 0.045f;
    // positionSmoothTime：
    // 上下浮动位置的平滑时间。
    // 数值越大，浮动越软。
    // 数值太大时，动作会显得拖沓。

    [SerializeField]
    private float rotationSmoothTime = 0.055f;
    // rotationSmoothTime：
    // Z 轴旋转的平滑时间。
    // 数值越大，旋转变化越柔和。

    [SerializeField]
    private float stopKickAmplitude = 0.018f;
    // stopKickAmplitude：
    // 停止移动瞬间额外产生的缓冲位移幅度。
    // 用来让角色停止后不是立刻回到 Idle，而是有一个轻微收势。

    [SerializeField]
    private float stopKickDecaySpeed = 6f;
    // stopKickDecaySpeed：
    // 停止缓冲效果的衰减速度。
    // 数值越大，停止缓冲消失越快。
    // 数值越小，停止后的残余摆动越久。

    [Header("Scan Animator")]

    [SerializeField]
    private Animator animator;
    // animator：
    // Sprite 节点上的 Animator。
    // 现在 Animator 不再负责 Walk / Idle。
    // 它只负责播放 Scan 状态。

    [SerializeField]
    private string scanStateName = "Base Layer.Scan";
    // scanStateName：
    // Animator 中 Scan 状态的完整路径。
    // 如果 Scan 在默认层 Base Layer 下，就填 "Base Layer.Scan"。
    // 注意这里是 Animator State 名字，不是 .anim 文件名。

    [SerializeField]
    private int scanLayerIndex = 0;
    // scanLayerIndex：
    // Animator Layer 索引。
    // 0 通常表示 Base Layer。
    // 如果没有创建额外动画层，保持 0 即可。

    [SerializeField]
    private bool pauseScanAnimatorWhenFinished = true;
    // pauseScanAnimatorWhenFinished：
    // Scan 播放完成后是否暂停 Animator。
    // true 适合 Animator 只有 Scan 一个状态的情况。
    // 这样可以避免 Scan 自动循环或反复播放。

    private Vector2 moveInput;
    // moveInput：
    // 当前输入方向。
    // x 表示横向输入。
    // y 表示纵向输入。

    private Vector3 baseLocalPosition;
    // baseLocalPosition：
    // visualRoot 初始局部坐标。
    // 程序动画所有上下浮动都基于这个位置叠加。

    private Quaternion baseLocalRotation;
    // baseLocalRotation：
    // visualRoot 初始局部旋转。
    // 程序动画所有 Z 轴旋转都基于这个旋转叠加。

    private float phase;
    // phase：
    // 循环动画相位。
    // 可以理解为正弦波动画当前走到哪里了。
    // 它会随着时间不断增加。

    private float moveBlend;
    // moveBlend：
    // 当前 Move 动效权重。
    // 0 表示完全 Idle 动效。
    // 1 表示完全 Move 动效。
    // 停止移动时它会慢慢从 1 回到 0，从而形成缓冲。

    private float currentYOffset;
    // currentYOffset：
    // 当前实际应用到 visualRoot 上的 Y 轴偏移。

    private float currentZRotation;
    // currentZRotation：
    // 当前实际应用到 visualRoot 上的 Z 轴旋转角度。

    private float yVelocity;
    // yVelocity：
    // Mathf.SmoothDamp 内部使用的速度缓存。
    // 不需要外部赋值。

    private float zRotationVelocity;
    // zRotationVelocity：
    // Mathf.SmoothDampAngle 内部使用的角速度缓存。
    // 不需要外部赋值。

    private float stopKick;
    // stopKick：
    // 停止移动时的额外缓冲值。
    // 从 1 逐渐衰减到 0。

    private bool wasMoving;
    // wasMoving：
    // 上一帧是否处于移动状态。
    // 用于判断"刚刚停止移动"的瞬间。

    private int scanStateHash;
    // scanStateHash：
    // Scan 状态名转换后的 Hash。
    // Hash 可以理解为状态名的整数编号。
    // 使用 Hash 可以避免运行时反复处理字符串。

    private static readonly int ScanShortNameHash = Animator.StringToHash("Scan");
    // ScanShortNameHash：
    // Scan 短状态名转换后的 Hash。
    // 用于判断当前 Animator 是否正在播放 Scan。
    // shortNameHash 只比较状态短名，不包含 Base Layer。

    private bool scanPlaying;
    // scanPlaying：
    // 当前是否正在播放 Scan。
    // 用于在 Scan 播放完成后暂停 Animator。

    private void Awake()
    {
        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        baseLocalPosition = visualRoot.localPosition;
        baseLocalRotation = visualRoot.localRotation;

        scanStateHash = Animator.StringToHash(scanStateName);
    }

    private void OnEnable()
    {
        ResetProceduralPose();

        if (animator != null && pauseScanAnimatorWhenFinished)
        {
            // speed = 0：
            // 暂停 Animator。
            // 如果 Animator 只有 Scan 一个状态，可以避免它开场就自动播放完整 Scan。
            animator.speed = 0f;
        }
    }

    private void Update()
    {
        TickProceduralAnimation(Time.deltaTime);
        TickScanAnimator();
    }

    /// <summary>
    /// 设置当前移动输入。
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
    public void SetMoveInput(float horizontalInput, float verticalInput)
    {
        moveInput = new Vector2(horizontalInput, verticalInput);
    }

    /// <summary>
    /// 从头播放 Scan 动画。
    /// </summary>
    public void PlayScan()
    {
        if (animator == null)
        {
            return;
        }

        if (!animator.HasState(scanLayerIndex, scanStateHash))
        {
            Debug.LogWarning(
                $"Animator 找不到 Scan 状态：{scanStateName}。请检查 Animator Controller 中的 State 名称。",
                this);

            return;
        }

        // speed = 1：
        // 允许 Animator 正常播放。
        animator.speed = 1f;

        // Play：
        // 直接播放指定状态。
        //
        // 参数 1 scanStateHash：
        // 要播放的 Scan 状态。
        //
        // 参数 2 scanLayerIndex：
        // 要播放的动画层索引，0 通常是 Base Layer。
        //
        // 参数 3 0f：
        // 从动画开头播放。
        animator.Play(scanStateHash, scanLayerIndex, 0f);

        scanPlaying = true;
    }

    /// <summary>
    /// 重置程序动画姿态。
    /// </summary>
    public void ResetProceduralPose()
    {
        moveInput = Vector2.zero;
        phase = 0f;
        moveBlend = 0f;
        currentYOffset = 0f;
        currentZRotation = 0f;
        yVelocity = 0f;
        zRotationVelocity = 0f;
        stopKick = 0f;
        wasMoving = false;

        if (visualRoot != null)
        {
            visualRoot.localPosition = baseLocalPosition;
            visualRoot.localRotation = baseLocalRotation;
        }
    }

    /// <summary>
    /// 每帧更新上下浮动和 Z 轴旋转。
    /// </summary>
    /// <param name="deltaTime">
    /// deltaTime：
    /// 当前帧和上一帧之间的时间间隔。
    /// 使用它可以保证动画速度不受帧率影响。
    /// </param>
    private void TickProceduralAnimation(float deltaTime)
    {
        if (visualRoot == null)
        {
            return;
        }

        float moveAmount = moveInput.magnitude;
        bool isMoving = moveAmount > moveThreshold;

        if (wasMoving && !isMoving)
        {
            // 刚刚从移动变成停止时，给予一次停止缓冲。
            // stopKick 会在后面逐渐衰减。
            stopKick = 1f;
        }

        wasMoving = isMoving;

        float targetBlend = isMoving ? 1f : 0f;
        float blendSpeed = isMoving ? moveBlendInSpeed : moveBlendOutSpeed;

        // MoveTowards：
        // 让 moveBlend 逐渐靠近 targetBlend。
        // 移动时快速接近 1。
        // 停止时慢慢回到 0。
        moveBlend = Mathf.MoveTowards(
            moveBlend,
            targetBlend,
            blendSpeed * deltaTime);

        // Lerp：
        // 根据 moveBlend 在 Idle 参数和 Move 参数之间插值。
        // moveBlend 越接近 0，越接近 Idle 动效。
        // moveBlend 越接近 1，越接近 Move 动效。
        float frequency = Mathf.Lerp(
            idleBobFrequency,
            moveBobFrequency,
            moveBlend);

        float bobAmplitude = Mathf.Lerp(
            idleBobAmplitude,
            moveBobAmplitude,
            moveBlend);

        float rotationAmplitude = Mathf.Lerp(
            idleRotationAmplitude,
            moveRotationAmplitude,
            moveBlend);

        // phase 推进：
        // frequency 表示每秒循环次数。
        // Mathf.PI * 2f 表示一个完整正弦循环。
        phase += frequency * Mathf.PI * 2f * deltaTime;

        // 防止 phase 无限增大。
        if (phase > Mathf.PI * 2f)
        {
            phase -= Mathf.PI * 2f;
        }

        // stopKick 衰减：
        // 停止后的缓冲会逐渐消失。
        stopKick = Mathf.MoveTowards(
            stopKick,
            0f,
            stopKickDecaySpeed * deltaTime);

        float targetYOffset =
            Mathf.Sin(phase) * bobAmplitude;

        // 停止瞬间增加一点向下收势。
        // 负号表示往下压。
        targetYOffset += -stopKick * stopKickAmplitude;

        float targetZRotation =
            Mathf.Sin(phase + Mathf.PI * 0.5f) * rotationAmplitude;

        // SmoothDamp：
        // 平滑靠近目标 Y 偏移。
        // 它会让位置变化更柔和，并提供缓冲感。
        currentYOffset = Mathf.SmoothDamp(
            currentYOffset,
            targetYOffset,
            ref yVelocity,
            positionSmoothTime);

        // SmoothDampAngle：
        // 平滑靠近目标角度。
        // 它适合处理角度变化。
        currentZRotation = Mathf.SmoothDampAngle(
            currentZRotation,
            targetZRotation,
            ref zRotationVelocity,
            rotationSmoothTime);

        visualRoot.localPosition =
            baseLocalPosition + new Vector3(0f, currentYOffset, 0f);

        visualRoot.localRotation =
            baseLocalRotation * Quaternion.Euler(0f, 0f, currentZRotation);
    }

    /// <summary>
    /// 检查 Scan 是否播放完成。
    /// </summary>
    private void TickScanAnimator()
    {
        if (!scanPlaying || animator == null)
        {
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(scanLayerIndex);

        if (stateInfo.shortNameHash != ScanShortNameHash)
        {
            return;
        }

        if (stateInfo.normalizedTime < 1f)
        {
            return;
        }

        scanPlaying = false;

        if (pauseScanAnimatorWhenFinished)
        {
            // 播放完成后暂停 Animator。
            // 这适合 Animator 只有 Scan 一个状态的情况。
            animator.speed = 0f;
        }
    }
}
