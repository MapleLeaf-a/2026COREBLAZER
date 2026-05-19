using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas Image 转世界 SpriteRenderer 工具。
///
/// 适用场景：
/// 当前地图背景、前景、道具等还放在 Canvas / UI Image 中，
/// 但角色使用 SpriteRenderer / Rigidbody2D / RailWalker2D 走世界坐标。
///
/// 工具目标：
/// 1. 扫描选中的 Canvas 或 RectTransform 子树。
/// 2. 找出所有带 Sprite 的 UnityEngine.UI.Image。
/// 3. 为每个 Image 创建一个世界空间 GameObject。
/// 4. 用 SpriteRenderer 显示原 Image 的 Sprite。
/// 5. 按指定 Pixels Per Unit 把 UI 坐标转换成世界坐标。
///
/// 注意：
/// 这个工具不会删除原 Canvas 对象。
/// 建议确认转换结果正确后，再手动禁用或删除原 Canvas 地图 Image。
/// </summary>
public sealed class CanvasImageToSpriteRendererConverterWindow : EditorWindow
{
	/// <summary>
	/// UI 坐标原点模式。
	/// 它决定 UI 坐标中的哪个点会映射到世界坐标 worldOrigin。
	/// </summary>
	private enum OriginMode
	{
		/// <summary>
		/// 使用选中 Source Root 的 RectTransform 中心作为 UI 原点。
		/// 适合选中一个完整地图 Image 或地图根节点时使用。
		/// </summary>
		SourceRootCenter = 0,

		/// <summary>
		/// 使用所在 Canvas 的 RectTransform 中心作为 UI 原点。
		/// 适合整个 Canvas 中的地图元素都围绕 Canvas 中心摆放时使用。
		/// </summary>
		CanvasCenter = 1,

		/// <summary>
		/// 使用手动填写的 UI 原点。
		/// 适合你已经知道原 UI 坐标中的某个点应该对应世界原点时使用。
		/// </summary>
		Manual = 2
	}

	/// <summary>
	/// 待转换的根对象。
	/// 可以是 Canvas，也可以是 Canvas 下的某个地图父节点。
	/// 工具会扫描它下面所有 Image。
	/// </summary>
	[SerializeField]
	private GameObject sourceRoot;

	/// <summary>
	/// 转换结果父节点。
	/// 如果为空，工具会自动创建一个新的 WorldRoot。
	/// </summary>
	[SerializeField]
	private Transform outputParent;

	/// <summary>
	/// 生成的根对象名称。
	/// </summary>
	[SerializeField]
	private string outputRootName = "Converted_WorldMap";

	/// <summary>
	/// UI 像素到世界单位的比例。
	///
	/// 100 表示：
	/// UI 中 100 像素 = 世界中 1 个单位。
	///
	/// 例如 UI 坐标 x = 924，使用 100 后，
	/// 世界坐标约为 x = 9.24。
	/// </summary>
	[SerializeField]
	private float pixelsPerUnit = 100f;

	/// <summary>
	/// UI 原点模式。
	/// SourceRootCenter 最适合一键把一张地图图转成世界地图。
	/// </summary>
	[SerializeField]
	private OriginMode originMode = OriginMode.SourceRootCenter;

	/// <summary>
	/// 手动 UI 原点。
	/// 只有 originMode = Manual 时生效。
	/// </summary>
	[SerializeField]
	private Vector2 manualUiOrigin = Vector2.zero;

	/// <summary>
	/// 转换后的世界原点。
	/// UI 原点会被放到这个世界坐标。
	/// 一般保持 Vector2.zero 即可。
	/// </summary>
	[SerializeField]
	private Vector2 worldOrigin = Vector2.zero;

	/// <summary>
	/// 生成 SpriteRenderer 时使用的 Sorting Layer 名称。
	/// 如果项目没有这个 Sorting Layer，可以先填 Default。
	/// </summary>
	[SerializeField]
	private string sortingLayerName = "Default";

	/// <summary>
	/// 第一个 SpriteRenderer 的 Order in Layer。
	/// 数字越大，越靠前显示。
	/// </summary>
	[SerializeField]
	private int startSortingOrder = 0;

