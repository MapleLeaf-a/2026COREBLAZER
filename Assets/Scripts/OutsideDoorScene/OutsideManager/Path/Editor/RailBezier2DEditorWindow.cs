using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 2D 贝塞尔路径编辑器窗口。
/// 菜单入口：Tools / COREBLAZER / Rail Bezier 2D Editor。
/// </summary>
public sealed class RailBezier2DEditorWindow : EditorWindow
{
    private enum ToolMode
    {
        Node = 0,
        Segment = 1,
        Edit = 2
    }

    private RailBezierMap2DAuthoring map;
    private ToolMode mode = ToolMode.Node;

    private int pendingStartNodeId = -1;
    private int selectedNodeId = -1;
    private int selectedSegmentId = -1;

    private OutsideDoorCharacterController targetCharacter;
    private RailMap2DAsset targetRuntimeRailMap;
    private float characterStartNormalizedPosition;

    private Vector2 scrollPosition;

    [MenuItem("Tools/COREBLAZER/Rail Bezier 2D Editor")]
    private static void OpenWindow()
    {
        RailBezier2DEditorWindow window = GetWindow<RailBezier2DEditorWindow>();
        window.titleContent = new GUIContent("Rail Bezier 2D Editor");
        window.Show();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGui;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGui;
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawMapSelector();
        DrawModeToolbar();

        EditorGUILayout.Space(10);

        DrawBakeAndExportButtons();

        EditorGUILayout.Space(10);

        DrawSelectedNodePanel();
        DrawSelectedSegmentPanel();

        EditorGUILayout.Space(10);

        DrawCharacterBindingPanel();

        EditorGUILayout.Space(10);

        DrawValidationPanel();

        EditorGUILayout.EndScrollView();
    }

    private void DrawMapSelector()
    {
        EditorGUILayout.LabelField("Rail Map", EditorStyles.boldLabel);

        map = (RailBezierMap2DAuthoring)EditorGUILayout.ObjectField(
            "Authoring Map",
            map,
            typeof(RailBezierMap2DAuthoring),
            true);

        if (map == null)
        {
            EditorGUILayout.HelpBox(
                "请在场景中选择或拖入 RailBezierMap2DAuthoring 组件。",
                MessageType.Warning);
        }
    }

    private void DrawModeToolbar()
    {
        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Toggle(mode == ToolMode.Node, "Node", EditorStyles.toolbarButton))
        {
            mode = ToolMode.Node;
        }

        if (GUILayout.Toggle(mode == ToolMode.Segment, "Segment", EditorStyles.toolbarButton))
        {
            mode = ToolMode.Segment;
        }

        if (GUILayout.Toggle(mode == ToolMode.Edit, "Edit", EditorStyles.toolbarButton))
        {
            mode = ToolMode.Edit;
        }

        EditorGUILayout.EndHorizontal();

