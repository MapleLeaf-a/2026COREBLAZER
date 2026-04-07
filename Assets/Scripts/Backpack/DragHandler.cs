using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image targetImage;
    public float dragIconScale = 1.2f;
    public float dragAlpha = 0.8f;

    private BackpackView backpackView;
    private int slotIndex;
    private GameObject dragObject;
    private Canvas dragCanvas;  // 独立的拖拽 Canvas

    void Start()
    {
        backpackView = GetComponentInParent<BackpackView>();
        slotIndex = transform.GetSiblingIndex();

        // 创建独立的拖拽 Canvas（只创建一次）
        if (dragCanvas == null)
        {
            GameObject canvasObj = new GameObject("DragCanvas");
            dragCanvas = canvasObj.AddComponent<Canvas>();
            dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dragCanvas.sortingOrder = 9999;  // 最上层

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            DontDestroyOnLoad(canvasObj);
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

        // 独立 Canvas 直接用屏幕坐标
        dragRect.position = eventData.position;

        backpackView.OnDragStart(slotIndex);
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

        backpackView.OnDragEnd();
    }

    public void OnDrop(PointerEventData eventData)
    {
        backpackView.OnDrop(slotIndex);
    }
}