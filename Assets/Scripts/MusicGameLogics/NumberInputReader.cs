using UnityEngine;
using System.Text;
using System.Collections.Generic;

public class NumberInputReader : MonoBehaviour
{
    private StringBuilder numberBuffer = new StringBuilder();
    private bool isReading = false;

    private void Start()
    {
        StartNumberInput();
    }

    void Update()
    {
        if (!isReading) return;

        // 读取所有输入字符
        foreach (char c in Input.inputString)
        {
            // 检测回车（结束输入）
            if (c == '\n' || c == '\r')
            {
                ProcessNumbers();
                return;
            }

            // 处理退格（删除上一个字符）
            if (c == '\b')
            {
                if (numberBuffer.Length > 0)
                {
                    numberBuffer.Remove(numberBuffer.Length - 1, 1);
                    Debug.Log($"当前输入: {numberBuffer}");
                }
                continue;
            }

            // 只接受数字字符
            if (char.IsDigit(c))
            {
                numberBuffer.Append(c);
                Debug.Log($"当前输入: {numberBuffer}");
            }
            else
            {
                Debug.Log($"忽略非数字字符: {c}");
            }
        }
    }

    // 开始输入数字
    public void StartNumberInput()
    {
        isReading = true;
        numberBuffer.Clear();
        Debug.Log("请输入数字（回车结束）...");
    }

    // 处理输入的数字
    void ProcessNumbers()
    {
        isReading = false;

        if (numberBuffer.Length == 0)
        {
            Debug.Log("没有输入任何数字");
            return;
        }

        string numberString = numberBuffer.ToString();
        Debug.Log($"原始输入: {numberString}");

        UseNumber(numberString);

        // 清空缓冲区
        numberBuffer.Clear();
    }

    void UseNumber(string numberString)
    {
        List<int> indexs = new List<int>();

        for (int i = 0; i < numberString.Length; i++)
        { 
            indexs.Add(numberString[i] - '0');
        }

        BarJudger.BarJudgerInstance.CreateMealNotesList(indexs);
    }
}