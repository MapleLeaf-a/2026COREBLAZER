using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPushAndPop : MonoBehaviour
{
    [Header("希望自动进行页面层级管理的目标Canvas")]
    public Canvas canvas;

    private void OnEnable()
    {
        if (CanvasManager.instance != null)
        {
            CanvasManager.instance.canvasStack.Push(canvas);

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
