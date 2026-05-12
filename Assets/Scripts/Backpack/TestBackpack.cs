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


    public OrderIngredientsModel orderIngredientsModel; 

    public OrderIngredientsView orderIngredientsView;

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

        FoodMaterial material = FoodMaterials.LookUpFoodMaterial("101");
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

        FoodRecipe foodRecipe1 = FoodRecipes.LookUpFoodRecipe("001");
        FoodRecipe foodRecipe2 = FoodRecipes.LookUpFoodRecipe("002");
        FoodRecipe foodRecipe3 = FoodRecipes.LookUpFoodRecipe("003");
        allRecipesModel = new AllRecipesModel(12);
        allRecipesModel.AddItem(foodRecipe1);
        allRecipesModel.AddItem(foodRecipe2);
        allRecipesModel.AddItem(foodRecipe3);
        allRecipesView.InitBackpackView(allRecipesModel);


        todaysRecipeModel = new TodaysRecipeModel(12);
        //todaysRecipeModel.AddItem(foodRecipe1);
        todaysRecipeView.InitBackpackView(todaysRecipeModel);

        orderIngredientsModel = new OrderIngredientsModel(10);
        orderIngredientsModel.AddItem(FoodMaterials.LookUpFoodMaterial("101"));
        orderIngredientsModel.AddItem(FoodMaterials.LookUpFoodMaterial("102"));
        orderIngredientsModel.AddItem(FoodMaterials.LookUpFoodMaterial("103"));
        orderIngredientsView.InitBackpackView(orderIngredientsModel);
    }

    void Update()
    {
        
    }
}
