using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadFreezerCanvas : LoadCanvas
{
    protected override void Start()
    {
        base.Start();
        actionName = "InteractMouse0";
    }

    protected override void OpenCanvas()
    {
        base.OpenCanvas();
        InputManager.instance.SetContext(InputContext.BACKPACK);
    }
}
