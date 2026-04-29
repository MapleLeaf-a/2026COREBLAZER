using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandlerEntry : DragHandler
{
    private int index;

    private static EditEntry editEntry;

    protected override void OnEnable()
    {
        base.OnEnable();
        editEntry = GetComponentInParent<EditEntry>();
        index = transform.GetSiblingIndex();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    { 
        base.OnBeginDrag(eventData);
        editEntry.OnDragBegin(index);
    }

    public override void OnDrop(PointerEventData eventData)
    {
        
    }
}
