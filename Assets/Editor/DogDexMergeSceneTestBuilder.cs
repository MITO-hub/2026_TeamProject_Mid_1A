using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DogDexMergeSceneTestBuilder
{
    private const string SourceScenePath = "Assets/Scenes/MergeScene.unity";
    private const string TestScenePath = "Assets/Scenes/MergeSceneDogDexTest.unity";
    private const string DexDatabasePath = "Assets/Data/DogDexDatabase.asset";
    private const string SlotPrefabPath = "Assets/Prefabs/DogDexSlot.prefab";

    [MenuItem("Tools/Dog Dex/Build Merge Test Scene")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene, TestScenePath);

        DogDexDatabase database = AssetDatabase.LoadAssetAtPath<DogDexDatabase>(DexDatabasePath);
        DogDexSlotView slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath).GetComponent<DogDexSlotView>();

        DogDexManager dexManager = EnsureDexManager(database);
        DisableAutoSpawnGameManager();
        Canvas canvas = CreateCanvas();
        DogDexView dexView = CreateDexPanel(canvas.transform, dexManager, slotPrefab, database);
        DogSpawner spawner = Object.FindAnyObjectByType<DogSpawner>();
        CreateSpawnButtons(canvas.transform, spawner, dexManager, dexView);

        EditorSceneManager.SaveScene(scene, TestScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static DogDexManager EnsureDexManager(DogDexDatabase database)
    {
        DogDexManager manager = Object.FindAnyObjectByType<DogDexManager>();
        if (manager == null)
        {
            GameObject managerObject = new GameObject("DogDexManager");
            manager = managerObject.AddComponent<DogDexManager>();
        }

        SerializedObject serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("database").objectReferenceValue = database;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        return manager;
    }

    private static void DisableAutoSpawnGameManager()
    {
        GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.gameObject.SetActive(false);
        }
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("DogDexTestCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static DogDexView CreateDexPanel(Transform parent, DogDexManager manager, DogDexSlotView slotPrefab, DogDexDatabase database)
    {
        GameObject panel = CreateRectObject("DexPanel", new Vector2(540f, 570f), parent);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-45f, 15f);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.40f, 0.65f, 0.82f, 0.94f);

        Text title = CreateText("Title", panel.transform, "Dog Dex", 30, TextAnchor.MiddleCenter);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -35f);
        titleRect.sizeDelta = new Vector2(260f, 42f);
        title.color = new Color(0.08f, 0.12f, 0.16f);
        title.fontStyle = FontStyle.Bold;

        GameObject grid = CreateRectObject("DogDexGrid", new Vector2(470f, 505f), panel.transform);
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.anchoredPosition = new Vector2(0f, -32f);

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
        serializedView.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
        serializedView.FindProperty("slotParent").objectReferenceValue = grid.transform;
        serializedView.FindProperty("showAllEntries").boolValue = false;
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        for (int i = 0; i < database.Entries.Count; i++)
        {
            GameObject slotObject = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab.gameObject, grid.transform);
            DogDexSlotView slot = slotObject.GetComponent<DogDexSlotView>();
            slot.name = "DogDexSlot_" + (i + 1).ToString("00");
            slot.SetEntry(database.Entries[i], false);
        }

        return view;
    }

    private static void CreateSpawnButtons(Transform parent, DogSpawner spawner, DogDexManager manager, DogDexView view)
    {
        GameObject row = CreateRectObject("DogSpawnTestButtons", new Vector2(760f, 82f), parent);
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

        for (int i = 1; i <= 9; i++)
        {
            Button button = CreateButton(row.transform, "Spawn " + i);
            DogSpawnDebugButton debugButton = button.gameObject.AddComponent<DogSpawnDebugButton>();
            SerializedObject serializedButton = new SerializedObject(debugButton);
            serializedButton.FindProperty("spawner").objectReferenceValue = spawner;
            serializedButton.FindProperty("dogId").intValue = i;
            serializedButton.ApplyModifiedPropertiesWithoutUndo();
        }

        Button resetButton = CreateButton(row.transform, "Reset");
        DogDexDebugUnlocker reset = resetButton.gameObject.AddComponent<DogDexDebugUnlocker>();
        SerializedObject serializedReset = new SerializedObject(reset);
        serializedReset.FindProperty("dexManager").objectReferenceValue = manager;
        serializedReset.FindProperty("dexView").objectReferenceValue = view;
        serializedReset.FindProperty("clearOnClick").boolValue = true;
        serializedReset.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Button CreateButton(Transform parent, string label)
    {
        GameObject buttonObject = CreateRectObject(label, new Vector2(68f, 30f), parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.20f, 0.30f, 0.54f);
        Button button = buttonObject.AddComponent<Button>();

        Text text = CreateText("Text", buttonObject.transform, label, 12, TextAnchor.MiddleCenter);
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
