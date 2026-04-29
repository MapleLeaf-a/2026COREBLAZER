using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditEntry : MonoBehaviour
{
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

    //食材展示GO列表
    private List<GameObject> ingredientGOs = new List<GameObject>();

    [Header("词条")]
    [Tooltip("设定好的词条")]
    public List<Image> entryImages;
    [Tooltip("词条展示父物体")]
    public Transform entrysParent;
    [Tooltip("词条填充预制体")]
    public GameObject entryFillUpPrefab;

    //词条展示GO列表
    private List<Description> entryDescriptions = new List<Description>();
    //词条的index
    private Dictionary<int, int> entryIndexes;
    //预加入entryIndexes的index
    int preEntryIndex;

    [Header("按钮")]
    [Tooltip("重置词条按钮")]
    public Button resetEntryButton;
    [Tooltip("完成设置词条按钮")]
    public Button completeEntrySettingButton;



    //拖拽相关
    //从哪里来
    private Sprite fromSprite;
    //到哪个



    //当前设置的食谱
    private FoodRecipe foodRecipe;


    //设置词条的canvas
    private Canvas canvas;

    private void OnEnable()
    {
        resetEntryButton.onClick.AddListener(ResetFoodEntry);
        completeEntrySettingButton.onClick.AddListener(SetFoodEntry);
    
        canvas = GetComponent<Canvas>();
    }

    public void RefreshDetail(FoodRecipe item)
    {
        foodRecipe = item;
        if (item != null)
        {
            itemIconImage.enabled = true;
            itemNameText.enabled = true;
            itemPriceText.enabled = true;
            priceIconImage.enabled = true;

            itemIconImage.sprite = SpriteStatic.GetSprite(item.spritePath);
            itemNameText.text = item.name;
            itemPriceText.text = item.basePrice.ToString();

            RefreshIngredients(item);

            RefreshEntryFillUpText(item);
        }
        else
        {
            itemIconImage.enabled = false;
            itemNameText.enabled = false;
            itemPriceText.enabled = false;
            priceIconImage.enabled = false;
            noIngredientPrompt.enabled = false;
        }
    }

    private void RefreshIngredients(FoodRecipe item)
    {
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

    private void CreateIngredientItem(string ingredientId, int quantity)
    {
        GameObject ingredientObject = Instantiate(ingredientUIItemPrefab, ingredientsParent);

        IngredientUIItem ingredient = ingredientObject.GetComponent<IngredientUIItem>();
        string path = FoodMaterials.LookUpFoodMaterial(ingredientId).spritePath;
        ingredient.SetUp(SpriteStatic.GetSprite(path), quantity);

        ingredientGOs.Add(ingredientObject);
    }

    private void RefreshEntryFillUpText(FoodRecipe item)
    {
        Dictionary<string, int> ingredients = item.ingredients;

        if (ingredients == null || ingredients.Count == 0)
        {
            return;
        }

        foreach ((string ingredientId, int quantity) in ingredients)
        {
            GameObject entryObject = Instantiate(entryFillUpPrefab, entrysParent);
            FoodMaterial foodMaterial = FoodMaterials.LookUpFoodMaterial(ingredientId);
            var description = entryObject.GetComponent<Description>();
            description.SetUp(null, foodMaterial.name);

            entryDescriptions.Add(description);
        }

        entryIndexes = Foods.GetFoodEntry(item.id);
        foreach ((int i, int j) in entryIndexes)
        {
            if (j != -1)
            {
                entryDescriptions[i].SetUp(entryImages[j].sprite);
            }
        }
    }

    private void OnDisable()
    {
        foreach (var go in ingredientGOs)
        { 
            Destroy(go);
        }
        foreach (var des in entryDescriptions)
        { 
            Destroy(des.gameObject);
        }

        entryDescriptions.Clear();
    }

    public void OnDragBegin(int index)
    {
        this.fromSprite = entryImages[index].sprite;
        preEntryIndex = index;
    }

    public void OnDrop(int slotIndex)
    {
        entryDescriptions[slotIndex].SetUp(fromSprite);
        entryIndexes[slotIndex] = preEntryIndex;
    }

    private void ResetFoodEntry()
    {
        foreach (var go in entryDescriptions)
        {
            go.SetUp(null);
        }

        foreach (int key in entryIndexes.Keys.ToList())
        {
            entryIndexes[key] = -1;
        }
    }

    private void SetFoodEntry()
    {
        canvas.gameObject.SetActive(false);
    }
}
