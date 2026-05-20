using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestFoodView : View<Food, FoodUIItem>
{

    public Canvas SubmitFoodCanvas;

    public GuestFoodViewModel guestFoodViewModel
    { 
        get => viewModel as GuestFoodViewModel;
        set => viewModel = value;
    }

    public virtual void InitBackpackView(GuestFoodModel guestFoodModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("������ʼ������");
        }
        capacity = guestFoodModel.Capacity;
        this.guestFoodViewModel = new GuestFoodViewModel(guestFoodModel, itemsPerPage);
    }

    public override void RefreshUI()
    {
        Food[] items = guestFoodViewModel.CurrentPageItems;
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Length && items[i] != null)
            {
                bool isSelected = (guestFoodViewModel.SelectedItem == items[i]);
                slots[i].SetUp(items[i], guestFoodViewModel.GetSprite(items[i].foodRecipe.spritePath), i, isSelected);
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
        
    }

    public override void OnDragEnd()
    {
        
    } 

    public override void OnDrop(int targetIndex)
    {
        if (DragState<Food, FoodUIItem>.FromIndex == -1)
        {
            return; //���û��������ק����Ʒ,����
        }

        if (DragState<Food, FoodUIItem>.SourceView == this) //����ͬһ������
        {
            //ͬһ�����ڲ����ƶ�
        }
        else if (DragState<Food, FoodUIItem>.SourceView as FoodView != null) 
        {
            //�����ƶ�,ע���Ǵ�Դ������Ŀǰ����
            var v = DragState<Food, FoodUIItem>.SourceView as FoodView;
            int fromIndex = DragState<Food, FoodUIItem>.FromIndex;
            guestFoodViewModel.AddItemAt(v.foodViewModel.GetItemAt(fromIndex), targetIndex);
            Food food = v.foodViewModel.GetItemAt(fromIndex);
            MoneyManager.IncreaseMoney(food.foodRecipe.basePrice);
            bool success = v.foodViewModel.RemoveItemAt(fromIndex);

            if (success)
            {
                //ˢ������������ҳ��
                DragState<Food, FoodUIItem>.SourceView.RefreshUI();
                RefreshUI();

                if (v.foodViewModel.Count == 0)
                {
                    SubmitFoodCanvas.gameObject.SetActive(false);
                }
            }
        }

        DragState<Food, FoodUIItem>.Reset();  //�����ק״̬
    }
}
