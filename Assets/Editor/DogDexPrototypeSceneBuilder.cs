using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DogDexPrototypeSceneBuilder
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string DataFolder = "Assets/Data";
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string ArtFolder = "Assets/Art";
    private const string PrototypeArtFolder = "Assets/Art/Prototype";

    private static readonly string[] DogNames =
    {
        "Dog 1",
        "Dog 2",
        "Dog 3",
        "Dog 4",
        "Dog 5",
        "Dog 6",
        "Dog 7",
        "Dog 8",
        "Dog 9"
    };

    private static readonly Color[] BodyColors =
    {
        new Color(0.42f, 0.45f, 0.42f),
        new Color(0.95f, 0.96f, 0.92f),
        new Color(0.92f, 0.86f, 0.70f),
        new Color(0.95f, 0.88f, 0.28f),
        new Color(0.98f, 0.98f, 0.96f),
        new Color(0.76f, 0.70f, 0.54f),
        new Color(0.96f, 0.96f, 0.92f),
        new Color(0.94f, 0.92f, 0.88f),
        new Color(0.82f, 0.80f, 0.70f)
    };

    [MenuItem("Tools/Dog Dex/Build Prototype Scene")]
    public static void Build()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Data");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Art");
        EnsureFolder(ArtFolder, "Prototype");

        Sprite[] icons = CreatePrototypeIcons();
        DogDexDatabase database = CreateDatabase(icons);
        GameObject slotPrefab = CreateSlotPrefab();
        CreateScene(database, slotPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static Sprite[] CreatePrototypeIcons()
    {
        Sprite[] sprites = new Sprite[DogNames.Length];

        for (int i = 0; i < DogNames.Length; i++)
        {
            string path = PrototypeArtFolder + "/dog_level_" + (i + 1) + ".png";
            CreateIconTexture(path, i);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprites[i] == null)
            {
                Debug.LogError("Dog dex prototype icon failed to load as Sprite: " + path);
            }
        }

        return sprites;
    }

    private static void CreateIconTexture(string assetPath, int index)
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Clear(texture, new Color(0f, 0f, 0f, 0f));
        DrawDog(texture, index);

        if (index == 1)
        {
            DrawSpots(texture);
        }
        else if (index == 2)
        {
            DrawCollar(texture, Color.red);
        }
        else if (index == 3)
        {
            DrawPartyHat(texture);
        }
        else if (index == 4)
        {
            DrawScarf(texture);
        }
        else if (index == 5)
        {
            DrawSunglasses(texture);
        }
        else if (index == 6)
        {
            DrawBone(texture);
        }
        else if (index == 7)
        {
            DrawChefHat(texture);
        }
        else if (index == 8)
        {
            DrawCrown(texture);
        }

        FlipVertically(texture);
        texture.Apply();
        File.WriteAllBytes(assetPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    private static void FlipVertically(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;

        for (int y = 0; y < height / 2; y++)
        {
            int oppositeY = height - y - 1;
            for (int x = 0; x < width; x++)
            {
                Color bottom = texture.GetPixel(x, y);
                Color top = texture.GetPixel(x, oppositeY);
                texture.SetPixel(x, y, top);
                texture.SetPixel(x, oppositeY, bottom);
            }
        }
    }

    private static DogDexDatabase CreateDatabase(Sprite[] icons)
    {
        string path = DataFolder + "/DogDexDatabase.asset";
        DogDexDatabase database = AssetDatabase.LoadAssetAtPath<DogDexDatabase>(path);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<DogDexDatabase>();
            AssetDatabase.CreateAsset(database, path);
        }

        SerializedObject serializedDatabase = new SerializedObject(database);
        SerializedProperty entries = serializedDatabase.FindProperty("entries");
        entries.arraySize = icons.Length;

        for (int i = 0; i < icons.Length; i++)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("id").intValue = i + 1;
            entry.FindPropertyRelative("displayName").stringValue = DogNames[i];
            entry.FindPropertyRelative("level").intValue = i + 1;
            SerializedProperty iconProperty = entry.FindPropertyRelative("icon");
            iconProperty.objectReferenceValue = icons[i];
            if (icons[i] == null)
            {
                Debug.LogError("Dog dex database entry has no icon: " + (i + 1));
            }
        }

        serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(database);
        return database;
    }

    private static GameObject CreateSlotPrefab()
    {
        string path = PrefabsFolder + "/DogDexSlot.prefab";

        GameObject root = CreateRectObject("DogDexSlot", new Vector2(145f, 158f));

        GameObject iconFrame = CreateRectObject("IconFrame", new Vector2(126f, 116f), root.transform);
        RectTransform frameRect = iconFrame.GetComponent<RectTransform>();
        frameRect.anchoredPosition = new Vector2(0f, 20f);
        Image frameImage = iconFrame.AddComponent<Image>();
        frameImage.color = new Color(0.82f, 0.82f, 0.80f);
        Outline frameOutline = iconFrame.AddComponent<Outline>();
        frameOutline.effectColor = new Color(0.10f, 0.12f, 0.14f);
        frameOutline.effectDistance = new Vector2(3f, -3f);

        GameObject iconObject = CreateRectObject("Icon", new Vector2(96f, 96f), iconFrame.transform);
        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.preserveAspect = true;

        Text nameText = CreateText("Name", root.transform, "1", 24, TextAnchor.MiddleCenter);
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(120f, 30f);
        nameRect.anchoredPosition = new Vector2(0f, -61f);
        nameText.color = new Color(0.11f, 0.16f, 0.20f);
        nameText.fontStyle = FontStyle.Bold;

        Text lockedMark = CreateText("LockedMark", iconFrame.transform, "?", 48, TextAnchor.MiddleCenter);
        RectTransform lockedRect = lockedMark.GetComponent<RectTransform>();
        lockedRect.sizeDelta = new Vector2(80f, 80f);
        lockedMark.color = new Color(0.18f, 0.20f, 0.22f);
        lockedMark.fontStyle = FontStyle.Bold;

        DogDexSlotView slotView = root.AddComponent<DogDexSlotView>();
        SerializedObject serializedSlot = new SerializedObject(slotView);
        serializedSlot.FindProperty("frameImage").objectReferenceValue = frameImage;
        serializedSlot.FindProperty("iconImage").objectReferenceValue = iconImage;
        serializedSlot.FindProperty("nameText").objectReferenceValue = nameText;
        serializedSlot.FindProperty("lockedMark").objectReferenceValue = lockedMark.gameObject;
        serializedSlot.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static void CreateScene(DogDexDatabase database, GameObject slotPrefab)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "DogDexPrototype";

        GameObject managerObject = new GameObject("DogDexManager");
        DogDexManager manager = managerObject.AddComponent<DogDexManager>();
        SerializedObject serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("database").objectReferenceValue = database;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();

        CreateCamera();
        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        GameObject panel = CreateRectObject("DexPanel", new Vector2(540f, 570f), canvas.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.40f, 0.65f, 0.82f);

        GameObject grid = CreateRectObject("DogDexGrid", new Vector2(470f, 505f), panel.transform);
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.anchoredPosition = new Vector2(0f, -2f);
        GridLayoutGroup layout = grid.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(145f, 158f);
        layout.spacing = new Vector2(12f, 10f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.childAlignment = TextAnchor.MiddleCenter;

        GameObject viewObject = new GameObject("DogDexView");
        viewObject.transform.SetParent(panel.transform, false);
        DogDexView view = viewObject.AddComponent<DogDexView>();
        SerializedObject serializedView = new SerializedObject(view);
        serializedView.FindProperty("dexManager").objectReferenceValue = manager;
        serializedView.FindProperty("slotPrefab").objectReferenceValue = slotPrefab.GetComponent<DogDexSlotView>();
        serializedView.FindProperty("slotParent").objectReferenceValue = grid.transform;
        serializedView.FindProperty("showAllEntries").boolValue = false;
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        for (int i = 0; i < database.Entries.Count; i++)
        {
            GameObject slotObject = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, grid.transform);
            DogDexSlotView slot = slotObject.GetComponent<DogDexSlotView>();
            slot.name = "DogDexSlot_" + (i + 1).ToString("00");
            slot.SetEntry(database.Entries[i], false);
        }

        CreateDebugButtons(canvas.transform, manager, view);

        EditorSceneManager.SaveScene(scene, ScenesFolder + "/DogDexPrototype.unity");
    }

    private static void CreateDebugButtons(Transform parent, DogDexManager manager, DogDexView view)
    {
        GameObject row = CreateRectObject("DebugUnlockButtons", new Vector2(760f, 82f), parent);
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0f);
        rowRect.anchorMax = new Vector2(0.5f, 0f);
        rowRect.anchoredPosition = new Vector2(0f, 48f);

        GridLayoutGroup layout = row.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(68f, 30f);
        layout.spacing = new Vector2(8f, 8f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;

        for (int i = 1; i <= DogNames.Length; i++)
        {
            CreateUnlockButton(row.transform, manager, view, i);
        }

        CreateClearButton(row.transform, manager, view);
    }

    private static void CreateUnlockButton(Transform parent, DogDexManager manager, DogDexView view, int dogId)
    {
        Button button = CreateButton(parent, dogId.ToString());
        DogDexDebugUnlocker unlocker = button.gameObject.AddComponent<DogDexDebugUnlocker>();
        SerializedObject serializedUnlocker = new SerializedObject(unlocker);
        serializedUnlocker.FindProperty("dexManager").objectReferenceValue = manager;
        serializedUnlocker.FindProperty("dexView").objectReferenceValue = view;
        serializedUnlocker.FindProperty("dogId").intValue = dogId;
        serializedUnlocker.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateClearButton(Transform parent, DogDexManager manager, DogDexView view)
    {
        Button button = CreateButton(parent, "Reset");
        DogDexDebugUnlocker unlocker = button.gameObject.AddComponent<DogDexDebugUnlocker>();
        SerializedObject serializedUnlocker = new SerializedObject(unlocker);
        serializedUnlocker.FindProperty("dexManager").objectReferenceValue = manager;
        serializedUnlocker.FindProperty("dexView").objectReferenceValue = view;
        serializedUnlocker.FindProperty("clearOnClick").boolValue = true;
        serializedUnlocker.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
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
        camera.backgroundColor = new Color(0.93f, 0.93f, 0.93f);
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

    private static Button CreateButton(Transform parent, string label)
    {
        GameObject buttonObject = CreateRectObject("Unlock_" + label, new Vector2(68f, 30f), parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.20f, 0.30f, 0.54f);
        Button button = buttonObject.AddComponent<Button>();

        Text text = CreateText("Text", buttonObject.transform, label, 14, TextAnchor.MiddleCenter);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
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

    private static void Clear(Texture2D texture, Color color)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }

    private static void DrawBlob(Texture2D texture, RectInt rect, Color color)
    {
        Vector2 center = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
        Vector2 radius = new Vector2(rect.width * 0.5f, rect.height * 0.5f);

        for (int y = rect.yMin; y < rect.yMax; y++)
        {
            for (int x = rect.xMin; x < rect.xMax; x++)
            {
                float dx = (x - center.x) / radius.x;
                float dy = (y - center.y) / radius.y;
                if (dx * dx + dy * dy <= 1f)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int yy = y; yy < y + height; yy++)
        {
            for (int xx = x; xx < x + width; xx++)
            {
                if (xx >= 0 && yy >= 0 && xx < texture.width && yy < texture.height)
                {
                    texture.SetPixel(xx, yy, color);
                }
            }
        }
    }

    private static void DrawDog(Texture2D texture, int index)
    {
        Color body = BodyColors[index];
        Color outline = new Color(0.08f, 0.08f, 0.08f);
        Color leg = Color.Lerp(body, Color.black, 0.25f);
        Color nose = new Color(0.08f, 0.07f, 0.06f);

        DrawRect(texture, 22, 54, 16, 8, outline);
        DrawRect(texture, 18, 48, 12, 8, body);
        DrawBlob(texture, new RectInt(30, 48, 58, 36), body);
        DrawBlob(texture, new RectInt(76, 34, 34, 32), body);
        DrawRect(texture, 103, 48, 14, 9, body);
        DrawRect(texture, 114, 51, 5, 5, nose);

        DrawRect(texture, 78, 24, 11, 18, outline);
        DrawRect(texture, 80, 26, 8, 16, Color.Lerp(body, Color.black, 0.18f));
        DrawRect(texture, 94, 24, 10, 18, outline);
        DrawRect(texture, 95, 26, 8, 16, Color.Lerp(body, Color.black, 0.18f));

        DrawRect(texture, 47, 80, 8, 20, leg);
        DrawRect(texture, 70, 80, 8, 20, leg);
        DrawRect(texture, 42, 98, 20, 5, outline);
        DrawRect(texture, 65, 98, 20, 5, outline);
        DrawRect(texture, 96, 43, 4, 4, Color.black);

        DrawRect(texture, 32, 48, 48, 3, outline);
        DrawRect(texture, 31, 82, 50, 3, outline);
        DrawRect(texture, 30, 52, 3, 28, outline);
        DrawRect(texture, 86, 52, 3, 28, outline);
        DrawRect(texture, 78, 34, 28, 3, outline);
        DrawRect(texture, 78, 64, 28, 3, outline);
        DrawRect(texture, 76, 38, 3, 24, outline);
        DrawRect(texture, 108, 40, 3, 22, outline);
    }

    private static void DrawSpots(Texture2D texture)
    {
        Color spot = new Color(0.20f, 0.20f, 0.18f);
        DrawBlob(texture, new RectInt(44, 54, 18, 14), spot);
        DrawBlob(texture, new RectInt(70, 60, 14, 12), spot);
    }

    private static void DrawCollar(Texture2D texture, Color color)
    {
        DrawRect(texture, 78, 62, 28, 5, color);
        DrawRect(texture, 90, 67, 7, 7, new Color(1.0f, 0.82f, 0.10f));
    }

    private static void DrawPartyHat(Texture2D texture)
    {
        DrawRect(texture, 85, 18, 18, 8, new Color(0.96f, 0.20f, 0.24f));
        DrawRect(texture, 89, 10, 10, 10, new Color(0.96f, 0.20f, 0.24f));
        DrawRect(texture, 92, 5, 5, 6, new Color(1.0f, 0.85f, 0.15f));
    }

    private static void DrawScarf(Texture2D texture)
    {
        DrawRect(texture, 76, 62, 36, 7, new Color(0.85f, 0.10f, 0.10f));
        DrawRect(texture, 100, 66, 8, 22, new Color(0.85f, 0.10f, 0.10f));
    }

    private static void DrawSunglasses(Texture2D texture)
    {
        DrawRect(texture, 83, 42, 12, 8, Color.black);
        DrawRect(texture, 99, 42, 12, 8, Color.black);
        DrawRect(texture, 95, 45, 4, 2, Color.black);
    }

    private static void DrawBone(Texture2D texture)
    {
        Color bone = new Color(0.96f, 0.92f, 0.82f);
        DrawRect(texture, 16, 42, 30, 8, bone);
        DrawBlob(texture, new RectInt(8, 36, 14, 14), bone);
        DrawBlob(texture, new RectInt(38, 36, 14, 14), bone);
    }

    private static void DrawChefHat(Texture2D texture)
    {
        DrawRect(texture, 78, 18, 30, 12, Color.white);
        DrawRect(texture, 84, 8, 20, 14, Color.white);
        DrawRect(texture, 84, 30, 18, 14, Color.white);
    }

    private static void DrawCrown(Texture2D texture)
    {
        Color crown = new Color(1.0f, 0.80f, 0.10f);
        DrawRect(texture, 78, 20, 34, 8, crown);
        DrawRect(texture, 80, 10, 8, 12, crown);
        DrawRect(texture, 92, 6, 8, 16, crown);
        DrawRect(texture, 104, 10, 8, 12, crown);
    }
}
