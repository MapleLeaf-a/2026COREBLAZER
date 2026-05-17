using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadKitchen : LoadCanvas
{
    [Tooltip("“Ù”Œµƒ“Ù¿÷≤•∑≈∆˜")]
    public MusicGame_AudioPlayer audioPlayer;

    public Canvas denyCanvas;
    public int GuestCount = 4;

    protected override void Start()
    {
        base.Start();
        actionName = "InteractMouse0";
        denyCanvas.gameObject.SetActive(false);
    }

    protected override void Update()
    {
        if (InputManager.instance.currenContext == InputContext.CHARACTER
            && InputManager.instance.GetKeyDown(actionName) && CheckMouseClick())
        {
            int count = TestBackpack.instance.todaysRecipeModel.Count;
            if (count < GuestCount || count > GuestCount * 2)
            {
                denyCanvas.gameObject.SetActive(true);
                return;
            }
            else
            {
                OpenCanvas();
            }
        }
    }

    protected override void OpenCanvas()
    {
        base.OpenCanvas();
        InputManager.instance.SetContext(InputContext.MUSICGAME);
        audioPlayer.gameObject.SetActive(true);
    }
}
