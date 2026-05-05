using AK.Wwise;
using StaticTemplates.MusicGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemToBeExcavated : MonoBehaviour
{
    //存放可以在此游戏中被获取的食材的ID
    private static readonly string[] foodMaterialIDs = { "103", "102" };

    [Header("食物图标的Sprite组件")]
    public Image foodIcon;

    [Header("声纹播放")]
    public Animation voiceprintAnimation;

    //被埋的食材
    private FoodMaterial foodMaterial;

    private void Start()
    {
        foodIcon.enabled = false;

        foodMaterial = FoodMaterials.LookUpFoodMaterial(foodMaterialIDs[Random.Range(0, foodMaterialIDs.Length)]);

        foodIcon.sprite = SpriteStatic.GetSprite(foodMaterial.spritePath);
        foodIcon.SetNativeSize();
    }

    public void ShowSprite()
    { 
        foodIcon.enabled = true;
        Invoke(nameof(DestroyGO), 1f);
    }

    public void PlayLoopAnimation()
    {
        voiceprintAnimation.Play();
        voiceprintAnimation.wrapMode = WrapMode.Loop;
    }

    public void StopPlayingAnimation()
    {
        voiceprintAnimation.Stop();
    }

    public void DestroyGO()
    {
        Destroy(gameObject);
    }
}
