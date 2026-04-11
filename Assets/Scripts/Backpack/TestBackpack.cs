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

    public BackpackView<UAVBackpackViewModel> backpackView1;
    public BackpackView<BackpackViewModel> backpackView2;

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
        BagItem bagItem = new BagItem(material, 1);
        FoodMaterial material2 = new FoodMaterial("514", "GGG", "Images/GoTo", "Go Go Go,出发咯", StaticTemplates.Food.MaterialType.Drink);
        BagItem bagItem2 = new BagItem(material2, 4);
        FoodMaterial material3 = new FoodMaterial("114", "KaZuHa", "Images/kazuha", "枫原万叶,流浪的武士", StaticTemplates.Food.MaterialType.Drink);
        BagItem bagItem3 = new BagItem(material3, 1);
        backpackModel = new BackpackModel(20);
        backpackModel.AddItem(bagItem);
        backpackModel.AddItem(bagItem2);
        backpackModel.AddItemAt(bagItem3, 3);
        backpackMode2 = new BackpackModel(20);
        backpackMode2.AddItem(bagItem);
        backpackMode2.AddItem(bagItem2);
        backpackMode2.AddItemAt(bagItem3, 3);

        backpackView1.InitBackpackView(backpackModel);
        backpackView2.InitBackpackView(backpackMode2);
    }

    void Update()
    {
        
    }
}
