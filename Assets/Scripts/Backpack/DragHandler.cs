using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
/*IBeginDragHandler - 开始拖拽时触发
IDragHandler - 拖拽过程中持续触发
IEndDragHandler - 结束拖拽时触发
IDropHandler - 在目标上放下时触发*/
{
    public Image targetImage;
    public float dragIconScale = 1.2f;
    public float dragAlpha = 0.8f;

    private BackpackView<BackpackViewModel> backpackView;
    private int slotIndex;
    private GameObject dragObject;
    private Canvas dragCanvas;

    private GameObject canvasObj;

    void OnEnable()
    {
        backpackView = GetComponentInParent<BackpackView<BackpackViewModel>>();
        slotIndex = transform.GetSiblingIndex();

        // 创建拖拽Canvas
        if (dragCanvas == null)
        {
            Canvas mainCanvas = GetComponentInParent<Canvas>().rootCanvas;

            canvasObj = new GameObject("DragCanvas");
            canvasObj.transform.SetParent(mainCanvas.transform.parent);
            canvasObj.transform.SetSiblingIndex(mainCanvas.transform.GetSiblingIndex() + 1);

            dragCanvas = canvasObj.AddComponent<Canvas>();
            dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;  // 用Overlay最简单
            dragCanvas.sortingOrder = 9999;

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (targetImage == null || targetImage.sprite == null) return;

        dragObject = new GameObject("DragIcon");
        dragObject.transform.SetParent(dragCanvas.transform);
        dragObject.transform.SetAsLastSibling();

        var img = dragObject.AddComponent<Image>();
        img.sprite = targetImage.sprite;
        img.raycastTarget = false;

        var canvasGroup = dragObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = dragAlpha;

        RectTransform dragRect = dragObject.GetComponent<RectTransform>();
        RectTransform sourceRect = targetImage.GetComponent<RectTransform>();

        dragRect.sizeDelta = sourceRect.sizeDelta * dragIconScale;
        dragRect.pivot = new Vector2(0.5f, 0.5f);

        // 直接使用鼠标位置（因为DragCanvas是Overlay模式）
        dragRect.position = eventData.position;

        backpackView.OnDragStart(slotIndex); //通知BackpackView:开始拖拽,槽位是slotIndex


        DragState.FromIndex = slotIndex;
        DragState.SourceView = backpackView;   //在DragHandler中设置开始拖拽时静态类状态,保证View不做业务逻辑
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            dragObject.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragObject != null)
            Destroy(dragObject);

        backpackView.OnDragEnd();  //通知BackpackView:拖拽结束

        DragState.Reset();                    //结束,清空状态
    }

    public void OnDrop(PointerEventData eventData)
    {
        backpackView.OnDrop(slotIndex); //通知BackpackView:在slotIndex这个槽位上放下了物品
                                        //(并非同一个脚本内调用这个方法,比如说开始拖拽时在0,0会调用OnDragStart,鼠标在3处松开,那么对应的3就会调用OnDrop)
    }

    void OnDisable()
    {
        Destroy(canvasObj);
    }
}