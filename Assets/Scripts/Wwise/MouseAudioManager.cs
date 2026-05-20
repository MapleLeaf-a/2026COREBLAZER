using UnityEngine;
using System.Collections.Generic;

public class MouseAudioManager : MonoBehaviour
{
    public List<Transform> interferenceItems;
    public List<Transform> feedbackItems;

    // UI 游戏专属：设置一个距离缩放系数（因为 UI 坐标动辄几百像素，我们要把它缩小）
    [Header("UI距离缩小倍数(若声音变化不明显，调大这个值)")]
    public float distanceScale = 8f;

    void Update()
    {
        // 1. UI 鼠标跟随（直接使用鼠标的屏幕像素坐标）
        transform.position = Input.mousePosition;

        // ========================================================
        // 核心逻辑 A：干扰项距离
        // ========================================================
        if (interferenceItems != null) interferenceItems.RemoveAll(item => item == null);

        float minInterferenceDist = float.MaxValue;
        bool hasInterference = interferenceItems != null && interferenceItems.Count > 0;

        if (hasInterference)
        {
            foreach (Transform item in interferenceItems)
            {
                if (item == null) continue;
                // 计算 UI 间的像素距离
                float d = Vector3.Distance(transform.position, item.position);
                if (d < minInterferenceDist) minInterferenceDist = d;
            }
        }
        // 将像素距离缩小后传给 Wwise（比如 500 像素远，缩小 100 倍就是 5 米）
        float interferenceValueToSend = hasInterference ? (minInterferenceDist / distanceScale) : 100f;
        AkSoundEngine.SetRTPCValue("Distance_To_Interference", interferenceValueToSend);


        // ========================================================
        // 核心逻辑 B：每个物品的独立距离
        // ========================================================
        if (feedbackItems != null) feedbackItems.RemoveAll(item => item == null);

        if (feedbackItems != null && feedbackItems.Count > 0)
        {
            foreach (Transform item in feedbackItems)
            {
                if (item == null) continue;
                // 计算像素距离并缩小
                float distanceToThisItem = Vector3.Distance(transform.position, item.position) / distanceScale;

                // 单独发送给该物品对应的 Wwise 游戏对象
                AkSoundEngine.SetRTPCValue("Distance_Mouse_To_Item", distanceToThisItem, item.gameObject);
            }
        }
    }
}