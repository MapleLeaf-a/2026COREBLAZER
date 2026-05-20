using UnityEngine;

/// <summary>
/// 玩家 2D 交互检测器。
/// 
/// 功能：
/// 1. 玩家按下 E 键时，检测 interactionArea 范围内的 Collider2D。
/// 2. 只筛选 targetLayers 指定层级上的对象。
/// 3. 如果对象身上有 InteractionPoint2D，则认为它是交互点。
/// 4. 默认选择距离玩家最近的交互点并触发。
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerInteractionDetector2D : MonoBehaviour
{
    [Header("Input")]

    /// <summary>
    /// 交互按键。
    /// 
    /// 默认是 E。
    /// 如果后续你接入新版 Input System，可以把这里替换成输入事件。
    /// </summary>
    [SerializeField]
    private KeyCode interactKey = KeyCode.E;

    [Header("Detection")]

    /// <summary>
    /// 玩家交互范围。
    /// 
    /// 这个 Collider2D 一般放在 Player 的子物体上。
    /// 推荐设置为 CircleCollider2D 或 BoxCollider2D。
    /// 
    /// 注意：
/// 这个碰撞体应该勾选 Is Trigger。
    /// 它不负责阻挡玩家，只负责表示“玩家能交互的范围”。
    /// </summary>
    [SerializeField]
    private Collider2D interactionArea;

    /// <summary>
    /// 可交互对象所在的层级。
    /// 
    /// 作用：
/// 只检测指定 Layer 的碰撞体，避免扫到玩家、地面、墙壁、装饰物。
    /// 
    /// 推荐：
/// 新建一个 Layer，叫 Interactable。
    /// 然后把 NPC、门、宝箱等对象放到这个 Layer。
    /// </summary>
    [SerializeField]
    private LayerMask targetLayers;

    /// <summary>
    /// 单次最多检测多少个碰撞体。
    /// 
    /// 作用：
/// 控制 overlapResults 数组容量。
    /// 这个数组会被复用，避免每次按 E 都产生 GC。
    /// 
    /// 一般场景中，玩家交互范围内不会同时有太多对象。
    /// 16 通常够用。
    /// </summary>
    [SerializeField]
    private int maxHitCount = 16;

    /// <summary>
    /// 是否输出调试日志。
    /// 
    /// true：按 E 时会打印检测结果。
    /// false：正式运行时建议关闭。
    /// </summary>
    [SerializeField]
    private bool debugLog = true;

    /// <summary>
    /// ContactFilter2D 是 Unity 2D 物理查询过滤器。
    /// 
    /// 作用：
/// 控制 OverlapCollider 要检测哪些层级、是否包含 Trigger。
    /// 
    /// 这里缓存它，避免每次按 E 都重新创建过滤配置。
    /// </summary>
    private ContactFilter2D contactFilter;

    /// <summary>
    /// OverlapCollider 的结果缓存数组。
    /// 
    /// 作用：
/// 存放 interactionArea 范围内检测到的 Collider2D。
    /// 
    /// 使用数组而不是 List，是为了减少运行时 GC 分配。
    /// </summary>
    private Collider2D[] overlapResults;

    private void Awake()
    {
        // 如果没有手动指定最大检测数量，则给一个安全值。
        // 防止 maxHitCount 被错误设置成 0 或负数。
        if (maxHitCount <= 0)
        {
            maxHitCount = 16;
        }

        // 初始化检测结果数组。
        // 后续每次按 E 都复用这个数组。
        overlapResults = new Collider2D[maxHitCount];

        // 初始化 2D 物理查询过滤器。
        contactFilter = new ContactFilter2D();

        // 启用 LayerMask 过滤。
        // 只有 targetLayers 中包含的层级才会被检测。
        contactFilter.useLayerMask = true;

        // 设置允许检测的层级。
        contactFilter.SetLayerMask(targetLayers);

        // 允许检测 Trigger。
        // 因为很多交互点的 Collider2D 会设置为 Is Trigger。
        contactFilter.useTriggers = true;

        // 关闭深度过滤。
        // 2D 项目一般不需要按 Z 深度过滤交互对象。
        contactFilter.useDepth = false;
    }

    private void Update()
    {
        // GetKeyDown 表示“按下这一帧”才触发。
        // 这样可以避免玩家长按 E 时每一帧都重复交互。
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    /// <summary>
    /// 尝试执行交互。
    /// 
    /// 逻辑：
/// 1. 检查 interactionArea 是否存在。
    /// 2. 扫描 interactionArea 内部重叠的碰撞体。
    /// 3. 找出最近的 InteractionPoint2D。
    /// 4. 调用该交互点的 Interact 方法。
    /// </summary>
    private void TryInteract()
    {
        if (interactionArea == null)
        {
            Debug.LogWarning($"{nameof(PlayerInteractionDetector2D)} 缺少 interactionArea。请在 Inspector 中绑定玩家交互范围 Collider2D。");
            return;
        }

        // 清理上一次检测残留的结果。
        // 因为 overlapResults 是复用数组，如果不清理，旧结果可能还留在数组中。
        for (int i = 0; i < overlapResults.Length; i++)
        {
            overlapResults[i] = null;
        }

        // 检测 interactionArea 当前范围内重叠的 Collider2D。
        // 参数 contactFilter：控制检测哪些 Layer、是否检测 Trigger。
        // 参数 overlapResults：用于接收检测结果，避免运行时产生额外 List 分配。
        // 返回值 hitCount：实际检测到的碰撞体数量。
        int hitCount = interactionArea.OverlapCollider(contactFilter, overlapResults);

        if (debugLog)
        {
            Debug.Log($"交互检测命中数量：{hitCount}");
        }

        InteractionPoint2D nearestPoint = null;
        float nearestDistanceSqr = float.MaxValue;

        // 遍历本次检测到的所有碰撞体。
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = overlapResults[i];

            // 理论上 hitCollider 不应该为空。
            // 这里保留空判断，是为了防止数组状态异常导致报错。
            if (hitCollider == null)
            {
                continue;
            }

            // TryGetComponent 表示：
            // 尝试从当前碰撞体所在对象上获取 InteractionPoint2D。
            // 如果没有这个组件，就说明它不是我们指定的交互点。
            if (!hitCollider.TryGetComponent(out InteractionPoint2D interactionPoint))
            {
                continue;
            }

            // 如果交互点当前不可交互，则跳过。
            if (!interactionPoint.CanInteract)
            {
                continue;
            }

            // 使用平方距离进行比较。
            // 原因：Vector2.Distance 内部会开平方，开平方比普通乘加更贵。
            // 这里只比较远近，不需要真实距离，所以用 sqrMagnitude 更合适。
            float distanceSqr = ((Vector2)interactionPoint.transform.position - (Vector2)transform.position).sqrMagnitude;

            // 记录距离玩家最近的交互点。
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestPoint = interactionPoint;
            }
        }

        // 如果没有找到任何可交互点，直接结束。
        if (nearestPoint == null)
        {
            if (debugLog)
            {
                Debug.Log("交互范围内没有可用的 InteractionPoint2D。");
            }

            return;
        }

        if (debugLog)
        {
            Debug.Log($"触发交互点：{nearestPoint.InteractionName}");
        }

        nearestPoint.Interact(gameObject);
    }
}