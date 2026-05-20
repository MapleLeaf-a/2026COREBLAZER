using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Profiling.HierarchyFrameDataView;

public class AllRecipesView : RecipesView
{
    public AllRecipesViewModel allRecipesViewModel
    {
        get => viewModel as AllRecipesViewModel;

        set => viewModel = value; //value��C#����setter�е������Ĺؼ���,����ֵ���������ֵ
    }

    public virtual void InitBackpackView(AllRecipesModel recipesModel)
    {
        if (capacity <= 0 || itemsPerPage <= 0)
        {
            throw new UnityException("������ʼ������");
        }
        capacity = recipesModel.Capacity;
        this.recipesViewModel = new AllRecipesViewModel(recipesModel, itemsPerPage);
    }

    protected void OnEnable()
    {
        allRecipesViewModel.OnShowTip += OnShowTip;
    }

    protected void OnDisable()
    {
        allRecipesViewModel.OnShowTip -= OnShowTip;
    }

    [Header("�������")]
    [Tooltip("����")]
    public TextMeshProUGUI itemNameText;
    [Tooltip("�۸�")]
    public TextMeshProUGUI itemPriceText;
    [Tooltip("ͼ��")]
    public Image itemIconImage;
    [Tooltip("�۸�ͼ��")]
    public Image priceIconImage;
    [Tooltip("ʳ��չʾ������")]
    public Transform ingredientsParent;
    [Tooltip("����ʳ�����Ԥ����")]
    public GameObject ingredientUIItemPrefab;
    [Tooltip("����ԭ���ϵ���ʾ�ı�")]
    public TextMeshProUGUI noIngredientPrompt;
    [Tooltip("���ò��״�����ť")]
    public Button editEntryButton;
    [Tooltip("���ô����Ļ���")]
    public Canvas editEntryCanvas;

    [Header("��ʾԭ���ϲ���Ļ���")]
    public Canvas IngredientsNotEnoughCanvas;

    //���һ��ѡ���ʳ�׵�ID
    private string lastSelectedItemId = "-1";

    //ʳ��չʾ��ʵ���б�
    private Queue<IngredientUIItem> ingredientItems = new Queue<IngredientUIItem>();


    private void OnShowTip(TipType type)
    {
        switch (type)
        {
            case TipType.ShowCanvas:
                IngredientsNotEnoughCanvas.gameObject.SetActive(true);
                break;
        }
    }


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
            
            if (item.ingredients.Count != 0) editEntryButton.gameObject.SetActive(true);  //�����ԭ����,����Ա༭����
            else editEntryButton.gameObject.SetActive(false);

            itemIconImage.sprite = allRecipesViewModel.GetSprite(item.spritePath);
            itemIconImage.SetNativeSize();
            itemIconImage.transform.localScale = new Vector3(2f, 2f, 2f);
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
    /// ˢ��ʳ���б�
    /// </summary>
    private void RefreshIngredients(FoodRecipe item)
    {
        //ѡ��ͬһ������,������
        if (lastSelectedItemId == item.id)
            return;

        lastSelectedItemId = item.id;

        //���������ʾ
        ClearIngredients();

        Dictionary<string, int> ingredients = item.ingredients;

        //ʳ���б�Ϊ��
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
    /// ��������ʳ�Ķ���
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
    /// ���ʳ����ʾ
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

    //��ק���(��Drag Handler����,�������ʹ����ͼ����ק�߼�����)
    private int draggingIndex = -1;
    public override void OnDragStart(int index)
    {
        draggingIndex = index;  //��¼��ʼ��ק�Ĳ�λ

        sourceView = this; //��¼Դͷ���Լ�

        Debug.Log($"��ק��ʼ: ����={index}, Դ����={name}");
    }

    public override void OnDragEnd()
    {
        draggingIndex = -1; //��ק������ռ�¼

        Debug.Log("��ק����");
    }

    public override void OnDrop(int targetIndex)
    {
        if (DragState<FoodRecipe, RecipesUIItem>.FromIndex == -1)
        {
            return; //���û��������ק����Ʒ,����
        }

        if (DragState<FoodRecipe, RecipesUIItem>.SourceView == this) //����ͬһ������
        {
            //ȫ��ʳ��ͬһ�����ڲ����ƶ�
        }
        else if (DragState<FoodRecipe, RecipesUIItem>.SourceView as TodaysRecipeView != null) //�ǽ��ղ��ױ���
        {
            //�����ƶ�,ע���Ǵ�Դ������Ŀǰ����
            var v = DragState<FoodRecipe, RecipesUIItem>.SourceView as TodaysRecipeView;
            int fromIndex = DragState<FoodRecipe, RecipesUIItem>.FromIndex;
            bool success = v.todaysRecipeViewModel.RemoveRecipeAndReturnIngredients(fromIndex);

            if (success)
            {
                v.todaysRecipeViewModel.Organize();
                v.todaysRecipeViewModel.CaculateRespectedTurnover();

               //ˢ������������ҳ��
               DragState<FoodRecipe, RecipesUIItem>.SourceView.RefreshUI();
                RefreshUI();
            }
        }

        DragState<FoodRecipe, RecipesUIItem>.Reset();  //�����ק״̬
    }
}
