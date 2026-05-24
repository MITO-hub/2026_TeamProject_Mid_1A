using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class InGameUIPrototypeSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/InGameUIPrototype.unity";
    private const string DexDatabasePath = "Assets/Data/DogDexDatabase.asset";
    private const string SlotPrefabPath = "Assets/Prefabs/DogDexSlot.prefab";
    private const string UiImageFolder = "Assets/Art/UI_References/20260522_DriveDownload";
    private const string GeneratedArtFolder = "Assets/Art/Generated";
    private const string RoomBackgroundPath = GeneratedArtFolder + "/ingame_room_background.png";

    [MenuItem("Tools/In Game UI/Build Prototype Scene")]
    public static void Build()
    {
        EnsureUiReferenceSprites();
        EnsureGeneratedArtFolder();
        Sprite roomBackground = CreateRoomBackgroundSprite();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "InGameUIPrototype";

        CreateCamera();
        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        DogDexDatabase database = AssetDatabase.LoadAssetAtPath<DogDexDatabase>(DexDatabasePath);
        DogDexSlotView slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath)?.GetComponent<DogDexSlotView>();
        DogDexManager manager = CreateDexManager(database);

        InGameUIController controller = new GameObject("InGameUIController").AddComponent<InGameUIController>();
        CreateMainScreen(canvas.transform, controller, roomBackground);
        DogDexView dexView = CreateDogDexPanel(canvas.transform, controller, manager, slotPrefab, database);
        GameObject settingsPanel = CreateSettingsPanel(canvas.transform, controller);

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        serializedController.FindProperty("dogDexPanel").objectReferenceValue = dexView.transform.parent.gameObject;
        serializedController.FindProperty("dogDexView").objectReferenceValue = dexView;
        serializedController.FindProperty("dogDexManager").objectReferenceValue = manager;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        settingsPanel.SetActive(false);
        dexView.transform.parent.gameObject.SetActive(false);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureUiReferenceSprites()
    {
        string[] guids =
        {
            "d03fe3da216e5ab4085447d4394acae6",
            "97789076acd861241b3ef75946a23570",
            "e85580a65501dca46b8365853bc5acac",
            "bb08aa6cc2ae28d45bfce4b30262b928",
            "c617a0f3628fba6439671d23fecbbc01",
            "7f3744dd4dfc8a4408ad9b1b2df4fde3",
            "4707fdb48b82a7746916cf21150c504d",
            "396f23740dde84146a1cb999a4351513"
        };

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("UI reference image missing: " + guids[i]);
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }

    private static DogDexManager CreateDexManager(DogDexDatabase database)
    {
        GameObject managerObject = new GameObject("DogDexManager");
        DogDexManager manager = managerObject.AddComponent<DogDexManager>();

        SerializedObject serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("database").objectReferenceValue = database;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        return manager;
    }

    private static void EnsureGeneratedArtFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Art"))
        {
            AssetDatabase.CreateFolder("Assets", "Art");
        }

        if (!AssetDatabase.IsValidFolder(GeneratedArtFolder))
        {
            AssetDatabase.CreateFolder("Assets/Art", "Generated");
        }
    }

    private static void CreateMainScreen(Transform parent, InGameUIController controller, Sprite roomBackground)
    {
        GameObject root = CreateRectObject("InGameUIScreen", Vector2.zero, parent);
        Stretch(root.GetComponent<RectTransform>());

        Image background = root.AddComponent<Image>();
        background.color = new Color(0.95f, 0.95f, 0.95f);

        CreateRoomScene(root.transform, controller, roomBackground);
    }

    private static void CreateRoomScene(Transform parent, InGameUIController controller, Sprite roomBackground)
    {
        GameObject room = CreateRectObject("InGameRoom", new Vector2(1280f, 720f), parent);
        RectTransform roomRect = room.GetComponent<RectTransform>();
        roomRect.anchorMin = new Vector2(0.5f, 0.5f);
        roomRect.anchorMax = new Vector2(0.5f, 0.5f);
        roomRect.anchoredPosition = Vector2.zero;

        Image roomImage = room.AddComponent<Image>();
        roomImage.sprite = roomBackground;
        roomImage.preserveAspect = false;
        roomImage.color = Color.white;

        CreateCurrencyInRoom(room.transform);
        AddImageButton(room.transform, "EscButton", "XButton", new Vector2(44f, 44f), controller.ToggleSettings, new Vector2(555f, 250f), new Vector2(0.5f, 0.5f));
        AddImageButton(room.transform, "BoneGenerateButton", "Create", new Vector2(98f, 56f), controller.GenerateBonePlaceholder, new Vector2(388f, -225f), new Vector2(0.5f, 0.5f));
        AddImageButton(room.transform, "DogDexButton", "Dex", new Vector2(76f, 67f), controller.ToggleDogDex, new Vector2(480f, -225f), new Vector2(0.5f, 0.5f));
        AddImageButton(room.transform, "ShopButton", "Continue", new Vector2(90f, 40f), controller.OpenShopPlaceholder, new Vector2(570f, -225f), new Vector2(0.5f, 0.5f));
    }

    private static void CreatePerspectiveFloorLines(Transform parent)
    {
        Color tileColor = new Color(0.48f, 0.48f, 0.44f, 0.85f);
        Vector2 backCorner = new Vector2(0f, 48f);
        Vector2 leftWallFloor = new Vector2(-440f, -24f);
        Vector2 rightWallFloor = new Vector2(440f, -24f);

        CreateUiLine(parent, "LeftWallFloorEdge", leftWallFloor, backCorner, Color.black, 2f);
        CreateUiLine(parent, "RightWallFloorEdge", backCorner, rightWallFloor, Color.black, 2f);
        CreateUiLine(parent, "FloorCenterDepth", backCorner, new Vector2(0f, -190f), tileColor, 1.2f);

        CreateUiLine(parent, "FloorRow_01", new Vector2(-440f, -74f), new Vector2(440f, -74f), tileColor, 1f);
        CreateUiLine(parent, "FloorRow_02", new Vector2(-440f, -124f), new Vector2(440f, -124f), tileColor, 1f);
        CreateUiLine(parent, "FloorRow_03", new Vector2(-440f, -162f), new Vector2(440f, -162f), tileColor, 1f);

        CreateUiLine(parent, "FloorLeftDepth_01", new Vector2(-330f, -42f), new Vector2(-240f, -190f), tileColor, 1f);
        CreateUiLine(parent, "FloorLeftDepth_02", new Vector2(-205f, -20f), new Vector2(-92f, -190f), tileColor, 1f);
        CreateUiLine(parent, "FloorLeftDepth_03", new Vector2(-86f, 12f), new Vector2(60f, -190f), tileColor, 1f);

        CreateUiLine(parent, "FloorRightDepth_01", new Vector2(86f, 12f), new Vector2(-60f, -190f), tileColor, 1f);
        CreateUiLine(parent, "FloorRightDepth_02", new Vector2(205f, -20f), new Vector2(92f, -190f), tileColor, 1f);
        CreateUiLine(parent, "FloorRightDepth_03", new Vector2(330f, -42f), new Vector2(240f, -190f), tileColor, 1f);
    }

    private static Sprite CreateRoomBackgroundSprite()
    {
        const int width = 1280;
        const int height = 720;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color leftWall = new Color(0.82f, 0.88f, 0.72f, 1f);
        Color rightWall = new Color(0.96f, 0.94f, 0.55f, 1f);
        Color floor = new Color(0.99f, 0.99f, 0.97f, 1f);

        FillTexture(texture, clear);

        DrawPolygon(texture, leftWall,
            ToPixel(new Vector2(-640f, 360f), width, height),
            ToPixel(new Vector2(0f, 360f), width, height),
            ToPixel(new Vector2(0f, 86f), width, height),
            ToPixel(new Vector2(-640f, -48f), width, height));

        DrawPolygon(texture, rightWall,
            ToPixel(new Vector2(0f, 360f), width, height),
            ToPixel(new Vector2(640f, 360f), width, height),
            ToPixel(new Vector2(640f, -48f), width, height),
            ToPixel(new Vector2(0f, 86f), width, height));

        DrawPolygon(texture, floor,
            ToPixel(new Vector2(-640f, -48f), width, height),
            ToPixel(new Vector2(0f, 86f), width, height),
            ToPixel(new Vector2(640f, -48f), width, height),
            ToPixel(new Vector2(640f, -360f), width, height),
            ToPixel(new Vector2(-640f, -360f), width, height));

        texture.Apply();
        System.IO.File.WriteAllBytes(RoomBackgroundPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(RoomBackgroundPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(RoomBackgroundPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(RoomBackgroundPath);
    }

    private static Vector2Int ToPixel(Vector2 point, int width, int height)
    {
        return new Vector2Int(
            Mathf.RoundToInt(point.x + width * 0.5f),
            Mathf.RoundToInt(point.y + height * 0.5f));
    }

    private static void FillTexture(Texture2D texture, Color color)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }

    private static void DrawPolygon(Texture2D texture, Color color, params Vector2Int[] points)
    {
        if (points == null || points.Length < 3)
        {
            return;
        }

        int minX = texture.width - 1;
        int maxX = 0;
        int minY = texture.height - 1;
        int maxY = 0;

        for (int i = 0; i < points.Length; i++)
        {
            minX = Mathf.Min(minX, points[i].x);
            maxX = Mathf.Max(maxX, points[i].x);
            minY = Mathf.Min(minY, points[i].y);
            maxY = Mathf.Max(maxY, points[i].y);
        }

        minX = Mathf.Clamp(minX, 0, texture.width - 1);
        maxX = Mathf.Clamp(maxX, 0, texture.width - 1);
        minY = Mathf.Clamp(minY, 0, texture.height - 1);
        maxY = Mathf.Clamp(maxY, 0, texture.height - 1);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (IsPointInPolygon(new Vector2(x + 0.5f, y + 0.5f), points))
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    private static bool IsPointInPolygon(Vector2 point, Vector2Int[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[j];
            bool intersects = ((a.y > point.y) != (b.y > point.y)) &&
                              (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x);
            if (intersects)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static void CreateCurrencyInRoom(Transform parent)
    {
        GameObject badge = CreateRectObject("CurrencyBadge", new Vector2(172f, 38f), parent);
        RectTransform badgeRect = badge.GetComponent<RectTransform>();
        badgeRect.anchoredPosition = new Vector2(430f, 250f);
        Image badgeImage = badge.AddComponent<Image>();
        badgeImage.color = Color.white;
        badge.AddComponent<Outline>().effectColor = new Color(0.26f, 0.24f, 0.20f);

        Image icon = CreateRectObject("CurrencyIcon", new Vector2(124f, 26f), badge.transform).AddComponent<Image>();
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(70f, 0f);
        icon.sprite = LoadSprite("Currency");
        icon.preserveAspect = true;

        Text amount = CreateText("Amount", badge.transform, "0", 20, TextAnchor.MiddleLeft);
        RectTransform amountRect = amount.GetComponent<RectTransform>();
        amountRect.anchorMin = new Vector2(0f, 0f);
        amountRect.anchorMax = new Vector2(1f, 1f);
        amountRect.offsetMin = new Vector2(134f, 0f);
        amountRect.offsetMax = new Vector2(-8f, 0f);
        amount.color = new Color(0.12f, 0.12f, 0.10f);
    }

    private static DogDexView CreateDogDexPanel(Transform parent, InGameUIController controller, DogDexManager manager, DogDexSlotView slotPrefab, DogDexDatabase database)
    {
        GameObject panel = CreateRectObject("DogDexPanel", new Vector2(620f, 590f), parent);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-80f, 0f);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.72f, 0.86f, 0.95f, 0.98f);
        panel.AddComponent<Outline>().effectColor = new Color(0.12f, 0.18f, 0.22f);

        Text title = CreateText("Title", panel.transform, "Dog Dex", 30, TextAnchor.MiddleCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -36f);
        titleRect.sizeDelta = new Vector2(260f, 44f);
        title.color = new Color(0.08f, 0.12f, 0.16f);

        AddImageButton(panel.transform, "CloseButton", "XButton", new Vector2(48f, 48f), controller.HideDogDex, new Vector2(274f, -34f), new Vector2(0.5f, 1f));

        ScrollRect scrollRect = CreateScrollView(panel.transform);
        Transform content = scrollRect.content;
        DogDexView viewObject = new GameObject("DogDexView").AddComponent<DogDexView>();
        viewObject.transform.SetParent(panel.transform, false);

        SerializedObject serializedView = new SerializedObject(viewObject);
        serializedView.FindProperty("dexManager").objectReferenceValue = manager;
        serializedView.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
        serializedView.FindProperty("slotParent").objectReferenceValue = content;
        serializedView.FindProperty("showAllEntries").boolValue = false;
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        if (database != null && slotPrefab != null)
        {
            for (int i = 0; i < database.Entries.Count; i++)
            {
                GameObject slotObject = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab.gameObject, content);
                DogDexSlotView slot = slotObject.GetComponent<DogDexSlotView>();
                slot.name = "DogDexSlot_" + (i + 1).ToString("00");
                slot.SetEntry(database.Entries[i], false);
            }
        }

        return viewObject;
    }

    private static ScrollRect CreateScrollView(Transform parent)
    {
        GameObject scrollObject = CreateRectObject("DogDexScrollView", new Vector2(540f, 462f), parent);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchoredPosition = new Vector2(0f, -44f);
        Image scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(1f, 1f, 1f, 0.42f);

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 28f;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;

        GameObject viewport = CreateRectObject("Viewport", Vector2.zero, scrollObject.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect);
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.03f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        GameObject content = CreateRectObject("Content", new Vector2(500f, 0f), viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 1f);
        contentRect.anchorMax = new Vector2(0.5f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = new Vector2(0f, -14f);

        GridLayoutGroup layout = content.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(145f, 158f);
        layout.spacing = new Vector2(16f, 16f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.verticalScrollbar = CreateVerticalScrollbar(scrollObject.transform);
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarSpacing = 6f;
        return scrollRect;
    }

    private static Scrollbar CreateVerticalScrollbar(Transform parent)
    {
        GameObject scrollbarObject = CreateRectObject("VerticalScrollbar", new Vector2(18f, 462f), parent);
        RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);
        scrollbarRect.pivot = new Vector2(1f, 0.5f);
        scrollbarRect.offsetMin = new Vector2(-18f, 0f);
        scrollbarRect.offsetMax = Vector2.zero;

        Image background = scrollbarObject.AddComponent<Image>();
        background.color = new Color(0.78f, 0.86f, 0.90f, 0.85f);

        GameObject handleArea = CreateRectObject("SlidingArea", Vector2.zero, scrollbarObject.transform);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        Stretch(handleAreaRect);
        handleAreaRect.offsetMin = new Vector2(3f, 3f);
        handleAreaRect.offsetMax = new Vector2(-3f, -3f);

        GameObject handle = CreateRectObject("Handle", Vector2.zero, handleArea.transform);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        Stretch(handleRect);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.26f, 0.42f, 0.52f, 0.95f);

        Scrollbar scrollbar = scrollbarObject.AddComponent<Scrollbar>();
        scrollbar.targetGraphic = handleImage;
        scrollbar.handleRect = handleRect;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        return scrollbar;
    }

    private static GameObject CreateSettingsPanel(Transform parent, InGameUIController controller)
    {
        GameObject panel = CreateRectObject("SettingsPanel", new Vector2(430f, 310f), parent);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-80f, 20f);
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.96f, 0.96f, 0.92f, 0.98f);
        panel.AddComponent<Outline>().effectColor = new Color(0.18f, 0.18f, 0.16f);

        Text title = CreateText("Title", panel.transform, "Settings", 30, TextAnchor.MiddleCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -46f);
        titleRect.sizeDelta = new Vector2(220f, 48f);
        title.color = new Color(0.12f, 0.12f, 0.10f);

        CreateTextLine(panel.transform, "BGM", new Vector2(0f, 34f));
        CreateTextLine(panel.transform, "SFX", new Vector2(0f, -34f));
        AddImageButton(panel.transform, "CloseButton", "XButton", new Vector2(48f, 48f), controller.HideSettings, new Vector2(184f, -34f), new Vector2(0.5f, 1f));
        return panel;
    }

    private static void CreateTextLine(Transform parent, string label, Vector2 position)
    {
        Text text = CreateText(label, parent, label + "  -  On", 24, TextAnchor.MiddleCenter);
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(260f, 42f);
        text.color = new Color(0.14f, 0.14f, 0.12f);
    }

    private static Button AddImageButton(Transform parent, string name, string spriteName, Vector2 size, UnityAction action)
    {
        return AddImageButton(parent, name, spriteName, size, action, Vector2.zero, new Vector2(0.5f, 0.5f));
    }

    private static Button AddImageButton(Transform parent, string name, string spriteName, Vector2 size, UnityAction action, Vector2 anchoredPosition, Vector2 anchor)
    {
        GameObject buttonObject = CreateRectObject(name, size, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = LoadSprite(spriteName);
        image.preserveAspect = true;
        image.color = image.sprite == null ? new Color(0.86f, 0.86f, 0.80f) : Color.white;

        Button button = buttonObject.AddComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }

    private static Button CreatePlainButton(Transform parent, string name, string label, Vector2 size, UnityAction action)
    {
        return CreatePlainButton(parent, name, label, size, action, Vector2.zero, new Vector2(0.5f, 0.5f));
    }

    private static Button CreatePlainButton(Transform parent, string name, string label, Vector2 size, UnityAction action, Vector2 anchoredPosition, Vector2 anchor)
    {
        GameObject buttonObject = CreateRectObject(name, size, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.98f, 0.93f, 0.78f, 0.96f);
        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.22f, 0.18f, 0.12f);
        outline.effectDistance = new Vector2(2f, -2f);

        Button button = buttonObject.AddComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, action);

        Text text = CreateText("Text", buttonObject.transform, label, 24, TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        Stretch(textRect);
        text.color = new Color(0.12f, 0.10f, 0.08f);
        text.fontStyle = FontStyle.Bold;
        return button;
    }

    private static Image CreateUiLine(Transform parent, string name, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 delta = end - start;
        GameObject lineObject = CreateRectObject(name, new Vector2(delta.magnitude, thickness), parent);
        RectTransform rect = lineObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = (start + end) * 0.5f;
        rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

        Image image = lineObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static Button AddLabeledImageButton(Transform parent, string name, string spriteName, string label, Vector2 size, UnityAction action)
    {
        return AddLabeledImageButton(parent, name, spriteName, label, size, action, Vector2.zero, new Vector2(0.5f, 0.5f));
    }

    private static Button AddLabeledImageButton(Transform parent, string name, string spriteName, string label, Vector2 size, UnityAction action, Vector2 anchoredPosition, Vector2 anchor)
    {
        Button button = AddImageButton(parent, name, spriteName, size, action, anchoredPosition, anchor);
        Text text = CreateText("Label", button.transform, label, 20, TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        Stretch(textRect);
        text.color = new Color(0.11f, 0.10f, 0.08f);
        text.fontStyle = FontStyle.Bold;
        return button;
    }

    private static Sprite LoadSprite(string name)
    {
        string guid = GetUiSpriteGuid(name);
        if (!string.IsNullOrEmpty(guid))
        {
            string guidPath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite guidSprite = AssetDatabase.LoadAssetAtPath<Sprite>(guidPath);
            if (guidSprite != null)
            {
                return guidSprite;
            }
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiImageFolder + "/" + name + ".png");
        if (sprite != null)
        {
            return sprite;
        }

        string[] guids = AssetDatabase.FindAssets(name + " t:Sprite", new[] { UiImageFolder });
        if (guids.Length == 0)
        {
            guids = AssetDatabase.FindAssets(name, new[] { UiImageFolder });
        }

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                return sprite;
            }
        }

        Debug.LogWarning("Sprite not found: " + name);
        return null;
    }

    private static string GetUiSpriteGuid(string name)
    {
        switch (name)
        {
            case "Start":
            case "시작":
                return "d03fe3da216e5ab4085447d4394acae6";
            case "Continue":
            case "계속하기":
                return "97789076acd861241b3ef75946a23570";
            case "Settings":
            case "설정 (세팅(esc))":
                return "e85580a65501dca46b8365853bc5acac";
            case "Exit":
            case "나가기":
                return "bb08aa6cc2ae28d45bfce4b30262b928";
            case "Dex":
            case "도감":
                return "c617a0f3628fba6439671d23fecbbc01";
            case "Create":
            case "생성":
                return "7f3744dd4dfc8a4408ad9b1b2df4fde3";
            case "Currency":
            case "재화":
                return "4707fdb48b82a7746916cf21150c504d";
            case "XButton":
            case "X버튼":
                return "396f23740dde84146a1cb999a4351513";
            default:
                return string.Empty;
        }
    }

    private static Sprite LoadPrototypeSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/" + name + ".png");
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("InGameUICanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.96f, 0.96f, 0.93f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static Text CreateText(string name, Transform parent, string value, int fontSize, TextAnchor anchor)
    {
        GameObject textObject = CreateRectObject(name, new Vector2(160f, 32f), parent);
        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        return text;
    }

    private static GameObject CreateRectObject(string name, Vector2 size, Transform parent = null)
    {
        GameObject gameObject = new GameObject(name);
        RectTransform rect = gameObject.AddComponent<RectTransform>();
        rect.sizeDelta = size;

        if (parent != null)
        {
            gameObject.transform.SetParent(parent, false);
        }

        return gameObject;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
