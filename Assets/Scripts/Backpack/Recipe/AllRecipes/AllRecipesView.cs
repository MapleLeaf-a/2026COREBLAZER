using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllRecipesView : RecipesView
{
    public AllRecipesViewModel allRecipesViewModel
    {
        get => viewModel as AllRecipesViewModel;

        set => viewModel = value; //value是C#属性setter中的上下文关键字,代表赋值操作传入的值
    }

    public virtual void InitBackpackView(AllRecipesModel recipesModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("背包初始化出错！");
        }
        capacity = recipesModel.Capacity;
        this.recipesViewModel = new AllRecipesViewModel(recipesModel, itemsPerPage);
    }

    [Header("详情面板")]
    [Tooltip("名称")]
    public TextMeshProUGUI itemNameText;
    [Tooltip("价格")]
    public TextMeshProUGUI itemPriceText;
    [Tooltip("图标")]
    public Image itemIconImage;
    [Tooltip("价格图标")]
    public Image priceIconImage;
    [Tooltip("食材展示父物体")]
    public Transform ingredientsParent;
    [Tooltip("单个食材物件预制体")]
    public GameObject ingredientUIItemPrefab;
    [Tooltip("无需原材料的提示文本")]
    public TextMeshProUGUI noIngredientPrompt;
    [Tooltip("设置菜谱词条按钮")]
    public Button editEntryButton;
    [Tooltip("设置词条的画布")]
    public Canvas editEntryCanvas;

    //最后一次选择的食谱的ID
    private string lastSelectedItemId = "-1";

    //食材展示的实例列表
    private Queue<IngredientUIItem> ingredientItems = new Queue<IngredientUIItem>();


    protected override void BindOtherButtons()
    {
        editEntryButton.onClick.AddListener(() => ActivateEditEntryCanvas(allRecipesViewModel.SelectedItem));
    }

    private void ActivateEditEntryCanvas(FoodRecipe foodRecipe)
    {
        editEntryCanvas.gameObject.SetActive(true);
        editEntryCanvas.GetComponent<EditEntry>().RefreshDetail(foodRecipe);
    }

    protected override void RefreshDetail()
    {
        FoodRecipe item = allRecipesViewModel.SelectedItem;
        if (item != null)
        {
            itemIconImage.enabled = true;
            itemNameText.enabled = true;
            itemPriceText.enabled = true;
            priceIconImage.enabled = true;
            
            if (item.ingredients.Count != 0) editEntryButton.gameObject.SetActive(true);  //如果有原材料,则可以编辑词条
            else editEntryButton.gameObject.SetActive(false);

            itemIconImage.sprite = allRecipesViewModel.GetSprite(item.spritePath);
            itemIconImage.SetNativeSize();
            //itemIconImage.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            itemNameText.text = item.name;
            itemPriceText.text = item.basePrice.ToString();

            RefreshIngredients(item);
        }
        else
        {
            itemIconImage.enabled = false;
            itemNameText.enabled = false;
            itemPriceText.enabled = false;
            priceIconImage.enabled = false;
            noIngredientPrompt.enabled = false;
            editEntryButton.gameObject.SetActive(false);

            ClearIngredients();
            lastSelectedItemId = "-1";
        }
    }

    /// <summary>
    /// 刷新食材列表
    /// </summary>
    private void RefreshIngredients(FoodRecipe item)
    {
        //选中同一个菜谱,不更新
        if (lastSelectedItemId == item.id)
            return;

        lastSelectedItemId = item.id;

        //清空现有显示
        ClearIngredients();

        Dictionary<string, int> ingredients = item.ingredients;

        //食材列表为空
        if (ingredients == null || ingredients.Count == 0)
        {
            noIngredientPrompt.enabled = true;
        }
        else
        {
            noIngredientPrompt.enabled = false;
            foreach ((string ingredientId, int quantity) in ingredients)
            {
                CreateIngredientItem(ingredientId, quantity);
            }
        }
    }

    /// <summary>
    /// 创建单个食材对象
    /// </summary>
    /// <param name="ingredientId"></param>
    private void CreateIngredientItem(string ingredientId, int quantity)
    {
        GameObject ingredientObject = Instantiate(ingredientUIItemPrefab, ingredientsParent);

        IngredientUIItem ingredient = ingredientObject.GetComponent<IngredientUIItem>();
        string path = FoodMaterials.LookUpFoodMaterial(ingredientId).spritePath;
        ingredient.SetUp(allRecipesViewModel.GetSprite(path), quantity);

        ingredientItems.Enqueue(ingredient);
    }

    /// <summary>
    /// 清空食材显示
    /// </summary>
    private void ClearIngredients()
    {
        while (ingredientItems.Count != 0)
        {
            var item = ingredientItems.Dequeue();

            item.Clear();
            Destroy(item.gameObject);
        }
    }

    //拖拽相关(由Drag Handler调用,这种设计使得视图和拖拽逻辑分离)
    private int draggingIndex = -1;
    public override void OnDragStart(int index)
    {
        draggingIndex = index;  //记录开始拖拽的槽位

        sourceView = this; //记录源头是自己

        Debug.Log($"拖拽开始: 索引={index}, 源背包={name}");
    }

    public override void OnDragEnd()
    {
        draggingIndex = -1; //拖拽结束清空记录

        Debug.Log("拖拽结束");
    }

    public override void OnDrop(int targetIndex)
    {
        if (DragState<FoodRecipe, RecipesUIItem>.FromIndex == -1)
        {
            return; //如果没有正在拖拽的物品,返回
        }

        if (DragState<FoodRecipe, RecipesUIItem>.SourceView == this) //若是同一个背包
        {
            //全部食谱同一背包内不可移动
        }
        else if (DragState<FoodRecipe, RecipesUIItem>.SourceView as TodaysRecipeView != null) //是今日菜谱背包
        {
            //尝试移动,注意是从源背包到目前背包
            var v = DragState<FoodRecipe, RecipesUIItem>.SourceView as TodaysRecipeView;
            bool success = v.todaysRecipeViewModel.RemoveItemAt(DragState<FoodRecipe, RecipesUIItem>.FromIndex);

            if (success)
            {
                v.todaysRecipeViewModel.Organize();
                v.todaysRecipeViewModel.CaculateRespectedTurnover();

               //刷新两个背包的页面
               DragState<FoodRecipe, RecipesUIItem>.SourceView.RefreshUI();
                RefreshUI();
            }
        }

        DragState<FoodRecipe, RecipesUIItem>.Reset();  //清空拖拽状态
    }
}
