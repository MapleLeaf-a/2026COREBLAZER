using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TuningFork : MonoBehaviour
{
    [Header("检测设置")]
    [Tooltip("检测半径（像素）")]
    public float detectionRadius = 200f;
    [Tooltip("父canvas")]
    public Canvas canvas;
    
    [Tooltip("设置剩余文本")]
    public SetRemainingCountsText text;
    private int count = 5; //一共可挖次数

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        UpdateCountText();
    }

    private void Update()
    {
        rectTransform.position = Input.mousePosition;
        if (InputManager.instance.GetKeyDown("Dig"))
        {
            bool isGetItem = false;

            List<GameObject> hitUI = GetUIInCircle(Input.mousePosition, detectionRadius);

            foreach (GameObject ui in hitUI)
            {
                if (ui.CompareTag("ItemToBeExcavated"))
                {
                    ui.GetComponent<ItemToBeExcavated>().ShowSprite();

                    isGetItem = true;
                }
            }

            if (isGetItem) MinusCount();
        }
        else if (InputManager.instance.GetKeyDown("Detect"))
        {
            List<GameObject> hitUI = GetUIInCircle(Input.mousePosition, detectionRadius);

            foreach (GameObject ui in hitUI)
            {
                if (ui.CompareTag("ItemToBeExcavated"))
                {
                    ui.GetComponent<ItemToBeExcavated>().PlayLoopAnimation();
                }
            }
        }
    }

    public void MinusCount()
    {
        count--;
        UpdateCountText();
    }

    public void AddCount()
    {
        count++;
        UpdateCountText();
    }

    void UpdateCountText()
    {
        text.SetText(count);
    }

    /// <summary>
    /// 获取圆形区域内的所有UI元素
    /// </summary>
    public List<GameObject> GetUIInCircle(Vector2 screenCenter, float radius)
    {
        List<GameObject> result = new List<GameObject>();

        //Canvas下的所有可交互UI
        Graphic[] allGraphics = canvas.GetComponentsInChildren<Graphic>();

        foreach (Graphic graphic in allGraphics)
        {
            if (!graphic.raycastTarget) continue;

            //获取UI元素的屏幕位置
            Vector2 uiScreenPos = graphic.rectTransform.position;

            //计算距离
            float distance = Vector2.Distance(screenCenter, uiScreenPos);

            if (distance <= radius)
            {
                result.Add(graphic.gameObject);
            }
        }

        return result;
    }
}
