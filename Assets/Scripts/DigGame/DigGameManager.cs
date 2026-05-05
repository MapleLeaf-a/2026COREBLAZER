using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigGameManager : MonoBehaviour
{
    [Tooltip("待挖物品预制体")]
    public GameObject itemsPrefab;
    [Tooltip("物品生成的父物体")]
    public GameObject parent;

    [Tooltip("物品之间的最小距离")]
    public float minDistance = 100f;
    [Tooltip("生成数量")]
    public int spawnCount = 5;

    private List<Vector2> spawnedPositions = new List<Vector2>();

   
    private void Start()
    {
        SpawnMultipleUI();
    }

    private void SpawnMultipleUI()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnRandomUI();
        }
    }

    private void SpawnRandomUI()
    {
        // 实例化
        GameObject newUI = Instantiate(itemsPrefab, parent.transform);
        RectTransform rect = newUI.GetComponent<RectTransform>();

        // 获取 UI 元素尺寸
        float width = rect.rect.width;
        float height = rect.rect.height;

        // 计算安全范围
        float minX = width / 2;
        float maxX = Screen.width - width / 2;
        float minY = height / 2;
        float maxY = Screen.height - height / 2;

        Vector2 finalPos;

        do
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            finalPos = new Vector2(randomX, randomY);
        }
        while (IsOverlapWithOthers(finalPos, minDistance));

        rect.position = finalPos;
        spawnedPositions.Add(finalPos);
    }

    private bool IsOverlapWithOthers(Vector2 newPos, float minDistance)
    {
        foreach (Vector2 existingPos in spawnedPositions)
        {
            if (Vector2.Distance(newPos, existingPos) < minDistance)
            {
                return true;
            }
        }
        return false;
    }
}
