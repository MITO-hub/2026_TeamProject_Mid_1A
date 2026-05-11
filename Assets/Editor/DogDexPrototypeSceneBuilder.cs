using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DogDexPrototypeSceneBuilder
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string DataFolder = "Assets/Data";
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string ArtFolder = "Assets/Art";
    private const string PrototypeArtFolder = "Assets/Art/Prototype";

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
        DogDexSlotView slotPrefab = CreateSlotPrefab();
        CreateScene(database, slotPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static Sprite[] CreatePrototypeIcons()
    {
        Color[] colors =
        {
            new Color(0.95f, 0.95f, 0.95f),
            new Color(1.00f, 0.82f, 0.24f),
            new Color(1.00f, 0.50f, 0.20f),
            new Color(0.90f, 0.25f, 0.30f),
            new Color(0.50f, 0.35f, 0.95f),
            new Color(0.25f, 0.70f, 1.00f)
        };

        Sprite[] sprites = new Sprite[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            string path = $"{PrototypeArtFolder}/dog_level_{i + 1}.png";
            CreateIconTexture(path, colors[i]);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.SaveAndReimport();

            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        return sprites;
    }

    private static void CreateIconTexture(string assetPath, Color color)
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.38f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color pixel = distance <= radius ? color : Color.clear;

                if (distance > radius - 5f && distance <= radius)
                {
                    pixel = Color.Lerp(pixel, Color.black, 0.18f);
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        File.WriteAllBytes(assetPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    private static DogDexDatabase CreateDatabase(Sprite[] icons)
    {
        string path = $"{DataFolder}/DogDexDatabase.asset";
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
            entry.FindPropertyRelative("displayName").stringValue = $"강아지 {i + 1}단계";
            entry.FindPropertyRelative("level").intValue = i + 1;
            entry.FindPropertyRelative("icon").objectReferenceValue = icons[i];
        }

        serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(database);
        return database;
    }

    private static DogDexSlotView CreateSlotPrefab()
    {
        string path = $"{PrefabsFolder}/DogDexSlot.prefab";

        GameObject root = CreateRectObject("DogDexSlot", new Vector2(150f, 190f));
        Image background = root.AddComponent<Image>();
        background.color = new Color(0.13f, 0.13f, 0.15f);

        GameObject iconObject = CreateRectObject("Icon", new Vector2(92f, 92f), root.transform);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchoredPosition = new Vector2(0f, 34f);
        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.preserveAspect = true;

        Text nameText = CreateText("Name", root.transform, "???", 18, TextAnchor.MiddleCenter);
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(130f, 28f);
        nameRect.anchoredPosition = new Vector2(0f, -42f);

        Text levelText = CreateText("Level", root.transform, "Locked", 14, TextAnchor.MiddleCenter);
        RectTransform levelRect = levelText.GetComponent<RectTransform>();
        levelRect.sizeDelta = new Vector2(130f, 24f);
        levelRect.anchoredPosition = new Vector2(0f, -72f);
        levelText.color = new Color(0.70f, 0.70f, 0.75f);

        Text lockedMark = CreateText("LockedMark", root.transform, "LOCK", 14, TextAnchor.MiddleCenter);
        RectTransform lockedRect = lockedMark.GetComponent<RectTransform>();
        lockedRect.sizeDelta = new Vector2(80f, 24f);
        lockedRect.anchoredPosition = new Vector2(0f, 34f);
        lockedMark.color = new Color(0.15f, 0.15f, 0.16f);

        DogDexSlotView slotView = root.AddComponent<DogDexSlotView>();
        SerializedObject serializedSlot = new SerializedObject(slotView);
        serializedSlot.FindProperty("iconImage").objectReferenceValue = iconImage;
        serializedSlot.FindProperty("nameText").objectReferenceValue = nameText;
        serializedSlot.FindProperty("levelText").objectReferenceValue = levelText;
        serializedSlot.FindProperty("lockedMark").objectReferenceValue = lockedMark.gameObject;
        serializedSlot.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<DogDexSlotView>();
    }

    private static void CreateScene(DogDexDatabase database, DogDexSlotView slotPrefab)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "DogDexPrototype";

        GameObject managerObject = new GameObject("DogDexManager");
        DogDexManager manager = managerObject.AddComponent<DogDexManager>();
        SerializedObject serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("database").objectReferenceValue = database;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();

        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        Text title = CreateText("Title", canvas.transform, "Dog Dex Prototype", 34, TextAnchor.MiddleCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -58f);
        titleRect.sizeDelta = new Vector2(620f, 52f);

        GameObject grid = CreateRectObject("DogDexGrid", new Vector2(960f, 420f), canvas.transform);
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.anchoredPosition = new Vector2(0f, 35f);
        GridLayoutGroup layout = grid.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(150f, 190f);
        layout.spacing = new Vector2(18f, 18f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.childAlignment = TextAnchor.MiddleCenter;

        GameObject viewObject = new GameObject("DogDexView");
        viewObject.transform.SetParent(canvas.transform, false);
        DogDexView view = viewObject.AddComponent<DogDexView>();
        SerializedObject serializedView = new SerializedObject(view);
        serializedView.FindProperty("dexManager").objectReferenceValue = manager;
        serializedView.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
        serializedView.FindProperty("slotParent").objectReferenceValue = grid.transform;
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        CreateButtons(canvas.transform, manager);

        EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/DogDexPrototype.unity");
    }

    private static void CreateButtons(Transform parent, DogDexManager manager)
    {
        GameObject buttonRow = CreateRectObject("DebugButtons", new Vector2(980f, 120f), parent);
        RectTransform rowRect = buttonRow.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0f);
        rowRect.anchorMax = new Vector2(0.5f, 0f);
        rowRect.anchoredPosition = new Vector2(0f, 86f);
        HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.spacing = 12f;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        for (int i = 1; i <= 6; i++)
        {
            CreateUnlockButton(buttonRow.transform, manager, i);
        }

        CreateClearButton(buttonRow.transform, manager);
    }

    private static void CreateUnlockButton(Transform parent, DogDexManager manager, int dogId)
    {
        Button button = CreateButton(parent, $"{dogId}단계 해금");
        DogDexDebugUnlocker unlocker = button.gameObject.AddComponent<DogDexDebugUnlocker>();
        SerializedObject serializedUnlocker = new SerializedObject(unlocker);
        serializedUnlocker.FindProperty("dexManager").objectReferenceValue = manager;
        serializedUnlocker.FindProperty("dogId").intValue = dogId;
        serializedUnlocker.ApplyModifiedPropertiesWithoutUndo();
        UnityEventTools.AddPersistentListener(button.onClick, new UnityAction(unlocker.UnlockDog));
    }

    private static void CreateClearButton(Transform parent, DogDexManager manager)
    {
        Button button = CreateButton(parent, "초기화");
        DogDexDebugUnlocker unlocker = button.gameObject.AddComponent<DogDexDebugUnlocker>();
        SerializedObject serializedUnlocker = new SerializedObject(unlocker);
        serializedUnlocker.FindProperty("dexManager").objectReferenceValue = manager;
        serializedUnlocker.ApplyModifiedPropertiesWithoutUndo();
        UnityEventTools.AddPersistentListener(button.onClick, new UnityAction(unlocker.ClearDex));
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static Button CreateButton(Transform parent, string label)
    {
        GameObject buttonObject = CreateRectObject(label, new Vector2(122f, 42f), parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.32f, 0.58f);
        Button button = buttonObject.AddComponent<Button>();

        Text text = CreateText("Text", buttonObject.transform, label, 15, TextAnchor.MiddleCenter);
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
}
