using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupImage : MonoBehaviour
{
    [Header("动画设置")]
    [Tooltip("弹出动画时长")]
    public float popDuration = 0.3f;

    [Tooltip("显示停留时长")]
    public float displayDuration = 1f;

    [Tooltip("消失动画时长")]
    public float fadeDuration = 0.2f;

    [Header("缩放设置")]
    [Tooltip("起始缩放")]
    public Vector3 startScale;

    [Tooltip("目标缩放")]
    public Vector3 targetScale = Vector3.one;

    private Image image;
    private CanvasGroup canvasGroup;
    private Coroutine currentAnimation;

    void Awake()
    {
        // 获取组件
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 确保有 CanvasGroup 组件（用于淡入淡出）
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        //初始启用，但透明不可见
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 显示弹出图片（外部调用接口）
    /// </summary>
    /// <param name="sprite">要显示的图片</param>
    public void Show(Sprite sprite)
    {
        // 停止当前动画
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // 设置图片
        if (image != null && sprite != null)
        {
            image.sprite = sprite;
        }

        // 确保物体激活
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // 开始弹出动画
        currentAnimation = StartCoroutine(PopupAnimation());
    }

    /// <summary>
    /// 显示弹出图片（带回调）
    /// </summary>
    public void Show(Sprite sprite, System.Action onComplete)
    {
        StartCoroutine(ShowWithCallback(sprite, onComplete));
    }

    private IEnumerator ShowWithCallback(Sprite sprite, System.Action onComplete)
    {
        yield return StartCoroutine(PopupAnimationCoroutine(sprite));
        onComplete?.Invoke();
    }

    private IEnumerator PopupAnimation()
    {
        yield return StartCoroutine(PopupAnimationCoroutine(image.sprite));
    }

    private IEnumerator PopupAnimationCoroutine(Sprite sprite)
    {
        // 设置图片
        if (image != null && sprite != null)
        {
            image.sprite = sprite;
            image.SetNativeSize();
        }

        // 重置状态
        startScale = transform.localScale;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(true);

        // 1. 弹出动画（缩放 + 淡入）
        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / popDuration;

            // 缩放
            float eased = EaseOutBack(t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, eased);

            // 淡入
            canvasGroup.alpha = t;

            yield return null;
        }
        transform.localScale = targetScale;
        canvasGroup.alpha = 1f;

        // 2. 停留显示
        yield return new WaitForSecondsRealtime(displayDuration);

        // 3. 淡出消失
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        // 4. 完全隐藏（但物体保持激活，只是透明）
        canvasGroup.alpha = 0f;
        currentAnimation = null;
    }

    /// <summary>
    /// 弹性缓动曲线
    /// </summary>
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    /// <summary>
    /// 立即隐藏
    /// </summary>
    public void HideImmediately()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        canvasGroup.alpha = 0f;
        currentAnimation = null;
    }
}