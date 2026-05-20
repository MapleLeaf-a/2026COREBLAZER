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

    private bool isProcessing = false;

    void Start()
    {
        button.onClick.AddListener(Display);    
        guide.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isProcessing)
        {
            guide.gameObject.SetActive(false);
        }
    }

    private void Display()
    {
        if (isProcessing) return;

        isProcessing = true;

        if (guide.activeSelf == false)
        {
            guide.SetActive(true);
        }
        else
        {
            guide.SetActive(false);
        }

        isProcessing = false;
    }
}
