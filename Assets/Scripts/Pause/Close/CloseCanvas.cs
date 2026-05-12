using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseCanvas : MonoBehaviour
{
    [Header("关闭相关")]
    [Tooltip("关闭的按钮")]
    public Button button;
    [Tooltip("关闭的画布")]
    public Canvas canvas;

    protected virtual void Start()
    {
        button.onClick.AddListener(Close);
    }

    protected virtual void Close()
    { 
        canvas.gameObject.SetActive(false);
    }

}
