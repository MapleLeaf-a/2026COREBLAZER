using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPushAndPop : MonoBehaviour
{
    public bool isDeactivated = true;

    //希望自动进行页面层级管理的目标Canvas
    private Canvas canvas;

    private void OnEnable()
    {
        canvas = GetComponent<Canvas>();
        if (CanvasManager.instance != null)
        {
            CanvasManager.instance.canvasStack.Push(canvas, isDeactivated);

        }
        else
        {
            Debug.LogWarning("CanvasManager单例不存在");
        }
    }

    private void OnDisable()
    {
        if (CanvasManager.instance != null)
        {
            CanvasManager.instance.canvasStack.PopTo(canvas);

        }
        else
        {
            Debug.LogWarning("CanvasManager单例不存在");
        }
    }
}
