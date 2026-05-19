using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderIngredientsUIItem : GenericSlot<FoodMaterial>
{
    ////ŒÔ∆∑ÕºœÒ
    //public Image itemImage;
    ////—°÷–œ‘ æ
    //public Image selectedImage;

    //±≥æ∞ÕºœÒ
    public Image bgImage;

    //¥Ú’€ÕºœÒ
    public Image discountImage;

    public void SetUp(FoodMaterial item, Sprite image, int index, bool isSelected, bool isDiscounted)
    {
        base.SetUp(item, image, index, isSelected);
        if (isDiscounted)
        {
            discountImage.enabled = true;
        }
        else
        {
            discountImage.enabled= false;
        }
    }

    public override void SetSelected(bool isSelected)
    {
        if (selectedImage != null)
        {
            if (isSelected)
            {
                selectedImage.enabled = true;
                bgImage.enabled = false;
            }
            else
            {
                selectedImage.enabled = false;
                bgImage.enabled = true;
            }
        }
    }
}
