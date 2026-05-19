using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseUAVGO : MonoBehaviour
{
    [Header("需要被关闭的游戏物体")]
    public GameObject GameObject;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(CloseGO);
    }

    private void CloseGO()
    {
        GameObject.SetActive(false);
    }
}
