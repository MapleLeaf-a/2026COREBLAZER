using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenUAVGO : MonoBehaviour
{
    [Header("需要被激活的游戏物体")]
    public GameObject GameObject;
    public Canvas UAVCanvas;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OpenGO);
    }

    private void OpenGO()
    {
        GameObject.SetActive(true);
        UAVCanvas.gameObject.SetActive(true);
    }
}
