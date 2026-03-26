using System.Collections;
using TMPro;
using UnityEngine;

public class PopUpText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    private Coroutine currentCoroutine;
    private Color originalColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        originalColor = textMesh.color;
    }

    public void ShowText(string message, Color color, float duration)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        textMesh.text = message;
        textMesh.color = color;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        currentCoroutine = StartCoroutine(ShowRoutine(duration));
    }

    IEnumerator ShowRoutine(float duration)
    {
        // 弹出动画
        transform.localScale = Vector3.zero;
        float timer = 0f;
        while (timer < 0.2f)
        {
            timer += Time.deltaTime;
            float progress = timer / 0.2f;
            float scale = Mathf.Sin(progress * Mathf.PI / 2);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        transform.localScale = Vector3.one;

        // 停留
        yield return new WaitForSeconds(duration);

        // 淡出 - 直接改颜色alpha
        timer = 0f;
        float fadeDuration = 0.3f;
        Color startColor = textMesh.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            textMesh.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }

        // 隐藏
        gameObject.SetActive(false);

        // 重置颜色（保留原始alpha）
        Color resetColor = originalColor;
        textMesh.color = resetColor;

        currentCoroutine = null;
    }
}