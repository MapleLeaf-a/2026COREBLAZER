using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
/*IBeginDragHandler - 开始拖拽时触发
IDragHandler - 拖拽过程中持续触发
IEndDragHandler - 结束拖拽时触发
IDropHandler - 在目标上放下时触发*/
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
