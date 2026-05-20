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

    public UAVBackpackView UAVBackpackView;
    public FreezerView FreezerBackpackView;

    public AllRecipesModel allRecipesModel;

    public AllRecipesView allRecipesView;


    public TodaysRecipeModel todaysRecipeModel;

    public TodaysRecipeView todaysRecipeView;


    public OrderIngredientsModel orderIngredientsModel; 

    public OrderIngredientsView orderIngredientsView;



    public FoodModel foodModel;
    public FoodView foodView;

    public GuestFoodModel guestFoodModel;
    public GuestFoodView guestFoodView;

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

        DontDestroyOnLoad(gameObject);

        FoodMaterial material = FoodMaterials.LookUpFoodMaterial("step_005");
        BagItem bagItem = new BagItem(material, 1);
        BagItem bagItem3 = new BagItem(material, 1);
        backpackModel = new BackpackModel(20);
        backpackModel.AddItem(bagItem);
        backpackModel.AddItemAt(bagItem3, 3);
        BagItem bagItem4 = new BagItem(material, 4);
        BagItem bagItem6 = new BagItem(material, 4);
        backpackMode2 = new BackpackModel(40);
        backpackMode2.AddItem(bagItem4);
        backpackMode2.AddItemAt(bagItem6, 3);

        UAVBackpackView.InitBackpackView(backpackModel);
        FreezerBackpackView.InitBackpackView(backpackMode2);

        FoodRecipe foodRecipe1 = FoodRecipes.LookUpFoodRecipe("step_004");
        //FoodRecipe foodRecipe2 = FoodRecipes.LookUpFoodRecipe("step_009");
        //FoodRecipe foodRecipe3 = FoodRecipes.LookUpFoodRecipe("step_010");
        FoodRecipe foodRecipe4 = FoodRecipes.LookUpFoodRecipe("step_011");
        allRecipesModel = new AllRecipesModel(8);
        allRecipesModel.AddItem(foodRecipe1);
        //allRecipesModel.AddItem(foodRecipe2);
        //allRecipesModel.AddItem(foodRecipe3);
        allRecipesModel.AddItem(foodRecipe4);
        allRecipesView.InitBackpackView(allRecipesModel);


        todaysRecipeModel = new TodaysRecipeModel(8);
        //todaysRecipeModel.AddItem(foodRecipe1);
        todaysRecipeView.InitBackpackView(todaysRecipeModel);

        orderIngredientsModel = new OrderIngredientsModel(14);
        orderIngredientsModel.AddItem(FoodMaterials.LookUpFoodMaterial("step_001"));
        orderIngredientsModel.AddItem(FoodMaterials.LookUpFoodMaterial("step_002"));
        orderIngredientsModel.AddItem(FoodMaterials.LookUpFoodMaterial("step_003"));
        orderIngredientsView.InitBackpackView(orderIngredientsModel);
        orderIngredientsView.GenerateDiscountIndex();

        foodModel = new FoodModel(12);
        foodView.InitBackpackView(foodModel);

        guestFoodModel = new GuestFoodModel(12);
        guestFoodView.InitBackpackView(guestFoodModel);
    }

    void Update()
    {
        
    }


}
