using Statics.Classes;
using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderIngredientsView : View<FoodMaterial, OrderIngredientsUIItem>
{
    [Header("资金短缺画布")]
    public Canvas ShortOfMoneyCanvas;

    [Header("详情面板")]
    [Tooltip("名称")]
    public TextMeshProUGUI itemNameText;
    [Tooltip("描述")]
    public TextMeshProUGUI itemDescribeText;
    [Tooltip("图标")]
    public Image itemIconImage;
    [Tooltip("单价")]
    public TextMeshProUGUI priceText;
    [Tooltip("管理别的UI的显示")]
    public GameObject otherUIs;

    [Tooltip("计算总价")]
    public TotalPriceCaculator totalPriceCaculator;

    [Tooltip("购买的按钮")]
    public Button buyButton;

    int discountIndex = -1;

    private float discountRate = 0.7f;

    //要买的数量
    int buyCount;

    public OrderIngredientsViewModel orderIngredientsViewModel
    {
        get => viewModel as OrderIngredientsViewModel;

        set => viewModel = value; //value是C#属性setter中的上下文关键字,代表赋值操作传入的值
    }
    public virtual void InitBackpackView(OrderIngredientsModel orderIngredientsModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        capacity = orderIngredientsModel.Capacity;
        this.orderIngredientsViewModel = new OrderIngredientsViewModel(orderIngredientsModel, itemsPerPage);
    }

    protected override void OnViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(orderIngredientsViewModel.CurrentPageItems):      //VM中此页物品刷新时
                RefreshItems(); //V刷新物品列表
                break;
            case nameof(orderIngredientsViewModel.SelectedItem): //VM中选中的物品刷新时
                RefreshDetail();                         //V刷新详情面板
                RefreshItems();
                break;
            case nameof(orderIngredientsViewModel.DiscountIndex):
                RefreshDiscount();
                break;
            case nameof(orderIngredientsViewModel.CurrentPageNumber):
                break;
        }
    }

    //刷新物品列表
    void RefreshItems()
    {
        FoodMaterial[] items = orderIngredientsViewModel.CurrentPageItems;
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                bool isSelected = (orderIngredientsViewModel.SelectedItem == items[i]);
                slots[i].SetUp(items[i], orderIngredientsViewModel.GetSprite(items[i].spritePath), i, isSelected);
            }
            else //不包含在items里的清除槽位的显示效果
            {
                slots[i].Clear();
            }
        }
    }

    void RefreshDetail()
    {
        FoodMaterial item = orderIngredientsViewModel.SelectedItem;
        if (item != null)
        {
            itemIconImage.enabled = true;
            itemNameText.enabled = true;
            itemDescribeText.enabled = true;
            priceText.enabled = true;
            otherUIs.SetActive(true);

            itemIconImage.sprite = orderIngredientsViewModel.GetSprite(item.spritePath);
            itemNameText.text = item.name;
            itemDescribeText.text = item.description;
             
            if (orderIngredientsViewModel.SelectedIndex == discountIndex) priceText.text = (Mathf.Round(item.price * discountRate)).ToString();
            else priceText.text = item.price.ToString();

            buyCount = totalPriceCaculator.Caculate();
        }
        else
        {
            itemIconImage.enabled = false;
            itemNameText.enabled = false;
            itemDescribeText.enabled = false;
            priceText.enabled = false;
            otherUIs.SetActive(false);
        }
    }

    public void GenerateDiscountIndex()
    {
        orderIngredientsViewModel.GenerateDiscountIndex();
    }

    public void RefreshDiscount()
    {
        discountIndex = orderIngredientsViewModel.DiscountIndex;
        FoodMaterial[] items = orderIngredientsViewModel.CurrentPageItems;
        bool isSelected = (orderIngredientsViewModel.SelectedItem == items[discountIndex]);
        slots[discountIndex].SetUp(items[discountIndex], orderIngredientsViewModel.GetSprite(items[discountIndex].spritePath), discountIndex, isSelected, true);
        RefreshDiscountDetail();
    }

    public void RefreshDiscountDetail()
    { 
        
    }

    //刷新所有UI
    public override void RefreshUI()
    {
        RefreshItems();
        RefreshDetail();
        RefreshDiscount();
    }

    protected override void BindOtherButtons()
    {
        buyButton.onClick.AddListener(Buy);
    }

    public void Buy()
    {
        int totalMoney = buyCount * int.Parse(priceText.text);
        if (totalMoney > MoneyManager.Money)
        {
            ShortOfMoneyCanvas.gameObject.SetActive(true);
        }
        else
        {
            MoneyManager.IncreaseMoney(-totalMoney);
            TestBackpack.instance.FreezerBackpackView.backpackViewModel.AddItem(new BagItem(orderIngredientsViewModel.SelectedItem, buyCount));
        }
    }


    public override void OnDragStart(int index)
    {

    }

    public override void OnDragEnd()
    {
        
    }

    public override void OnDrop(int targetIndex)
    {
        
    }
}