	/// <summary>
	/// 每转换一个 Image 后，Sorting Order 增加多少。
	/// 设为 1 时，后扫描到的图片会显示在更前面。
	/// </summary>
	[SerializeField]
	private int sortingOrderStep = 1;

	/// <summary>
	/// 生成对象时使用的 Z 起点。
	/// 2D 项目一般保持 0。
	/// </summary>
	[SerializeField]
	private float startZ = 0f;

	/// <summary>
	/// 每转换一个 Image 后，Z 轴偏移多少。
	/// 如果完全依赖 Sorting Layer，可以保持 0。
	/// </summary>
	[SerializeField]
	private float zStep = 0f;

	/// <summary>
	/// 是否包含未激活对象。
	/// 如果地图层中有暂时隐藏的 Image，也想一起转换，就打开。
	/// </summary>
	[SerializeField]
	private bool includeInactive = true;

	/// <summary>
	/// 是否跳过透明度为 0 的 Image。
	/// 透明 Image 常被用作占位或遮罩，通常不需要转成世界 Sprite。
	/// </summary>
	[SerializeField]
	private bool skipFullyTransparentImages = true;

	/// <summary>
	/// 是否复制 Image 的颜色到 SpriteRenderer。
	/// 这会保留透明度和染色效果。
	/// </summary>
	[SerializeField]
	private bool copyColor = true;

	/// <summary>
	/// 是否把转换后的对象设为 Static。
	/// 地图背景通常不动，可以设为 Static。
	/// </summary>
	[SerializeField]
	private bool markStatic = false;

	/// <summary>
	/// 是否在转换完成后禁用原 UI Image。
	/// 第一轮建议关闭，确认结果正确后再手动处理。
	/// </summary>
	[SerializeField]
	private bool disableSourceImagesAfterConvert = false;

	[MenuItem("Tools/COREBLAZER/Canvas Image To SpriteRenderer Converter")]
	private static void OpenWindow()
	{
		CanvasImageToSpriteRendererConverterWindow window =
			GetWindow<CanvasImageToSpriteRendererConverterWindow>();

		window.titleContent = new GUIContent("Canvas To Sprite");
		window.minSize = new Vector2(420f, 520f);

		if (Selection.activeGameObject != null)
		{
			window.sourceRoot = Selection.activeGameObject;
		}

		window.Show();
	}

	[MenuItem("Tools/COREBLAZER/Convert Selected Canvas Images To SpriteRenderers")]
	private static void ConvertSelectedWithDefaultSettings()
	{
		GameObject selected = Selection.activeGameObject;

		if (selected == null)
		{
			EditorUtility.DisplayDialog(
				"No Selection",
				"请先在 Hierarchy 中选中一个 Canvas、地图父节点，或带 Image 的对象。",
				"OK");

			return;
		}

		ConvertSettings settings = new ConvertSettings
		{
			sourceRoot = selected,
			outputParent = null,
			outputRootName = selected.name + "_WorldSprites",
			pixelsPerUnit = 100f,
			originMode = OriginMode.SourceRootCenter,
			manualUiOrigin = Vector2.zero,
			worldOrigin = Vector2.zero,
			sortingLayerName = "Default",
			startSortingOrder = 0,
			sortingOrderStep = 1,
			startZ = 0f,
			zStep = 0f,
			includeInactive = true,
			skipFullyTransparentImages = true,
			copyColor = true,
			markStatic = false,
			disableSourceImagesAfterConvert = false
		};

		ConvertCanvasImagesToSprites(settings);
	}

	private void OnGUI()
	{
		EditorGUILayout.LabelField("Canvas Image To SpriteRenderer Converter", EditorStyles.boldLabel);

		EditorGUILayout.Space(6f);

		sourceRoot = (GameObject)EditorGUILayout.ObjectField(
			"Source Root",
			sourceRoot,
			typeof(GameObject),
			true);

		outputParent = (Transform)EditorGUILayout.ObjectField(
			"Output Parent",
			outputParent,
			typeof(Transform),
			true);

		outputRootName = EditorGUILayout.TextField(
			"Output Root Name",
			outputRootName);

		EditorGUILayout.Space(8f);
		EditorGUILayout.LabelField("Coordinate", EditorStyles.boldLabel);

		pixelsPerUnit = EditorGUILayout.FloatField(
			"Pixels Per Unit",
			pixelsPerUnit);

		originMode = (OriginMode)EditorGUILayout.EnumPopup(
			"Origin Mode",
			originMode);

		if (originMode == OriginMode.Manual)
		{
			manualUiOrigin = EditorGUILayout.Vector2Field(
				"Manual UI Origin",
				manualUiOrigin);
		}

		worldOrigin = EditorGUILayout.Vector2Field(
			"World Origin",
			worldOrigin);

		EditorGUILayout.Space(8f);
		EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);

