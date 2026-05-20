using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 2D 交互点。
///
/// 用法：
/// 1. 挂在 NPC、门、宝箱、提示牌等可交互对象上。
/// 2. 该对象需要有 Collider2D。
/// 3. 玩家按 E 检测到这个组件后，会调用 Interact。
/// 4. onInteract 可以在 Inspector 面板中绑定方法。
/// </summary>
[DisallowMultipleComponent]
public sealed class InteractionPoint2D : MonoBehaviour
{
	[Header("Interaction Info")]

	/// <summary>
	/// 交互点名称。
	///
	/// 作用：
	/// 方便在 Inspector 和 Debug 中识别这个交互点。
	/// 例如：饭店门、NPC_老板、宝箱_01。
	/// </summary>
	[SerializeField]
	private string interactionName = "Interaction Point";

	[Header("Interaction State")]

	/// <summary>
	/// 是否允许交互。
	///
	/// true：玩家按 E 时可以触发。
	/// false：玩家按 E 时会被忽略。
	///
	/// 用途：
	/// 比如门暂时锁住、NPC 暂时不能对话、宝箱已经打开。
	/// </summary>
	[SerializeField]
	private bool canInteract = true;

	[Header("Interaction Event")]

	/// <summary>
	/// 交互事件。
	///
	/// 作用：
	/// 玩家按 E 成功交互时，会调用这里绑定的方法。
	///
	/// 示例：
	/// - 门：绑定 OpenDoor 方法。
	/// - NPC：绑定 StartDialogue 方法。
	/// - 宝箱：绑定 OpenChest 方法。
	/// - 场景切换点：绑定 RequestSceneChange 方法。
	/// </summary>
	[SerializeField]
	private UnityEvent onInteract;

	/// <summary>
	/// 外部读取交互点名称。
	/// </summary>
	public string InteractionName => interactionName;

	/// <summary>
	/// 外部判断当前是否可以交互。
	/// </summary>
	public bool CanInteract => canInteract;

	/// <summary>
	/// 设置是否允许交互。
	///
	/// <param name="value">
	/// true 表示允许交互。
	/// false 表示禁止交互。
	/// </param>
	/// </summary>
	public void SetInteractable(bool value)
	{
		canInteract = value;
	}

	/// <summary>
	/// 执行交互。
	///
	/// <param name="interactor">
	/// 发起交互的对象。
	/// 通常是玩家 Player。
	/// 当前版本没有直接使用它，但保留这个参数方便后续扩展。
	/// 例如：根据不同玩家、不同角色状态，触发不同逻辑。
	/// </param>
	/// </summary>
	public void Interact(GameObject interactor)
	{
		// 如果当前交互点被禁用，则直接返回。
		// 这样可以避免已经打开的宝箱、锁住的门、不可对话 NPC 被重复触发。
		if (!canInteract)
		{
			return;
		}

		// 调用 Inspector 中绑定的方法。
		// UnityEvent 的好处是不用在代码里写死目标逻辑。
		// 门、NPC、宝箱、传送点都可以复用同一个 InteractionPoint2D。
		onInteract?.Invoke();
		Debug.Log("点击交互");
	}

	/// <summary>
	/// 从 RailNodeClickMoveTarget2D 的点击自动移动流程触发交互。
	///
	/// 作用：
	/// RailNodeClickMoveTarget2D 在自动移动完成并且碰撞盒重合后，
	/// 调用这个方法触发交互。
	///
	/// <param name="interactor">
	/// 发起交互的对象，一般是 Player。
	/// </param>
	/// </summary>
	public void TriggerInteractionFromClickMove(GameObject interactor)
	{
		// 复用已有的 Interact 方法，不新写第二套交互逻辑。
		Interact(interactor);
	}
}
