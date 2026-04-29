using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class DragHandlerDescription : DragHandler
{
    private int slotIndex;

    private static EditEntry editEntry;

    protected override void OnEnable()
    {
        slotIndex = transform.GetSiblingIndex();
        editEntry = GetComponentInParent<EditEntry>(); 
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public override void OnDrop(PointerEventData eventData)
    {
        editEntry.OnDrop(slotIndex);
    }
}