		sortingLayerName = EditorGUILayout.TextField(
			"Sorting Layer",
			sortingLayerName);

		startSortingOrder = EditorGUILayout.IntField(
			"Start Sorting Order",
			startSortingOrder);

		sortingOrderStep = EditorGUILayout.IntField(
			"Sorting Order Step",
			sortingOrderStep);

		startZ = EditorGUILayout.FloatField(
			"Start Z",
			startZ);

		zStep = EditorGUILayout.FloatField(
			"Z Step",
			zStep);

		EditorGUILayout.Space(8f);
		EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

		includeInactive = EditorGUILayout.Toggle(
			"Include Inactive",
			includeInactive);

		skipFullyTransparentImages = EditorGUILayout.Toggle(
			"Skip Alpha 0 Images",
			skipFullyTransparentImages);

		copyColor = EditorGUILayout.Toggle(
			"Copy Image Color",
			copyColor);

		markStatic = EditorGUILayout.Toggle(
			"Mark Static",
			markStatic);

		disableSourceImagesAfterConvert = EditorGUILayout.Toggle(
			"Disable Source Images",
			disableSourceImagesAfterConvert);

		EditorGUILayout.Space(10f);

		EditorGUILayout.HelpBox(
			"推荐用法：\n" +
			"1. 选中 Canvas 中的地图父节点或背景 Image。\n" +
			"2. Pixels Per Unit 填 100。\n" +
			"3. Origin Mode 先用 SourceRootCenter。\n" +
			"4. Convert 后把生成对象放到 WorldRoot / MapRoot 下。\n\n" +
			"注意：这个工具只转换 UI Image，不转换 Text、Button 逻辑、Slider 逻辑或 TMP。",
			MessageType.Info);

		EditorGUI.BeginDisabledGroup(sourceRoot == null || pixelsPerUnit <= 0f);

		if (GUILayout.Button("Convert Canvas Images To SpriteRenderers", GUILayout.Height(36f)))
		{
			ConvertSettings settings = new ConvertSettings
			{
				sourceRoot = sourceRoot,
				outputParent = outputParent,
				outputRootName = outputRootName,
				pixelsPerUnit = pixelsPerUnit,
				originMode = originMode,
				manualUiOrigin = manualUiOrigin,
				worldOrigin = worldOrigin,
				sortingLayerName = sortingLayerName,
				startSortingOrder = startSortingOrder,
				sortingOrderStep = sortingOrderStep,
				startZ = startZ,
				zStep = zStep,
				includeInactive = includeInactive,
				skipFullyTransparentImages = skipFullyTransparentImages,
				copyColor = copyColor,
				markStatic = markStatic,
				disableSourceImagesAfterConvert = disableSourceImagesAfterConvert
			};

			ConvertCanvasImagesToSprites(settings);
		}

