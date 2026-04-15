using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracksManager : MonoBehaviour
{
    //轨道
    public List<NoteManager> tracks;

    //音游画布
    public Canvas canvas;

    //预设的各音符轨道的音符情况
    //四轨道
    public List<List<bool>> notesPres = new List<List<bool>>
    {new List<bool>    { true, false, false, true, false, true},
     new List<bool>    { false, true, false, true, false, true},
     new List<bool>    { false, false, true, false, false, true},
     new List<bool>    { true, true, false, true, false, true}
    };
    //单轨道
    private List<bool> notesPre = new List<bool> { true, false, false, true, false, true, true, true, true };

    //单例
    public static TracksManager instance;

    void OnEnable()
    {
        CanvasManager.canvasManagerInstance.canvasStack.Push(canvas);

        InputManager.InputManagerInstance.SetContext(InputManager.InputContext.MUSICGAME);
    }

    void OnDisable()
    {
        CanvasManager.canvasManagerInstance.canvasStack.PopTo(canvas);
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (tracks.Count == 1)
        {
            tracks[0].Initialize(0, tracks.Count, notesPre);
        }
        else
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                tracks[i].Initialize(i, tracks.Count, notesPres[i]);
            }
        }
    }

    void Update()
    {
        
    }
}
