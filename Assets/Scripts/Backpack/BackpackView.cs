using Statics.Classes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BackpackView : MonoBehaviour
{
    BackpackViewModel backpackViewModel;
    BackpackModel backpackModel;

    [Header("背包初始属性")]
    [Tooltip("背包容量")]
    public int capacity;
    [Tooltip("每页的物品量")]
    public int itemsPerPage;

    [Header("分页")]
    public Button prevButton;
    public Button nextButton;

    [Header("详情面板")]
    [Tooltip("名称")]
    public TextMeshProUGUI itemNameText;
    [Tooltip("描述")]
    public TextMeshProUGUI itemDescribeText;
    [Tooltip("图标")]
    public Image itemIconImage;


    [Header("背包ItemUI相关")]
    [Tooltip("父物体")]
    public Transform contentsParent;
    [Tooltip("物品槽预制体")]
    public GameObject slotPrefab;

    private List<PackageUIItem> slots = new List<PackageUIItem>();


    void Start()
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        backpackModel = TestBackpack.instance.backpackModel;
        capacity = backpackModel.Capacity;
        backpackViewModel = new BackpackViewModel(backpackModel,itemsPerPage);

        //创建物品槽
        CreateSlots();

        //订阅viewModel变化
        backpackViewModel.PropertyChanged += OnViewModelChanged;

        //绑定切换页面事件
        PageChangingEvent();

        //初始化显示
        RefreshUI();
    }

    void Update()
    {
        
    } 



    void CreateSlots()
    {
        for (int i = 0; i < itemsPerPage; i++)
        {
            int index = i; //防止用lambda引用的闭包陷阱
            GameObject slot = Instantiate(slotPrefab, contentsParent);
            Button button = slot.GetComponent<Button>();
            button.onClick.AddListener(() => OnSlotClick(index)); //用lambda解决了"按钮点击事件无法直接传递参数"的问题
            slots.Add(slot.GetComponent<PackageUIItem>());
        }
    }

    void OnSlotClick(int index)
    { 
        backpackViewModel.SelectItem(index);
    }

    //更换页面事件
    void PageChangingEvent()
    {
        prevButton.onClick.AddListener(() => backpackViewModel.PrevPage());
        nextButton.onClick.AddListener(() => backpackViewModel.NextPage());
    }


    //响应ViewModel变化,当ViewModel中的属性发生变化时,这个方法会被自动调用
    void OnViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(backpackViewModel.CurrentPageItems):      //VM中此页物品刷新时
                RefreshItems(); //V刷新物品列表
                break;
            case nameof(backpackViewModel.SelectedItem): //VM中选中的物品刷新时
                RefreshDetail();                         //V刷新详情面板
                RefreshItems();
                break;
            case nameof(backpackViewModel.HasPrevPage):
                prevButton.interactable = backpackViewModel.HasPrevPage;
                break;
            case nameof(backpackViewModel.HasNextPage):
                nextButton.interactable = backpackViewModel.HasNextPage;
                break;

            case nameof(backpackViewModel.CurrentPageNumber):
                break;
        }
    }

    //刷新物品列表
    void RefreshItems()
    {
        BagItem[] items = backpackViewModel.CurrentPageItems;
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                bool isSelected = (backpackViewModel.SelectedItem == items[i]);
                slots[i].SetUp(items[i], backpackViewModel.GetSprite(items[i].SpritePath), i, isSelected);
            }
            else //不包含在items里的清除槽位的显示效果
            {
                slots[i].Clear();
            }
        }
    }

    void RefreshDetail()
    {
        BagItem item = backpackViewModel.SelectedItem;
        if (item != null)
        {
            itemIconImage.enabled = true;
            itemNameText.enabled = true;
            itemDescribeText.enabled = true;

            itemIconImage.sprite = backpackViewModel.GetSprite(item.SpritePath);
            itemNameText.text = item.Name;
            itemDescribeText.text = item.Description;
        }
        else 
        {
            itemIconImage.enabled = false;
            itemNameText.enabled = false;
            itemDescribeText.enabled = false;
        }
    }

    //刷新所有UI
    void RefreshUI()
    {
        RefreshItems();
        RefreshDetail();
        prevButton.interactable = backpackViewModel.HasPrevPage;
        nextButton.interactable = backpackViewModel.HasNextPage;
    }

    void OnDestroy()
    { 
        //取消订阅防止内存泄漏
        backpackViewModel.PropertyChanged -= OnViewModelChanged;
    }


    //拖拽相关(由Drag Handler调用,这种设计使得视图和拖拽逻辑分离)
    private int draggingIndex = -1;

    public void OnDragStart(int index)
    {
        draggingIndex = index;  //记录开始拖拽的槽位
    }

    public void OnDragEnd()
    {
        draggingIndex = -1; //拖拽结束清空记录
    }

    public void OnDrop(int targetIndex)
    {
        //如果有正在拖拽的物品，且不是拖到同一个槽位
        if (draggingIndex != -1 && draggingIndex != targetIndex)
        {
            //尝试移动物品
            bool success = backpackViewModel.TryMoveItem(draggingIndex, targetIndex);

            if (success)
            {
                backpackViewModel.SelectItem(targetIndex);
                RefreshUI(); //刷新界面
            }
        }
        OnDragEnd(); //清空拖拽状态
    }
}
