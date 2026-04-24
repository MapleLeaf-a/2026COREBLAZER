using Statics.Classes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TItem:物品类泛型,TSlot:槽位类泛型
public abstract class View<TItem, TSlot> : MonoBehaviour 
    where TItem : class
    where TSlot : GenericSlot<TItem>
{
    protected ViewModel<TItem> viewModel;

    [Header("背包初始属性")]
    [Tooltip("背包容量")]
    public int capacity;
    [Tooltip("每页的物品量")]
    public int itemsPerPage;

    [Header("背包ItemUI相关")]
    [Tooltip("父物体")]
    public Transform contentsParent;
    [Tooltip("物品槽预制体")]
    public GameObject slotPrefab;

    protected List<TSlot> slots = new List<TSlot>();

    protected View<TItem, TSlot> sourceView;  //记录拖拽源头的背包视图

    protected virtual void Start()
    {
        //InitBackpackView();

        //创建物品槽
        CreateSlots();

        //订阅viewModel变化
        viewModel.PropertyChanged += OnViewModelChanged;

        //子类按钮绑定
        BindOtherButtons();

        //初始化显示
        RefreshUI();
    }

    /// <summary>
    /// 初始化背包View层，子类最好重写这个方法
    /// </summary>
    /// <param name="model"></param>
    /// <exception cref="UnityException"></exception>
    public virtual void InitBackpackView(Model<TItem> model)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        capacity = model.Capacity;
        this.viewModel = new ViewModel<TItem>(model, itemsPerPage);
    }

    protected virtual void CreateSlots()
    {
        for (int i = 0; i < itemsPerPage; i++)
        {
            int index = i; //防止用lambda引用的闭包陷阱
            GameObject slot = Instantiate(slotPrefab, contentsParent);
            Button button = slot.GetComponent<Button>();
            button.onClick.AddListener(() => OnSlotClick(index)); //用lambda解决了"按钮点击事件无法直接传递参数"的问题
            slots.Add(slot.GetComponent<TSlot>());
        }
    }

    void OnSlotClick(int index)
    {
        viewModel.SelectItem(index);
    }

    /// <summary>
    /// 子类实现该抽象方法，绑定子类其他别的用途的按钮
    /// </summary>
    protected abstract void BindOtherButtons();

    //响应ViewModel变化,当ViewModel中的属性发生变化时,这个方法会被自动调用
    protected virtual void OnViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RefreshUI();
    }

    //刷新所有UI
    public abstract void RefreshUI();

    void OnDestroy()
    {
        //取消订阅防止内存泄漏
        viewModel.PropertyChanged -= OnViewModelChanged;
    }

    public abstract void OnDragStart(int index) ;

    public abstract void OnDragEnd();

    public abstract void OnDrop(int targetIndex) ;
}
