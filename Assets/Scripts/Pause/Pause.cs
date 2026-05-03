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

    public ResumeCountdown resumeCountdown;

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

        CanvasManager.instance.canvasStack.Push(pauseCanvas);

        MusicGame_AudioPlayer.instance.PauseMusic();
    }

    public void ContinueGame()
    {
        isPaused = false;

        CanvasManager.instance.canvasStack.PopTo(pauseCanvas);

        resumeCountdown.StartResumeCountdown();
    }
}
