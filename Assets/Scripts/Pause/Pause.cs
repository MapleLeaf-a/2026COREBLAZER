using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    //是否暂停
    bool isPaused = false;

    //暂停的按钮
    public Button pauseButton;

    //关闭暂停页面的按钮
    public Button resumeButton;

    //暂停的画布
    public Canvas pauseCanvas;


    void Start()
    {
        pauseButton.onClick.AddListener(TogglePause);
        resumeButton.onClick.AddListener(ContinueGame);
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ContinueGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0.0f;
        isPaused = true;

        CanvasManager.canvasManagerInstance.canvasStack.Push(pauseCanvas);
    }

    public void ContinueGame()
    {
        Time.timeScale = 1.0f;
        isPaused = false;

        CanvasManager.canvasManagerInstance.canvasStack.PopTo(pauseCanvas);
    }
}
