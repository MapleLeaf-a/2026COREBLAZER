using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualStaticEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/VisualStaticEditor")]
    public static void ShowExample()
    {
        VisualStaticEditor wnd = GetWindow<VisualStaticEditor>();
        wnd.titleContent = new GUIContent("VisualStaticEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        TwoPaneSplitView splitView = new TwoPaneSplitView(0,250,TwoPaneSplitViewOrientation.Horizontal);
        
        root.Add(splitView);
        
        ListView leftTreeList = new ListView();
        VisualElement rightPanel = new VisualElement();

        List<Type> types = GetClassList();
        
        leftTreeList.makeItem = ()=> new Label();
        leftTreeList.bindItem = (item, index) =>
        {
            if (item is Label label) label.text = types[index].Name;
        };
        leftTreeList.itemsSource = types;
        
        splitView.Add(leftTreeList);
        splitView.Add(rightPanel);
        
    }

    private List<Type> GetClassList()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        var classTypes = assembly.GetTypes()
            .Where(t => t.IsClass && t.Namespace != null && ( t.Namespace == "StaticTemplates"||t.Namespace.StartsWith("StaticTemplates.")));
        return classTypes.ToList();
    }
}
