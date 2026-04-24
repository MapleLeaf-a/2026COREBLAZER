using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public PageStack<Canvas> canvasStack;


    //单例
    public static CanvasManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }

        canvasStack = new PageStack<Canvas>();

        //绑定事件处理
        canvasStack.OnPagePopped += OnPagePopped;
        canvasStack.OnPagePushed += OnPagePushed;
        canvasStack.OnPageActivated += OnPageActivated;
        canvasStack.OnPageDeactivated += OnPageDeactivated;
    }

    void OnDestroy()
    {
        if (canvasStack != null)
        {
            //解绑事件
            canvasStack.OnPagePopped -= OnPagePopped;
            canvasStack.OnPagePushed -= OnPagePushed;
            canvasStack.OnPageActivated -= OnPageActivated;
            canvasStack.OnPageDeactivated -= OnPageDeactivated;
            /*
             * 若不解除订阅:由于 事件发布者（PageStack）持有订阅者（UIManager）的引用，导致 CanvasManager 无法被 GC 回收（引用计数不为0）
             */
        }
    }

    void OnPagePopped(Canvas pageCanvas)
    {
        //只有Pop时才隐藏Canvas,不在停用里面写,防止Push时也隐藏了下面的页面
        pageCanvas.gameObject.SetActive(false);
    }

    void OnPagePushed(Canvas pageCanvas)
    {
        pageCanvas.overrideSorting = true;
        pageCanvas.sortingOrder = canvasStack.Count;
    }

    void OnPageActivated(Canvas pageCanvas)
    {
        pageCanvas.gameObject.SetActive(true);
        
        //确保最上层页面的射线检测是开启的
        EnableRaycasterForPage(pageCanvas);
    }

    void OnPageDeactivated(Canvas pageCanvas)
    {
        //页面被停用（不是被 Pop）时，不隐藏，但禁用射线检测
        DisableRaycasterForPage(pageCanvas);
    }

    //禁用单个Canvas的射线检测
    void DisableRaycasterForPage(Canvas canvas)
    {
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            raycaster.enabled = false;
        }
    }

    //启用单个Canvas的射线检测
    void EnableRaycasterForPage(Canvas canvas)
    {
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            raycaster.enabled = true;
        }
    }
}
