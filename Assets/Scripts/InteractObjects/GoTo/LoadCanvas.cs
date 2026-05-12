using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCanvas : MonoBehaviour
{
    [Header("需要加载的画布")]
    public Canvas canvas;

    protected string actionName = "";

    private Camera mainCamera;

    protected virtual void Start()
    {
        mainCamera = Camera.main;

        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }

    protected virtual void Update()
    {
        if (InputManager.instance.currenContext == InputContext.CHARACTER
            && InputManager.instance.GetKeyDown(actionName))
        {
            CheckMouseClick();
        }
    }

    protected virtual void CheckMouseClick()
    {
        if (mainCamera == null) return;

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            OpenCanvas();
        }
    }

    protected virtual void OpenCanvas()
    {
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
        }
    }
}
