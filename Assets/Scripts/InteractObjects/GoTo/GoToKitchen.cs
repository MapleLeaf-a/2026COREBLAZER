using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToKitchen : GoToObject
{
    protected override void Start()
    {
        base.Start();
        sceneName = "CookingScene";
        actionName = "InteractF";
        interactPrompt = "∞¥FΩªª•£¨\n«∞Õ˘Œ‘ “";
    }

    protected override void Update()
    { 
        base.Update();
    }
}
