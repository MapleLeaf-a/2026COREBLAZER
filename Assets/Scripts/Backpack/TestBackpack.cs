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

    public AllRecipesModel allRecipesModel;

    public AllRecipesView allRecipesView;

    public TodaysRecipeModel todaysRecipeModel;

    public TodaysRecipeView todaysRecipeView;

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

        FoodMaterial material = new FoodMaterial("114", "KaZuHa", "Images/Recipes/kazuha", "∑„‘≠ÕÚ“∂,¡˜¿ÀµƒŒ‰ ø", StaticTemplates.Food.MaterialType.Drink);
        BagItem bagItem = new BagItem(material, 1);
        BagItem bagItem3 = new BagItem(material, 1);
        backpackModel = new BackpackModel(20);
        backpackModel.AddItem(bagItem);
        backpackModel.AddItemAt(bagItem3, 3);
        BagItem bagItem4 = new BagItem(material, 2);
        BagItem bagItem6 = new BagItem(material, 2);
        backpackMode2 = new BackpackModel(20);
        backpackMode2.AddItem(bagItem4);
        backpackMode2.AddItemAt(bagItem6, 3);

        backpackView1.InitBackpackView(backpackModel);
        backpackView2.InitBackpackView(backpackMode2);

        Dictionary<string, int> ingredients1 = new Dictionary<string, int>(){ { "Images/Recipes/kazuha", 3 } }; 
        FoodRecipe foodRecipe1 = new FoodRecipe(60, "001", "«·÷Àƒﬁª‘Œ’", "Images/Recipes/Sushi", ingredients1);
        Dictionary<string, int> ingredients2 = new Dictionary<string, int>() { { "Images/Recipes/kazuha", 1 }, { "Images/Recipes/fish", 1 }, { "Images/Recipes/seaweed", 1 } };
        FoodRecipe foodRecipe2 = new FoodRecipe(20, "002", "Ã¿", "Images/Recipes/Soup", ingredients2);
        Dictionary<string, int> ingredients3 = new Dictionary<string, int>();
        FoodRecipe foodRecipe3 = new FoodRecipe(0, "003", "ÀÆ", "Images/Recipes/Water", ingredients3);
        allRecipesModel = new AllRecipesModel(12);
        allRecipesModel.AddItem(foodRecipe1);
        allRecipesModel.AddItem(foodRecipe2);
        allRecipesModel.AddItem(foodRecipe3);
        allRecipesView.InitBackpackView(allRecipesModel);


        todaysRecipeModel = new TodaysRecipeModel(12);
        //todaysRecipeModel.AddItem(foodRecipe1);
        todaysRecipeView.InitBackpackView(todaysRecipeModel);
    }

    void Update()
    {
        
    }
}
