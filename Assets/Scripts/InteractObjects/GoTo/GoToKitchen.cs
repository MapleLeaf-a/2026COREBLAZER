using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToKitchen : GoToObject
{
    void Start()
    {
        sceneName = "CookingScene";
        interactPrompt = "按F交互，\n前往厨房";
    }
}
