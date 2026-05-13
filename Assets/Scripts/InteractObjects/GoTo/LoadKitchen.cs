using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadKitchen : LoadCanvas
{
    [Tooltip("“Ù”Œµƒ“Ù¿÷≤•∑≈∆˜")]
    public MusicGame_AudioPlayer audioPlayer;

    [Header("æ‹æ¯Canvas")]
    public Canvas denyCanvas;
    [Header("testBackpack")]
    public TestBackpack testBackpack;

    private TodaysRecipeModel todaysRecipeModel;

    protected override void Start()
    {
        base.Start();
        actionName = "InteractMouse0";
        denyCanvas.gameObject.SetActive(false);
        todaysRecipeModel = testBackpack.todaysRecipeModel;
    }

    protected override void Update()
    {
        if (InputManager.instance.currenContext == InputContext.CHARACTER
            && InputManager.instance.GetKeyDown(actionName) && CheckMouseClick())
        {
            if (todaysRecipeModel.Count < 4 || todaysRecipeModel.Count > 4 * 3)
            {
                ShowDenyCanvas();
                return;
            }
            OpenCanvas();
        }
    }

    protected override void OpenCanvas()
    {
        base.OpenCanvas();
        InputManager.instance.SetContext(InputContext.MUSICGAME);
        audioPlayer.gameObject.SetActive(true);
    }

    private void ShowDenyCanvas()
    { 
        denyCanvas.gameObject.SetActive(true);
    }
}
