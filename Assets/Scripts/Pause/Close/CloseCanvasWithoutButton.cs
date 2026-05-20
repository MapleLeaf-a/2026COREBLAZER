using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseCanvasWithoutButton : MonoBehaviour
{
    //需要关闭的画布
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            canvas.gameObject.SetActive(false);
        }
    }
}
