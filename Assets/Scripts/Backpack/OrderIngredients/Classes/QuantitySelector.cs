using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuantitySelector : MonoBehaviour
{
    [Header("UI组件")]
    public Button valueButton;              //显示数值的按钮
    public TextMeshProUGUI quantityText;    //显示数量的文本
    public GameObject sliderPanel;          //滑动条面板
    public Slider quantitySlider;           //滑动条
    public Button confirmButton;            //确认按钮

    [Header("设置")]
    public int minValue = 1;
    public int maxValue = 10;

    [Header("计算价格文本")]
    public TotalPriceCaculator totalPriceCaculator;

    private int currentValue = 1;

    public int CurrentValue => currentValue;

    void Start()
    {
        //初始隐藏滑动条面板
        if (sliderPanel != null)
            sliderPanel.SetActive(false);

        //配置滑动条
        if (quantitySlider != null)
        {
            quantitySlider.minValue = minValue;
            quantitySlider.maxValue = maxValue;
            quantitySlider.wholeNumbers = true;
            quantitySlider.value = currentValue;
            quantitySlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        //点击数值按钮显示滑动条
        if (valueButton != null)
            valueButton.onClick.AddListener(ShowSlider);

        //点击确认按钮关闭滑动条
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmAndClose);

        UpdateValueText();
    }

    void ShowSlider()
    {
        if (sliderPanel != null)
            sliderPanel.SetActive(true);
        quantitySlider.value = currentValue;
    }

    void ConfirmAndClose()
    {
        if (sliderPanel != null)
            sliderPanel.SetActive(false);
        OnQuantityConfirmed(currentValue);
    }

    void OnSliderValueChanged(float value)
    {
        currentValue = Mathf.RoundToInt(value);
        UpdateValueText();
    }

    void UpdateValueText()
    {
        if (quantityText != null)
            quantityText.text = currentValue.ToString();
    }

    void OnQuantityConfirmed(int finalQuantity)
    {
        //Debug.Log($"最终数量确定为: {finalQuantity}");
        totalPriceCaculator.Caculate();
    }
}