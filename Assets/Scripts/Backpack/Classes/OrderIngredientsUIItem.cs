using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderIngredientsUIItem : GenericSlot<FoodMaterial>
{
    //±≥æ∞ÕºœÒ
    public Image bgImage;

    [Header("¥Ú’€Õº∆¨")]
    public Sprite discountImage;
    [Header("ƒ¨»œÕº∆¨")]
    public Sprite defaultImage;

    public void SetUp(FoodMaterial item, Sprite image, int index, bool isSelected, bool isDiscounted)
    {
        base.SetUp(item, image, index, isSelected);
        if (isDiscounted)
        {
            bgImage.sprite = discountImage;
        }
        else
        {
            bgImage.sprite = defaultImage;
        }
    }
}
