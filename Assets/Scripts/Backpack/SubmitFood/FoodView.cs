using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodView : View<Food, FoodUIItem>
{

    public FoodViewModel foodViewModel
    { 
        get => viewModel as FoodViewModel;
        set => viewModel = value;
    }

    public virtual void InitBackpackView(FoodModel foodModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("������ʼ������");
        }
        capacity = foodModel.Capacity;
        this.foodViewModel = new FoodViewModel(foodModel, itemsPerPage);
    }

    public void UpdateFoods(FoodRecipe[] foodRecipes)
    {
        foodViewModel.UpdateFoods(foodRecipes);
    }


    public override void RefreshUI()
    {
        Food[] items = foodViewModel.CurrentPageItems;
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                bool isSelected = (foodViewModel.SelectedItem == items[i]);
                slots[i].SetUp(items[i], foodViewModel.GetSprite(items[i].foodRecipe.spritePath), i, isSelected);
            }
            else //��������items��������λ����ʾЧ��
            {
                slots[i].Clear();
            }
        }
    }

    protected override void BindOtherButtons()
    {
        
    }

    //��ק���(��Drag Handler����,�������ʹ����ͼ����ק�߼�����)
    private int draggingIndex = -1;
    public override void OnDragStart(int index)
    {
        draggingIndex = index;  //��¼��ʼ��ק�Ĳ�λ

        sourceView = this; //��¼Դͷ���Լ�

        Debug.Log($"��ק��ʼ: ����={index}, Դ����={name}");
    }

    public override void OnDragEnd()
    {
        draggingIndex = -1; //��ק������ռ�¼

        Debug.Log("��ק����");
    }

    public override void OnDrop(int targetIndex)
    {
        if (DragState<Food, FoodUIItem>.FromIndex == -1)
        {
            return; //���û��������ק����Ʒ,����
        }

        //�����ƶ����˱���

        DragState<Food, FoodUIItem>.Reset();  //�����ק״̬
    }
}
