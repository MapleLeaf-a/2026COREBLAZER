using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ResumeCountdown : MonoBehaviour
{
    [Header("倒计时 UI")]
    public GameObject countdownCanvas;      // 倒计时画布
    public TextMeshProUGUI countdownText;   // 倒计时文字
    public float countdownTime = 3f;        // 倒计时时长

    private bool isCountingDown = false;

    void Start()
    {
        // 初始隐藏倒计时界面
        if (countdownCanvas != null)
            countdownCanvas.SetActive(false);
    }

    /// <summary>
    /// 开始倒计时恢复
    /// </summary>
    public void StartResumeCountdown()
    {
        if (isCountingDown) return;
        // 显示倒计时界面
        if (countdownCanvas != null)
            countdownCanvas.SetActive(true);
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        isCountingDown = true;

        float remainingTime = countdownTime;

        while (remainingTime > 0)
        {
            // 更新显示（显示整数，如 3, 2, 1）
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(remainingTime).ToString();

            remainingTime -= Time.unscaledDeltaTime;  // 使用 unscaledDeltaTime，不受 Time.timeScale 影响
            yield return null;
        }

        // 倒计时结束，显示 "GO!" 或 "开始"
        if (countdownText != null)
            countdownText.text = "GO!";

        yield return new WaitForSecondsRealtime(0.2f);  // 短暂显示 GO!

        // 隐藏倒计时界面
        if (countdownCanvas != null)
            countdownCanvas.SetActive(false);

        // 真正恢复游戏
        Time.timeScale = 1f;

        isCountingDown = false;
    }

    /// <summary>
    /// 取消倒计时（如果游戏又暂停了）
    /// </summary>
    public void CancelCountdown()
    {
        if (isCountingDown)
        {
            StopAllCoroutines();
            isCountingDown = false;

            if (countdownCanvas != null)
                countdownCanvas.SetActive(false);
        }
    }
}