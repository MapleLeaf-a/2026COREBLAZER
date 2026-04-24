using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    
    protected bool hasTriggerZone = true;      //是否有触发区
    protected bool hasBlockZone = false;       //是否有阻挡区
    protected bool isPlayerInRange = false;    //玩家是否在触发区

    [Header("触发区")]
    public Collider2D triggerCollider;
    [Header("阻挡区")]
    public Collider2D blockCollider;

    //交互文本提示
    protected string interactPrompt = "按F交互";
    protected string actionName = "InteractF"; //KeyCode.F;
    //UI
    public TextMeshProUGUI promptText;
    public Canvas promptPanel;

    //交互逻辑
    public abstract void InteractLogics();

    public virtual void Update()
    {
        if (hasTriggerZone && isPlayerInRange) //处在触发区
        {
            if (InputManager.InputManagerInstance.currenContext == InputManager.InputContext.CHARACTER
            && InputManager.InputManagerInstance.GetKeyDown(actionName))
            {
                //按下按键执行交互逻辑
                InteractLogics();
            }
        }
    }

    //进入触发区
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggerZone)
        {
            if (collision.tag == "Player")
            {
                isPlayerInRange = true;
                //显示提示文本
                ShowPrompt();
                CanvasManager.instance.canvasStack.Push(promptPanel);
                Debug.Log("Trigger!" + collision.name);
            }
        }
    }

    //离开触发区
    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (hasTriggerZone)
        {
            if (collision.tag == "Player")
            {
                isPlayerInRange = false;
                //隐藏文本
                HidePrompt();
                CanvasManager.instance.canvasStack.PopTo(promptPanel);
                Debug.Log("TriggerExit!" + collision.name);
            }
        }
    }

    //UI方法
    protected virtual void ShowPrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.gameObject.SetActive(true);
            if (promptText != null)
            {
                promptText.text = interactPrompt;
            }
        }
    }

    protected virtual void UpdatePrompt()
    {
        if (promptPanel != null && promptText != null)
        {
            promptText.text = interactPrompt;
        }
    }

    protected virtual void HidePrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.gameObject.SetActive(false);
        }
    }
}
