using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowGuide : MonoBehaviour
{
    [Tooltip("展示教程的按钮")]
    public Button button;
    [Tooltip("教程游戏物体")]
    public GameObject guide;
    
    void Start()
    {
        button.onClick.AddListener(Display);    
        guide.SetActive(false);
    }

    void Update()
    {
        
    }

    private void Display()
    {
        if (guide.activeSelf == false)
        {
            guide.SetActive(true);
        }
        else
        {
            guide.SetActive(false);
        }
    }
}
