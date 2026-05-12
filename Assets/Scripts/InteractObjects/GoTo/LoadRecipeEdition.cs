using UnityEngine;

public class LoadRecipeEdition : LoadCanvas
{
    protected override void Start()
    {
        base.Start();
        actionName = "InteractMouse0";
    }

    protected override void OpenCanvas()
    {
        base.OpenCanvas();
        InputManager.instance.SetContext(InputContext.RECIPE);
    }
}