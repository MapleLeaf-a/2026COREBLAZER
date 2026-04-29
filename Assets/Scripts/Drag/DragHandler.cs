using JSONInterpreter.Tokens.Implement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Tooltip("要被拖拽的图片组件")]
    public Image targetImage;
    public float dragIconScale = 1.2f;
    public float dragAlpha = 0.8f;

    protected GameObject dragObject;
    protected Canvas dragCanvas;

    protected GameObject canvasObj;

    protected virtual void OnEnable()
    {
        // 创建拖拽Canvas
        if (dragCanvas == null)
        {
            Canvas mainCanvas = GetComponentInParent<Canvas>().rootCanvas;

            canvasObj = new GameObject("DragCanvas");
            canvasObj.transform.SetParent(mainCanvas.transform.parent);
            canvasObj.transform.SetSiblingIndex(mainCanvas.transform.GetSiblingIndex() + 1);

            dragCanvas = canvasObj.AddComponent<Canvas>();
            dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dragCanvas.sortingOrder = 9999;

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
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

        //直接使用鼠标位置（因为DragCanvas是Overlay模式）
        dragRect.position = eventData.position;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            dragObject.transform.position = eventData.position;
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (dragObject != null)
            Destroy(dragObject);
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        
    }

    void OnDisable()
    {
        Destroy(canvasObj);
    }
}