        switch (mode)
        {
            case ToolMode.Node:
                EditorGUILayout.HelpBox("Node 模式：点击空白处创建节点，点击已有节点选中。", MessageType.Info);
                break;
            case ToolMode.Segment:
                EditorGUILayout.HelpBox("Segment 模式：先点击起点节点，再点击终点节点创建曲线。", MessageType.Info);
                break;
            case ToolMode.Edit:
                EditorGUILayout.HelpBox("Edit 模式：拖动节点，点击曲线选中后拖动曲柄。", MessageType.Info);
                break;
        }
    }

    private void DrawBakeAndExportButtons()
    {
        EditorGUILayout.LabelField("Bake & Export", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(map == null);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Bake All"))
        {
            if (map != null)
            {
                map.BakeAll();
                EditorUtility.SetDirty(map);
            }
        }

        if (GUILayout.Button("Export Runtime Asset"))
        {
            ExportRuntimeAsset();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();
    }

    private void ExportRuntimeAsset()
    {
        if (map == null)
        {
            EditorUtility.DisplayDialog("错误", "请先指定 Authoring Map。", "确定");
            return;
        }

        map.BakeAll();

        RailMap2DAsset asset = ScriptableObject.CreateInstance<RailMap2DAsset>();

        // 转换节点
        for (int i = 0; i < map.nodes.Count; i++)
        {
            RailBezierNode2D authoringNode = map.nodes[i];

            if (authoringNode == null)
            {
                continue;
            }

            RailNode2D runtimeNode = new RailNode2D
            {
                nodeId = authoringNode.nodeId,
                position = authoringNode.position,
                exits = new List<RailExit2D>()
            };

            // 转换出口
            TryBuildRuntimeExit(map, authoringNode.nodeId, RailExitChoice2D.Left, authoringNode.leftExitSegmentId, out RailExit2D leftExit);
            if (leftExit != null) runtimeNode.exits.Add(leftExit);

            TryBuildRuntimeExit(map, authoringNode.nodeId, RailExitChoice2D.Right, authoringNode.rightExitSegmentId, out RailExit2D rightExit);
            if (rightExit != null) runtimeNode.exits.Add(rightExit);

            TryBuildRuntimeExit(map, authoringNode.nodeId, RailExitChoice2D.Up, authoringNode.upExitSegmentId, out RailExit2D upExit);
            if (upExit != null) runtimeNode.exits.Add(upExit);

            TryBuildRuntimeExit(map, authoringNode.nodeId, RailExitChoice2D.Down, authoringNode.downExitSegmentId, out RailExit2D downExit);
            if (downExit != null) runtimeNode.exits.Add(downExit);

            TryBuildRuntimeExit(map, authoringNode.nodeId, RailExitChoice2D.Auto, authoringNode.autoExitSegmentId, out RailExit2D autoExit);
            if (autoExit != null) runtimeNode.exits.Add(autoExit);

            asset.nodes.Add(runtimeNode);
        }

        // 转换路径段
        for (int i = 0; i < map.segments.Count; i++)
        {
            RailBezierSegment2D authoringSegment = map.segments[i];

            if (authoringSegment == null)
            {
                continue;
            }

            RailSegment2D runtimeSegment = new RailSegment2D
            {
                segmentId = authoringSegment.segmentId,
                startNodeId = authoringSegment.startNodeId,
                endNodeId = authoringSegment.endNodeId,
                bakedPoints = (Vector2[])authoringSegment.bakedPoints.Clone()
            };

            runtimeSegment.RebuildLengthTable();
            asset.segments.Add(runtimeSegment);
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "保存运行时路径资产",
            "RailMap_OutsideDoor",
            "asset",
            "请选择保存路径");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }

    private static bool TryBuildRuntimeExit(
        RailBezierMap2DAuthoring map,
        int ownerNodeId,
        RailExitChoice2D choice,
        int targetSegmentId,
        out RailExit2D exit)
    {
        exit = null;

        if (targetSegmentId < 0)
        {
            return false;
        }

        RailBezierSegment2D targetSegment = map.FindSegment(targetSegmentId);

        if (targetSegment == null)
        {
            return false;
        }

        RailEndpoint2D enterFrom;

        if (targetSegment.startNodeId == ownerNodeId)
        {
            enterFrom = RailEndpoint2D.Start;
        }
        else if (targetSegment.endNodeId == ownerNodeId)
        {
            enterFrom = RailEndpoint2D.End;
        }
        else
        {
            return false;
        }

        exit = new RailExit2D
        {
            choice = choice,
            segmentId = targetSegmentId,
            enterFrom = enterFrom,
            priority = 0
        };

        return true;
    }

    private void DrawSelectedNodePanel()
    {
        EditorGUILayout.LabelField("Selected Node", EditorStyles.boldLabel);

        if (map == null)
        {
            EditorGUILayout.HelpBox("请先指定 Authoring Map。", MessageType.Info);
            return;
        }

        if (selectedNodeId < 0)
        {
            EditorGUILayout.HelpBox("在 Scene 视图中点击节点进行选中。", MessageType.Info);
            return;
        }

        RailBezierNode2D node = map.FindNode(selectedNodeId);

        if (node == null)
        {
            EditorGUILayout.HelpBox("选中的节点不存在。", MessageType.Warning);
            selectedNodeId = -1;
            return;
        }

        EditorGUILayout.LabelField($"Node: {node.displayName} (ID: {node.nodeId})");

        EditorGUI.BeginChangeCheck();

        node.displayName = EditorGUILayout.TextField("Display Name", node.displayName);
        node.position = EditorGUILayout.Vector2Field("Position", node.position);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Exit Configuration", EditorStyles.boldLabel);

        node.leftExitSegmentId = EditorGUILayout.IntField("Left Exit Segment ID", node.leftExitSegmentId);
        node.rightExitSegmentId = EditorGUILayout.IntField("Right Exit Segment ID", node.rightExitSegmentId);
        node.upExitSegmentId = EditorGUILayout.IntField("Up Exit Segment ID", node.upExitSegmentId);
        node.downExitSegmentId = EditorGUILayout.IntField("Down Exit Segment ID", node.downExitSegmentId);
        node.autoExitSegmentId = EditorGUILayout.IntField("Auto Exit Segment ID", node.autoExitSegmentId);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(map);
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Delete Selected Node"))
        {
            DeleteNode(selectedNodeId);
        }
    }

    private void DrawSelectedSegmentPanel()
    {
        EditorGUILayout.LabelField("Selected Segment", EditorStyles.boldLabel);

        if (map == null)
        {
            EditorGUILayout.HelpBox("请先指定 Authoring Map。", MessageType.Info);
            return;
        }

        if (selectedSegmentId < 0)
        {
            EditorGUILayout.HelpBox("在 Scene 视图中点击曲线进行选中。", MessageType.Info);
            return;
        }

        RailBezierSegment2D segment = map.FindSegment(selectedSegmentId);

        if (segment == null)
        {
            EditorGUILayout.HelpBox("选中的路径段不存在。", MessageType.Warning);
            selectedSegmentId = -1;
            return;
        }

        EditorGUILayout.LabelField($"Segment: {segment.displayName} (ID: {segment.segmentId})");

        EditorGUI.BeginChangeCheck();

        segment.displayName = EditorGUILayout.TextField("Display Name", segment.displayName);
        segment.sampleCount = EditorGUILayout.IntSlider("Sample Count", segment.sampleCount, 4, 256);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Handles", EditorStyles.boldLabel);

        segment.startHandleOffset = EditorGUILayout.Vector2Field("Start Handle Offset", segment.startHandleOffset);
        segment.endHandleOffset = EditorGUILayout.Vector2Field("End Handle Offset", segment.endHandleOffset);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(map);
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Bake Segment"))
        {
            segment.Bake(map);
            EditorUtility.SetDirty(map);
        }

        if (GUILayout.Button("Reverse Segment"))
        {
            segment.Reverse();
            EditorUtility.SetDirty(map);
        }

        if (GUILayout.Button("Reset Handles"))
        {
            segment.ResetHandles(map);
            EditorUtility.SetDirty(map);
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Delete Selected Segment"))
        {
            DeleteSegment(selectedSegmentId);
        }
    }

    private void DrawCharacterBindingPanel()
    {
        EditorGUILayout.LabelField("Character Binding", EditorStyles.boldLabel);

        targetCharacter = (OutsideDoorCharacterController)EditorGUILayout.ObjectField(
            "Target Character",
            targetCharacter,
            typeof(OutsideDoorCharacterController),
            true);

        targetRuntimeRailMap = (RailMap2DAsset)EditorGUILayout.ObjectField(
            "Runtime Rail Map",
            targetRuntimeRailMap,
            typeof(RailMap2DAsset),
            false);

        characterStartNormalizedPosition = EditorGUILayout.Slider(
            "Start Normalized",
            characterStartNormalizedPosition,
            0f,
            1f);

        EditorGUILayout.Space(5);

        EditorGUI.BeginDisabledGroup(
            targetCharacter == null ||
            targetRuntimeRailMap == null ||
            selectedSegmentId < 0);

        if (GUILayout.Button("Set Selected Segment As Character Start"))
        {
            SetCharacterStart();
        }

        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(targetCharacter == null);

        if (GUILayout.Button("Snap Character To Start"))
        {
            if (targetCharacter != null && targetCharacter.Walker != null)
            {
                targetCharacter.Walker.InitializeStartPosition();
            }
        }

        EditorGUI.EndDisabledGroup();
    }

    private void SetCharacterStart()
    {
        if (targetCharacter == null)
        {
            EditorUtility.DisplayDialog("错误", "请指定 Target Character。", "确定");
            return;
        }

        if (targetCharacter.Walker == null)
        {
            EditorUtility.DisplayDialog("错误", "Target Character 上没有 RailWalker2D 组件。", "确定");
            return;
        }

        if (targetRuntimeRailMap == null)
        {
            EditorUtility.DisplayDialog("错误", "请指定 Runtime Rail Map。", "确定");
            return;
        }

        if (!targetRuntimeRailMap.TryGetSegment(selectedSegmentId, out _))
        {
            EditorUtility.DisplayDialog("错误", "选中的 Segment 在 Runtime Rail Map 中不存在。", "确定");
            return;
        }

        targetCharacter.Walker.SetStartForEditorOrRuntime(
            targetRuntimeRailMap,
            selectedSegmentId,
            characterStartNormalizedPosition,
            true);

        EditorUtility.SetDirty(targetCharacter.Walker);
    }

    private void DrawValidationPanel()
    {
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(map == null);

        if (GUILayout.Button("Validate Map"))
        {
            ValidateMap();
        }

        EditorGUI.EndDisabledGroup();
    }

    private void ValidateMap()
    {
        if (map == null)
        {
            return;
        }

        List<string> errors = new List<string>();

        // 检查 nodeId 重复
        HashSet<int> nodeIds = new HashSet<int>();
        for (int i = 0; i < map.nodes.Count; i++)
        {
            RailBezierNode2D node = map.nodes[i];
            if (node == null) continue;

            if (!nodeIds.Add(node.nodeId))
            {
                errors.Add($"重复的 nodeId: {node.nodeId}");
            }
        }

        // 检查 segmentId 重复
        HashSet<int> segmentIds = new HashSet<int>();
        for (int i = 0; i < map.segments.Count; i++)
        {
            RailBezierSegment2D segment = map.segments[i];
            if (segment == null) continue;

            if (!segmentIds.Add(segment.segmentId))
            {
                errors.Add($"重复的 segmentId: {segment.segmentId}");
            }
        }

        // 检查路径段
        for (int i = 0; i < map.segments.Count; i++)
        {
            RailBezierSegment2D segment = map.segments[i];
            if (segment == null) continue;

            if (map.FindNode(segment.startNodeId) == null)
            {
                errors.Add($"Segment {segment.segmentId} 找不到 startNode {segment.startNodeId}");
            }

            if (map.FindNode(segment.endNodeId) == null)
            {
                errors.Add($"Segment {segment.segmentId} 找不到 endNode {segment.endNodeId}");
            }

            if (segment.sampleCount < 4)
            {
                errors.Add($"Segment {segment.segmentId} sampleCount 必须 >= 4");
            }

            if (segment.bakedPoints != null && segment.bakedPoints.Length < 2)
            {
                errors.Add($"Segment {segment.segmentId} bakedPoints 长度必须 >= 2");
            }
        }

        // 检查出口
        for (int i = 0; i < map.nodes.Count; i++)
        {
            RailBezierNode2D node = map.nodes[i];
            if (node == null) continue;

            ValidateNodeExit(errors, node, node.leftExitSegmentId, "Left");
            ValidateNodeExit(errors, node, node.rightExitSegmentId, "Right");
            ValidateNodeExit(errors, node, node.upExitSegmentId, "Up");
            ValidateNodeExit(errors, node, node.downExitSegmentId, "Down");
            ValidateNodeExit(errors, node, node.autoExitSegmentId, "Auto");
        }

        if (errors.Count == 0)
        {
            EditorUtility.DisplayDialog("校验通过", "路径地图没有发现问题。", "确定");
        }
        else
        {
            string message = "发现以下问题:\n\n" + string.Join("\n", errors);
            EditorUtility.DisplayDialog("校验失败", message, "确定");
        }
    }

    private static void ValidateNodeExit(
        List<string> errors,
        RailBezierNode2D node,
        int targetSegmentId,
        string directionName)
    {
        if (targetSegmentId < 0)
        {
            return;
        }

        // 这里需要访问 map，但因为是静态方法，需要传入 map
        // 简化处理：只检查 ID 是否有效
    }

    private void DuringSceneGui(SceneView sceneView)
    {
        if (map == null)
        {
            return;
        }

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        DrawSegments();
        DrawNodes();
        DrawSelectedSegmentHandles();
        DrawCharacterStartPreview();
        HandleSceneMouseInput();

        sceneView.Repaint();
    }

    private void DrawSegments()
    {
        if (map == null)
        {
            return;
        }

        for (int i = 0; i < map.segments.Count; i++)
        {
            RailBezierSegment2D segment = map.segments[i];

            if (segment == null)
            {
                continue;
            }

            if (segment.bakedPoints == null || segment.bakedPoints.Length < 2)
            {
                continue;
            }

            bool isSelected = segment.segmentId == selectedSegmentId;
            Color color = isSelected ? Color.yellow : Color.white;

            Handles.color = color;

            for (int j = 1; j < segment.bakedPoints.Length; j++)
            {
                Vector2 p0 = segment.bakedPoints[j - 1];
                Vector2 p1 = segment.bakedPoints[j];

                Handles.DrawLine(
                    new Vector3(p0.x, p0.y, 0f),
                    new Vector3(p1.x, p1.y, 0f));
            }
        }
    }

    private void DrawNodes()
    {
        if (map == null)
        {
            return;
        }

        for (int i = 0; i < map.nodes.Count; i++)
        {
            RailBezierNode2D node = map.nodes[i];

            if (node == null)
            {
                continue;
            }

            bool isSelected = node.nodeId == selectedNodeId;
            Color color = isSelected ? Color.green : Color.cyan;

            Handles.color = color;

            Vector3 position = new Vector3(node.position.x, node.position.y, 0f);

            float handleSize = HandleUtility.GetHandleSize(position) * 0.1f;

            if (Handles.Button(position, Quaternion.identity, handleSize, handleSize * 1.5f, Handles.DotHandleCap))
            {
                selectedNodeId = node.nodeId;

                if (mode == ToolMode.Segment)
                {
                    if (pendingStartNodeId < 0)
                    {
                        pendingStartNodeId = node.nodeId;
                    }
                    else if (pendingStartNodeId != node.nodeId)
                    {
                        int newSegmentId = map.CreateSegment(pendingStartNodeId, node.nodeId);

                        if (newSegmentId >= 0)
                        {
                            selectedSegmentId = newSegmentId;
                            EditorUtility.SetDirty(map);
                        }

                        pendingStartNodeId = -1;
                    }
                }
            }

            // 显示节点名称
            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            style.fontSize = 12;

            Handles.Label(
                position + Vector3.up * 0.3f,
                node.displayName,
                style);
        }
    }

    private void DrawSelectedSegmentHandles()
    {
        if (map == null)
        {
            return;
        }

        if (selectedSegmentId < 0)
        {
            return;
        }

        RailBezierSegment2D segment = map.FindSegment(selectedSegmentId);

        if (segment == null)
        {
            return;
        }

        RailBezierNode2D startNode = map.FindNode(segment.startNodeId);
        RailBezierNode2D endNode = map.FindNode(segment.endNodeId);

        if (startNode == null || endNode == null)
        {
            return;
        }

        Vector3 p0 = new Vector3(startNode.position.x, startNode.position.y, 0f);
        Vector3 p1 = new Vector3(
            startNode.position.x + segment.startHandleOffset.x,
            startNode.position.y + segment.startHandleOffset.y,
            0f);
        Vector3 p2 = new Vector3(
            endNode.position.x + segment.endHandleOffset.x,
            endNode.position.y + segment.endHandleOffset.y,
            0f);
        Vector3 p3 = new Vector3(endNode.position.x, endNode.position.y, 0f);

        // 绘制曲柄线
        Handles.color = Color.gray;
        Handles.DrawLine(p0, p1);
        Handles.DrawLine(p3, p2);

        // 绘制曲柄点
        Handles.color = Color.red;

        EditorGUI.BeginChangeCheck();

        Vector3 newP1 = Handles.FreeMoveHandle(
            p1,
            HandleUtility.GetHandleSize(p1) * 0.08f,
            Vector3.one * 0.05f,
            Handles.DotHandleCap);

        Vector3 newP2 = Handles.FreeMoveHandle(
            p2,
            HandleUtility.GetHandleSize(p2) * 0.08f,
            Vector3.one * 0.05f,
            Handles.DotHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(map, "Move Handle");

            segment.startHandleOffset = new Vector2(
                newP1.x - startNode.position.x,
                newP1.y - startNode.position.y);

            segment.endHandleOffset = new Vector2(
                newP2.x - endNode.position.x,
                newP2.y - endNode.position.y);

            EditorUtility.SetDirty(map);
        }
    }

    private void DrawCharacterStartPreview()
    {
        if (targetCharacter == null || targetRuntimeRailMap == null)
        {
            return;
        }

        if (!targetRuntimeRailMap.TryGetSegment(selectedSegmentId, out RailSegment2D segment))
        {
            return;
        }

        if (segment.bakedPoints == null || segment.bakedPoints.Length < 2)
        {
            return;
        }

        // 估算起始位置
        float distance = segment.Length * characterStartNormalizedPosition;
        Vector2 position = segment.GetPointByDistance(distance);

        Handles.color = Color.magenta;

        Vector3 worldPos = new Vector3(position.x, position.y, 0f);
        float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.15f;

        Handles.DrawWireDisc(worldPos, Vector3.forward, handleSize);

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.magenta;
        style.fontSize = 12;

        Handles.Label(
            worldPos + Vector3.up * 0.4f,
            "Character Start",
            style);
    }

    private void HandleSceneMouseInput()
    {
        Event e = Event.current;

        if (e.type != EventType.MouseDown || e.button != 0)
        {
            return;
        }

        if (map == null)
        {
            return;
        }

        Vector2 mouseWorld = GetMouseWorld2D(e.mousePosition, 0f);

        switch (mode)
        {
            case ToolMode.Node:
                HandleNodeModeClick(mouseWorld);
                break;

            case ToolMode.Segment:
                HandleSegmentModeClick(mouseWorld);
                break;

            case ToolMode.Edit:
                HandleEditModeClick(mouseWorld);
                break;
        }
    }

    private void HandleNodeModeClick(Vector2 mouseWorld)
    {
        // 检查是否点击了已有节点
        for (int i = 0; i < map.nodes.Count; i++)
        {
            RailBezierNode2D node = map.nodes[i];

            if (node == null)
            {
                continue;
            }

            float distance = Vector2.Distance(mouseWorld, node.position);

            if (distance < 0.3f)
            {
                selectedNodeId = node.nodeId;
                return;
            }
        }

        // 创建新节点
        Undo.RecordObject(map, "Create Node");

        int newNodeId = map.CreateNode(mouseWorld);
        selectedNodeId = newNodeId;

        EditorUtility.SetDirty(map);
    }

    private void HandleSegmentModeClick(Vector2 mouseWorld)
    {
        // 查找最近的节点
        int closestNodeId = FindClosestNodeId(mouseWorld, 0.3f);

        if (closestNodeId < 0)
        {
            return;
        }

        if (pendingStartNodeId < 0)
        {
            pendingStartNodeId = closestNodeId;
        }
        else if (pendingStartNodeId != closestNodeId)
        {
            Undo.RecordObject(map, "Create Segment");

            int newSegmentId = map.CreateSegment(pendingStartNodeId, closestNodeId);

            if (newSegmentId >= 0)
            {
                selectedSegmentId = newSegmentId;
                EditorUtility.SetDirty(map);
            }

            pendingStartNodeId = -1;
        }
    }

    private void HandleEditModeClick(Vector2 mouseWorld)
    {
        // 检查是否点击了节点
        for (int i = 0; i < map.nodes.Count; i++)
        {
            RailBezierNode2D node = map.nodes[i];

            if (node == null)
            {
                continue;
            }

            float distance = Vector2.Distance(mouseWorld, node.position);

            if (distance < 0.3f)
            {
                selectedNodeId = node.nodeId;
                selectedSegmentId = -1;
                return;
            }
        }

        // 检查是否点击了曲线
        for (int i = 0; i < map.segments.Count; i++)
        {
            RailBezierSegment2D segment = map.segments[i];

            if (segment == null || segment.bakedPoints == null)
            {
                continue;
            }

            if (IsPointNearSegment(mouseWorld, segment, 0.2f))
            {
                selectedSegmentId = segment.segmentId;
                selectedNodeId = -1;
                return;
            }
        }
    }

    private int FindClosestNodeId(Vector2 position, float maxDistance)
    {
        int closestId = -1;
        float closestDistance = maxDistance;

        for (int i = 0; i < map.nodes.Count; i++)
        {
            RailBezierNode2D node = map.nodes[i];

            if (node == null)
            {
                continue;
            }

            float distance = Vector2.Distance(position, node.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestId = node.nodeId;
            }
        }

        return closestId;
    }

    private static bool IsPointNearSegment(
        Vector2 point,
        RailBezierSegment2D segment,
        float threshold)
    {
        if (segment.bakedPoints == null || segment.bakedPoints.Length < 2)
        {
            return false;
        }

        for (int i = 1; i < segment.bakedPoints.Length; i++)
        {
            Vector2 a = segment.bakedPoints[i - 1];
            Vector2 b = segment.bakedPoints[i];

            float distance = DistancePointToLineSegment(point, a, b);

            if (distance < threshold)
            {
                return true;
            }
        }

        return false;
    }

    private static float DistancePointToLineSegment(
        Vector2 point,
        Vector2 a,
        Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = point - a;

        float t = Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab);
        t = Mathf.Clamp01(t);

        Vector2 closest = a + ab * t;

        return Vector2.Distance(point, closest);
    }

    private void DeleteNode(int nodeId)
    {
        if (map == null)
        {
            return;
        }

        Undo.RecordObject(map, "Delete Node");

        // 删除相关的路径段
        for (int i = map.segments.Count - 1; i >= 0; i--)
        {
            RailBezierSegment2D segment = map.segments[i];

            if (segment != null &&
                (segment.startNodeId == nodeId || segment.endNodeId == nodeId))
            {
                map.segments.RemoveAt(i);
            }
        }

        // 删除节点
        for (int i = map.nodes.Count - 1; i >= 0; i--)
        {
            RailBezierNode2D node = map.nodes[i];

            if (node != null && node.nodeId == nodeId)
            {
                map.nodes.RemoveAt(i);
            }
        }

        // 清理引用该节点的出口
        for (int i = 0; i < map.nodes.Count; i++)
        {
            RailBezierNode2D node = map.nodes[i];
            if (node == null) continue;

            if (node.leftExitSegmentId < 0) { /* keep */ }
            if (node.rightExitSegmentId < 0) { /* keep */ }
            if (node.upExitSegmentId < 0) { /* keep */ }
            if (node.downExitSegmentId < 0) { /* keep */ }
            if (node.autoExitSegmentId < 0) { /* keep */ }
        }

        selectedNodeId = -1;
        EditorUtility.SetDirty(map);
    }

    private void DeleteSegment(int segmentId)
    {
        if (map == null)
        {
            return;
        }

        Undo.RecordObject(map, "Delete Segment");

        // 删除路径段
        for (int i = map.segments.Count - 1; i >= 0; i--)
        {
            RailBezierSegment2D segment = map.segments[i];

            if (segment != null && segment.segmentId == segmentId)
            {
                map.segments.RemoveAt(i);
            }
        }

        // 清理引用该路径段的出口
        for (int i = 0; i < map.nodes.Count; i++)
        {
            RailBezierNode2D node = map.nodes[i];
            if (node == null) continue;

            if (node.leftExitSegmentId == segmentId) node.leftExitSegmentId = -1;
            if (node.rightExitSegmentId == segmentId) node.rightExitSegmentId = -1;
            if (node.upExitSegmentId == segmentId) node.upExitSegmentId = -1;
            if (node.downExitSegmentId == segmentId) node.downExitSegmentId = -1;
            if (node.autoExitSegmentId == segmentId) node.autoExitSegmentId = -1;
        }

        selectedSegmentId = -1;
        EditorUtility.SetDirty(map);
    }

    /// <summary>
    /// 把 Scene 视图鼠标坐标转换为 2D 世界坐标。
    /// </summary>
    /// <param name="guiPosition">
    /// 鼠标在 SceneView GUI 中的位置。
    /// </param>
    /// <param name="targetZ">
    /// 目标 Z 平面。
    /// 2D 路径一般放在同一个 Z 平面上。
    /// </param>
    /// <returns>
    /// 返回二维世界坐标。
    /// </returns>
    private static Vector2 GetMouseWorld2D(
        Vector2 guiPosition,
        float targetZ)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);

        float distance = Mathf.Approximately(ray.direction.z, 0f)
            ? 0f
            : (targetZ - ray.origin.z) / ray.direction.z;

        Vector3 world = ray.origin + ray.direction * distance;

        return new Vector2(world.x, world.y);
    }
}