		EditorGUI.EndDisabledGroup();
	}

	/// <summary>
	/// 转换参数集合。
	/// 用结构体集中保存参数，避免 ConvertCanvasImagesToSprites 参数列表过长。
	/// </summary>
	private struct ConvertSettings
	{
		public GameObject sourceRoot;
		public Transform outputParent;
		public string outputRootName;
		public float pixelsPerUnit;
		public OriginMode originMode;
		public Vector2 manualUiOrigin;
		public Vector2 worldOrigin;
		public string sortingLayerName;
		public int startSortingOrder;
		public int sortingOrderStep;
		public float startZ;
		public float zStep;
		public bool includeInactive;
		public bool skipFullyTransparentImages;
		public bool copyColor;
		public bool markStatic;
		public bool disableSourceImagesAfterConvert;
	}

	/// <summary>
	/// 执行 Canvas Image 到 SpriteRenderer 的转换。
	/// </summary>
	/// <param name="settings">
	/// 转换参数。
	/// 包含源对象、输出位置、坐标比例、排序层级和转换选项。
	/// </param>
	private static void ConvertCanvasImagesToSprites(ConvertSettings settings)
	{
		if (settings.sourceRoot == null)
		{
			return;
		}

		if (settings.pixelsPerUnit <= 0f)
		{
			EditorUtility.DisplayDialog(
				"Invalid Pixels Per Unit",
				"Pixels Per Unit 必须大于 0。",
				"OK");

			return;
		}

		RectTransform sourceRect = settings.sourceRoot.GetComponent<RectTransform>();

		if (sourceRect == null)
		{
			EditorUtility.DisplayDialog(
				"Invalid Source Root",
				"Source Root 必须是 Canvas 或 Canvas 下的 UI 对象，也就是需要 RectTransform。",
				"OK");

			return;
		}

		Image[] images = settings.sourceRoot.GetComponentsInChildren<Image>(
			settings.includeInactive);

		if (images == null || images.Length == 0)
		{
			EditorUtility.DisplayDialog(
				"No Images",
				"Source Root 下没有找到 Image 组件。",
				"OK");

			return;
		}

		Vector2 uiOrigin = ResolveUiOrigin(
			settings,
			sourceRect);

		GameObject outputRoot = new GameObject(
			string.IsNullOrWhiteSpace(settings.outputRootName)
				? settings.sourceRoot.name + "_WorldSprites"
				: settings.outputRootName);

		Undo.RegisterCreatedObjectUndo(
			outputRoot,
			"Create Converted World Sprites Root");

		if (settings.outputParent != null)
		{
			outputRoot.transform.SetParent(
				settings.outputParent,
				false);
		}

		outputRoot.transform.position = Vector3.zero;
		outputRoot.transform.rotation = Quaternion.identity;
		outputRoot.transform.localScale = Vector3.one;

		int convertedCount = 0;
		List<string> warnings = new List<string>();

		for (int i = 0; i < images.Length; i++)
		{
			Image image = images[i];

			if (!CanConvertImage(
					image,
					settings,
					warnings))
			{
				continue;
			}

			ConvertSingleImage(
				image,
				outputRoot.transform,
				settings,
				uiOrigin,
				convertedCount,
				warnings);

			convertedCount++;
		}

		Selection.activeGameObject = outputRoot;

		string warningText = warnings.Count == 0
			? string.Empty
			: "\n\nWarnings:\n" + string.Join("\n", warnings);

		EditorUtility.DisplayDialog(
			"Convert Finished",
			$"转换完成。\nConverted Images: {convertedCount}{warningText}",
			"OK");
	}

	/// <summary>
	/// 判断某个 Image 是否可以转换。
	/// </summary>
	/// <param name="image">
	/// 待检查的 UI Image。
	/// </param>
	/// <param name="settings">
	/// 转换设置。
	/// skipFullyTransparentImages 会影响透明图片是否被跳过。
	/// </param>
	/// <param name="warnings">
	/// 警告收集列表。
	/// 用于记录跳过原因或不完全支持的情况。
	/// </param>
	/// <returns>
	/// true 表示可以转换。
	/// false 表示应该跳过。
	/// </returns>
	private static bool CanConvertImage(
		Image image,
		ConvertSettings settings,
		List<string> warnings)
	{
		if (image == null)
		{
			return false;
		}

		if (image.sprite == null)
		{
			warnings.Add($"{image.name}: 没有 Source Image，已跳过。");
			return false;
		}

		if (settings.skipFullyTransparentImages && image.color.a <= 0.001f)
		{
			warnings.Add($"{image.name}: Alpha 为 0，已跳过。");
			return false;
		}

		RectTransform rectTransform = image.rectTransform;

		if (rectTransform == null)
		{
			warnings.Add($"{image.name}: 没有 RectTransform，已跳过。");
			return false;
		}

		if (rectTransform.rect.width <= 0f || rectTransform.rect.height <= 0f)
		{
			warnings.Add($"{image.name}: Rect 尺寸无效，已跳过。");
			return false;
		}

		if (image.type == Image.Type.Filled)
		{
			warnings.Add($"{image.name}: Image.Type.Filled 不能被 SpriteRenderer 完整还原，将按 Simple 转换。");
		}

		return true;
	}

	/// <summary>
	/// 转换单个 Image。
	/// </summary>
	/// <param name="image">
	/// 源 UI Image。
	/// </param>
	/// <param name="outputRoot">
	/// 转换结果父节点。
	/// </param>
	/// <param name="settings">
	/// 转换设置。
	/// 包含 pixelsPerUnit、worldOrigin、Sorting 等参数。
	/// </param>
	/// <param name="uiOrigin">
	/// 已计算出的 UI 原点。
	/// </param>
	/// <param name="convertIndex">
	/// 当前转换序号。
	/// 用于计算 Sorting Order 和 Z 偏移。
	/// </param>
	/// <param name="warnings">
	/// 警告列表。
	/// 用于记录不完全支持的 Image 类型。
	/// </param>
	private static void ConvertSingleImage(
		Image image,
		Transform outputRoot,
		ConvertSettings settings,
		Vector2 uiOrigin,
		int convertIndex,
		List<string> warnings)
	{
		RectTransform rectTransform = image.rectTransform;

		Vector3[] corners = new Vector3[4];

		// GetWorldCorners 返回 UI 矩形四个角的世界坐标。
		// 对 Screen Space Overlay Canvas 来说，这些坐标通常等价于屏幕 / UI 坐标尺度。
		rectTransform.GetWorldCorners(corners);

		Vector2 uiCenter = new Vector2(
			(corners[0].x + corners[2].x) * 0.5f,
			(corners[0].y + corners[2].y) * 0.5f);

		float uiWidth = Vector3.Distance(
			corners[0],
			corners[3]);

		float uiHeight = Vector3.Distance(
			corners[0],
			corners[1]);

		Vector2 worldCenter = UiPointToWorldPoint(
			uiCenter,
			uiOrigin,
			settings.pixelsPerUnit,
			settings.worldOrigin);

		Vector2 desiredWorldSize = new Vector2(
			uiWidth / settings.pixelsPerUnit,
			uiHeight / settings.pixelsPerUnit);

		GameObject spriteObject = new GameObject(
			image.gameObject.name + "_Sprite");

		Undo.RegisterCreatedObjectUndo(
			spriteObject,
			"Create Converted Sprite");

		spriteObject.transform.SetParent(
			outputRoot,
			true);

		spriteObject.transform.position = new Vector3(
			worldCenter.x,
			worldCenter.y,
			settings.startZ + convertIndex * settings.zStep);

		spriteObject.transform.rotation = Quaternion.identity;
		spriteObject.transform.localScale = Vector3.one;
		spriteObject.isStatic = settings.markStatic;

		SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = image.sprite;
		spriteRenderer.sortingLayerName = settings.sortingLayerName;
		spriteRenderer.sortingOrder = settings.startSortingOrder + convertIndex * settings.sortingOrderStep;

		if (settings.copyColor)
		{
			spriteRenderer.color = image.color;
		}

		ApplySpriteSize(
			spriteRenderer,
			image,
			desiredWorldSize,
			warnings);

		if (settings.disableSourceImagesAfterConvert)
		{
			Undo.RecordObject(
				image,
				"Disable Source Image After Convert");

			image.enabled = false;
			EditorUtility.SetDirty(image);
		}
	}

	/// <summary>
	/// 应用 SpriteRenderer 的显示尺寸。
	/// </summary>
	/// <param name="spriteRenderer">
	/// 新建的 SpriteRenderer。
	/// </param>
	/// <param name="sourceImage">
	/// 源 UI Image。
	/// 用于读取 Image.Type。
	/// </param>
	/// <param name="desiredWorldSize">
	/// 目标世界尺寸。
	/// X 是世界宽度，Y 是世界高度。
	/// </param>
	/// <param name="warnings">
	/// 警告列表。
	/// 用于记录 Sliced / Tiled 可能无法完全匹配的情况。
	/// </param>
	private static void ApplySpriteSize(
		SpriteRenderer spriteRenderer,
		Image sourceImage,
		Vector2 desiredWorldSize,
		List<string> warnings)
	{
		if (spriteRenderer == null || spriteRenderer.sprite == null)
		{
			return;
		}

		if (sourceImage.type == Image.Type.Sliced)
		{
			spriteRenderer.drawMode = SpriteDrawMode.Sliced;
			spriteRenderer.size = desiredWorldSize;
			warnings.Add($"{sourceImage.name}: Sliced 已转为 SpriteRenderer.Sliced，请检查九宫格边缘是否正确。");
			return;
		}

		if (sourceImage.type == Image.Type.Tiled)
		{
			spriteRenderer.drawMode = SpriteDrawMode.Tiled;
			spriteRenderer.size = desiredWorldSize;
			warnings.Add($"{sourceImage.name}: Tiled 已转为 SpriteRenderer.Tiled，请检查平铺效果。");
			return;
		}

		spriteRenderer.drawMode = SpriteDrawMode.Simple;

		Vector3 spriteSizeAtScaleOne = spriteRenderer.sprite.bounds.size;

		float scaleX = spriteSizeAtScaleOne.x <= Mathf.Epsilon
			? 1f
			: desiredWorldSize.x / spriteSizeAtScaleOne.x;

		float scaleY = spriteSizeAtScaleOne.y <= Mathf.Epsilon
			? 1f
			: desiredWorldSize.y / spriteSizeAtScaleOne.y;

		spriteRenderer.transform.localScale = new Vector3(
			scaleX,
			scaleY,
			1f);
	}

	/// <summary>
	/// 根据设置计算 UI 原点。
	/// </summary>
	/// <param name="settings">
	/// 转换设置。
	/// originMode 决定使用哪种原点计算方式。
	/// </param>
	/// <param name="sourceRect">
	/// Source Root 上的 RectTransform。
	/// </param>
	/// <returns>
	/// 返回 UI 坐标中的原点。
	/// 这个点会映射到 settings.worldOrigin。
	/// </returns>
	private static Vector2 ResolveUiOrigin(
		ConvertSettings settings,
		RectTransform sourceRect)
	{
		if (settings.originMode == OriginMode.Manual)
		{
			return settings.manualUiOrigin;
		}

		if (settings.originMode == OriginMode.SourceRootCenter)
		{
			return GetRectTransformWorldCenter2D(sourceRect);
		}

		Canvas canvas = sourceRect.GetComponentInParent<Canvas>();

		if (canvas != null)
		{
			RectTransform canvasRect = canvas.GetComponent<RectTransform>();

			if (canvasRect != null)
			{
				return GetRectTransformWorldCenter2D(canvasRect);
			}
		}

		return GetRectTransformWorldCenter2D(sourceRect);
	}

	/// <summary>
	/// 获取 RectTransform 的世界中心点。
	/// </summary>
	/// <param name="rectTransform">
	/// 目标 RectTransform。
	/// </param>
	/// <returns>
	/// 返回二维世界中心点。
	/// </returns>
	private static Vector2 GetRectTransformWorldCenter2D(RectTransform rectTransform)
	{
		if (rectTransform == null)
		{
			return Vector2.zero;
		}

		Vector3[] corners = new Vector3[4];
		rectTransform.GetWorldCorners(corners);

		return new Vector2(
			(corners[0].x + corners[2].x) * 0.5f,
			(corners[0].y + corners[2].y) * 0.5f);
	}

	/// <summary>
	/// UI 点转换为世界点。
	/// </summary>
	/// <param name="uiPoint">
	/// UI 坐标点。
	/// </param>
	/// <param name="uiOrigin">
	/// UI 原点。
	/// uiOrigin 会映射到 worldOrigin。
	/// </param>
	/// <param name="pixelsPerUnitValue">
	/// UI 像素到世界单位的换算比例。
	/// </param>
	/// <param name="worldOriginValue">
	/// 世界原点。
	/// </param>
	/// <returns>
	/// 返回转换后的世界二维坐标。
	/// </returns>
	private static Vector2 UiPointToWorldPoint(
		Vector2 uiPoint,
		Vector2 uiOrigin,
		float pixelsPerUnitValue,
		Vector2 worldOriginValue)
	{
		return (uiPoint - uiOrigin) / pixelsPerUnitValue + worldOriginValue;
	}
}