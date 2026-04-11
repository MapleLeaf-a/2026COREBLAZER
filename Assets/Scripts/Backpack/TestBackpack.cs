using Statics.Classes;
using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBackpack : MonoBehaviour
{
    public static TestBackpack instance;

    public BackpackModel backpackModel;
    public BackpackModel backpackMode2;

    public UAVBackpackView backpackView1;
    public FreezerView backpackView2;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        FoodMaterial material = new FoodMaterial("114", "KaZuHa", "Images/kazuha", "枫原万叶,流浪的武士", StaticTemplates.Food.MaterialType.Drink);
        FoodMaterial material2 = new FoodMaterial("514", "GGG", "Images/GoTo", "Go Go Go,出发咯", StaticTemplates.Food.MaterialType.Drink);
        BagItem bagItem = new BagItem(material, 1);
        BagItem bagItem2 = new BagItem(material2, 2);
        BagItem bagItem3 = new BagItem(material, 1);
        backpackModel = new BackpackModel(20);
        backpackModel.AddItem(bagItem);
        backpackModel.AddItem(bagItem2);
        backpackModel.AddItemAt(bagItem3, 3);
        BagItem bagItem4 = new BagItem(material, 2);
        BagItem bagItem5 = new BagItem(material2, 3);
        BagItem bagItem6 = new BagItem(material, 2);
        backpackMode2 = new BackpackModel(20);
        backpackMode2.AddItem(bagItem4);
        backpackMode2.AddItem(bagItem5);
        backpackMode2.AddItemAt(bagItem6, 3);

        backpackView1.InitBackpackView(backpackModel);
        backpackView2.InitBackpackView(backpackMode2);
    }

    void Update()
    {
        
    }
}
