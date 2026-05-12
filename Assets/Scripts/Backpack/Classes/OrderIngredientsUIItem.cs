using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderIngredientsUIItem : GenericSlot<FoodMaterial>
{
    //±³¾°Í¼Ïñ
    public Image bgImage;

    public void SetUp(FoodMaterial item, Sprite bgImage, Sprite image, int index, bool isSelected)
    {
        base.SetUp(item, image, index, isSelected);
        this.bgImage.sprite = bgImage;
    }
}
