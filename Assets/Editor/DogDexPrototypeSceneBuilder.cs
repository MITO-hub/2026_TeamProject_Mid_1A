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
        "\ub3cc\uae30",
        "\ubc31\ub3cc\uae30",
        "\ub2ed\ub3cc\uae30",
        "\ube44\ub3cc\uae30",
        "\ub208\ub3cc\uae30",
        "\ub3c4\ub451\ub3cc\uae30",
        "\ubc18\uc2e0\uc695\ub3cc\uae30",
        "\uc250\ud504\ub3cc\uae30",
        "\ub3cc\uae30\ub300\uc655"
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

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        return sprites;
    }

    private static void CreateIconTexture(string assetPath, int index)
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Clear(texture, new Color(0f, 0f, 0f, 0f));
        DrawBlob(texture, new RectInt(28, 48, 62, 40), BodyColors[index]);
        DrawBlob(texture, new RectInt(66, 32, 28, 30), BodyColors[index]);
        DrawRect(texture, 86, 43, 14, 5, new Color(0.96f, 0.62f, 0.22f));
        DrawRect(texture, 46, 84, 7, 22, new Color(0.48f, 0.26f, 0.14f));
        DrawRect(texture, 70, 84, 7, 22, new Color(0.48f, 0.26f, 0.14f));
        DrawRect(texture, 38, 104, 20, 5, new Color(0.24f, 0.16f, 0.12f));
        DrawRect(texture, 64, 104, 20, 5, new Color(0.24f, 0.16f, 0.12f));
        DrawRect(texture, 77, 40, 5, 5, Color.black);
        DrawOutline(texture);

        if (index == 2)
        {
            DrawRect(texture, 72, 24, 18, 8, Color.red);
            DrawRect(texture, 80, 16, 6, 10, Color.red);
        }
        else if (index == 3)
        {
            DrawUmbrella(texture);
        }
        else if (index == 4)
        {
            DrawScarf(texture);
        }
        else if (index == 5)
        {
            DrawMask(texture);
        }
        else if (index == 6)
        {
            DrawBath(texture);
        }
        else if (index == 7)
        {
            DrawChefHat(texture);
        }
        else if (index == 8)
        {
            DrawCape(texture);
        }

        texture.Apply();
        File.WriteAllBytes(assetPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
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
            entry.FindPropertyRelative("icon").objectReferenceValue = icons[i];
        }

        serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(database);
        return database;
    }

    private static GameObject CreateSlotPrefab()
    {
        string path = PrefabsFolder + "/DogDexSlot.prefab";

        GameObject root = CreateRectObject("DogDexSlot", new Vector2(170f, 190f));

        GameObject iconFrame = CreateRectObject("IconFrame", new Vector2(152f, 138f), root.transform);
        RectTransform frameRect = iconFrame.GetComponent<RectTransform>();
        frameRect.anchoredPosition = new Vector2(0f, 24f);
        Image frameImage = iconFrame.AddComponent<Image>();
        frameImage.color = new Color(0.82f, 0.82f, 0.80f);
        Outline frameOutline = iconFrame.AddComponent<Outline>();
        frameOutline.effectColor = new Color(0.10f, 0.12f, 0.14f);
        frameOutline.effectDistance = new Vector2(3f, -3f);

        GameObject iconObject = CreateRectObject("Icon", new Vector2(118f, 118f), iconFrame.transform);
        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.preserveAspect = true;

        Text nameText = CreateText("Name", root.transform, "1. ???", 22, TextAnchor.MiddleCenter);
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(178f, 34f);
        nameRect.anchoredPosition = new Vector2(0f, -72f);
        nameText.color = new Color(0.11f, 0.16f, 0.20f);
        nameText.fontStyle = FontStyle.Bold;

        Text lockedMark = CreateText("LockedMark", iconFrame.transform, "?", 48, TextAnchor.MiddleCenter);
        RectTransform lockedRect = lockedMark.GetComponent<RectTransform>();
        lockedRect.sizeDelta = new Vector2(80f, 80f);
        lockedMark.color = new Color(0.18f, 0.20f, 0.22f);
        lockedMark.fontStyle = FontStyle.Bold;

        DogDexSlotView slotView = root.AddComponent<DogDexSlotView>();
        SerializedObject serializedSlot = new SerializedObject(slotView);
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

        GameObject panel = CreateRectObject("DexPanel", new Vector2(650f, 665f), canvas.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.40f, 0.65f, 0.82f);

        GameObject grid = CreateRectObject("DogDexGrid", new Vector2(570f, 610f), panel.transform);
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.anchoredPosition = new Vector2(0f, -4f);
        GridLayoutGroup layout = grid.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(170f, 190f);
        layout.spacing = new Vector2(20f, 16f);
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
        serializedView.FindProperty("showAllEntries").boolValue = true;
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        for (int i = 0; i < database.Entries.Count; i++)
        {
            GameObject slotObject = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, grid.transform);
            DogDexSlotView slot = slotObject.GetComponent<DogDexSlotView>();
            slot.name = "DogDexSlot_" + (i + 1).ToString("00");
            slot.SetEntry(database.Entries[i], true);
        }

        EditorSceneManager.SaveScene(scene, ScenesFolder + "/DogDexPrototype.unity");
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720f, 720f);
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

    private static void DrawOutline(Texture2D texture)
    {
        Color outline = new Color(0.08f, 0.08f, 0.08f);
        for (int x = 28; x < 90; x++)
        {
            texture.SetPixel(x, 48, outline);
            texture.SetPixel(x, 88, outline);
        }
        for (int y = 48; y < 88; y++)
        {
            texture.SetPixel(28, y, outline);
            texture.SetPixel(90, y, outline);
        }
    }

    private static void DrawUmbrella(Texture2D texture)
    {
        DrawRect(texture, 18, 74, 54, 5, new Color(0.20f, 0.55f, 0.20f));
        DrawRect(texture, 24, 80, 42, 10, new Color(0.45f, 0.85f, 0.25f));
        DrawRect(texture, 44, 58, 4, 22, new Color(0.35f, 0.20f, 0.12f));
    }

    private static void DrawScarf(Texture2D texture)
    {
        DrawRect(texture, 66, 56, 42, 8, new Color(0.85f, 0.10f, 0.10f));
        DrawRect(texture, 90, 44, 8, 24, new Color(0.85f, 0.10f, 0.10f));
    }

    private static void DrawMask(Texture2D texture)
    {
        DrawRect(texture, 68, 34, 24, 12, new Color(0.08f, 0.08f, 0.08f));
        DrawRect(texture, 20, 68, 24, 6, new Color(0.98f, 0.92f, 0.30f));
    }

    private static void DrawBath(Texture2D texture)
    {
        DrawRect(texture, 22, 58, 64, 28, new Color(0.80f, 0.95f, 1.00f));
        DrawRect(texture, 18, 82, 72, 6, new Color(0.20f, 0.70f, 0.85f));
        DrawRect(texture, 30, 90, 8, 10, new Color(0.20f, 0.70f, 0.85f));
        DrawRect(texture, 72, 90, 8, 10, new Color(0.20f, 0.70f, 0.85f));
    }

    private static void DrawChefHat(Texture2D texture)
    {
        DrawRect(texture, 66, 18, 28, 12, Color.white);
        DrawRect(texture, 70, 8, 20, 14, Color.white);
        DrawRect(texture, 70, 30, 20, 20, Color.white);
    }

    private static void DrawCape(Texture2D texture)
    {
        DrawRect(texture, 34, 54, 48, 42, new Color(0.82f, 0.12f, 0.18f));
        DrawRect(texture, 52, 66, 22, 18, new Color(1.00f, 0.82f, 0.18f));
    }
}
