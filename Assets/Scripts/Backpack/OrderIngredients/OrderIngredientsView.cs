using Statics.Classes;
using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderIngredientsView : View<FoodMaterial, OrderIngredientsUIItem>
{
    [Header("�ʽ��ȱ����")]
    public Canvas ShortOfMoneyCanvas;

    [Header("�������")]
    [Tooltip("����")]
    public TextMeshProUGUI itemNameText;
    [Tooltip("����")]
    public TextMeshProUGUI itemDescribeText;
    [Tooltip("ͼ��")]
    public Image itemIconImage;
    [Tooltip("����")]
    public TextMeshProUGUI priceText;
    [Tooltip("������UI����ʾ")]
    public GameObject otherUIs;

    [Tooltip("�����ܼ�")]
    public TotalPriceCaculator totalPriceCaculator;

    [Tooltip("����İ�ť")]
    public Button buyButton;

    int discountIndex = -1;

    private float discountRate = 0.8f;

    //Ҫ�������
    int buyCount;

    public OrderIngredientsViewModel orderIngredientsViewModel
    {
        get => viewModel as OrderIngredientsViewModel;

        set => viewModel = value; //value��C#����setter�е������Ĺؼ���,����ֵ���������ֵ
    }
    public virtual void InitBackpackView(OrderIngredientsModel orderIngredientsModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("������ʼ������");
        }
        capacity = orderIngredientsModel.Capacity;
        this.orderIngredientsViewModel = new OrderIngredientsViewModel(orderIngredientsModel, itemsPerPage);
    }

    protected override void OnViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(orderIngredientsViewModel.CurrentPageItems):      //VM�д�ҳ��Ʒˢ��ʱ
                RefreshItems(); //Vˢ����Ʒ�б�
                break;
            case nameof(orderIngredientsViewModel.SelectedItem): //VM��ѡ�е���Ʒˢ��ʱ
                RefreshDetail();                         //Vˢ���������
                RefreshItems();
                break;
            case nameof(orderIngredientsViewModel.DiscountIndex):
                RefreshDiscount();
                break;
            case nameof(orderIngredientsViewModel.CurrentPageNumber):
                break;
        }
    }

    //ˢ����Ʒ�б�
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
            else //��������items��������λ����ʾЧ��
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
    }

    //ˢ������UI
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
