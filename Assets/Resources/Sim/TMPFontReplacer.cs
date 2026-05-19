using UnityEngine;
using TMPro;
using UnityEditor;
using System.Collections.Generic;

public class TMPFontReplacer : EditorWindow
{
    private TMP_FontAsset targetFont;
    private bool includeInactive = true;

    [MenuItem("Tools/批量替换 TMP 字体")]
    public static void ShowWindow()
    {
        GetWindow<TMPFontReplacer>("字体替换工具");
    }

    void OnGUI()
    {
        GUILayout.Label("替换场景中所有 TextMeshPro 的字体", EditorStyles.boldLabel);

        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("目标字体", targetFont, typeof(TMP_FontAsset), false);
        includeInactive = EditorGUILayout.Toggle("包含非激活物体", includeInactive);

        if (GUILayout.Button("替换", GUILayout.Height(30)))
        {
            ReplaceAllFonts();
        }
    }

    void ReplaceAllFonts()
    {
        if (targetFont == null)
        {
            Debug.LogError("请先指定目标字体！");
            return;
        }

        // 查找场景中所有 TextMeshPro 组件
        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(includeInactive);

        int count = 0;
        foreach (TMP_Text text in allTexts)
        {
            Undo.RecordObject(text, "Change Font");
            text.font = targetFont;
            EditorUtility.SetDirty(text);
            count++;
        }

        Debug.Log($"已替换 {count} 个文本组件为字体: {targetFont.name}");
    }
}