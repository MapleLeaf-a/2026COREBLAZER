using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadKitchen : LoadCanvas
{
    [Tooltip("“Ù”Œµƒ“Ù¿÷≤•∑≈∆˜")]
    public MusicGame_AudioPlayer audioPlayer;

    protected override void Start()
    {
        base.Start();
        actionName = "InteractMouse0";
    }

    protected override void Update()
    {
        if (false)
        {
            return;
        }
        base.Update();
    }

    protected override void OpenCanvas()
    {
        base.OpenCanvas();
        InputManager.instance.SetContext(InputContext.MUSICGAME);
        audioPlayer.gameObject.SetActive(true);
    }
}
