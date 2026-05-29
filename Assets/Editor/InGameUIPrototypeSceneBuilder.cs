using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class InGameUIPrototypeSceneBuilder
{
    private const string InGameScenePath = "Assets/Scenes/InGameUIPrototype.unity";
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";
    private const string DexDatabasePath = "Assets/Data/DogDexDatabase.asset";
    private const string SlotPrefabPath = "Assets/Prefabs/DogDexSlot.prefab";
    private const string NewUiFolder = "Assets/Art/Prototype/UI_20260529";

    [MenuItem("Tools/In Game UI/Build Prototype Scene")]
    public static void Build()
    {
        AssetDatabase.Refresh();
        EnsureNewUiSprites();
        BuildTitleScene();
        BuildInGameScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void BuildTitleScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "TitleScene";

        CreateCamera();
        Canvas canvas = CreateCanvas("TitleCanvas");
        CreateEventSystem();

        InGameUIController controller = new GameObject("TitleUIController").AddComponent<InGameUIController>();
        CreateFullScreenImage("TitleBackground", canvas.transform, LoadNewUiSprite("UI-01"));
        AddTransparentButton(canvas.transform, "StartButton", new Vector2(250f, 78f), controller.StartGame, new Vector2(-72f, -76f));
        AddTransparentButton(canvas.transform, "ContinueButton", new Vector2(190f, 58f), controller.ContinueGame, new Vector2(-72f, -164f));
        AddTransparentButton(canvas.transform, "HelpButton", new Vector2(54f, 54f), controller.ContinueGame, new Vector2(-85f, -248f));
        AddTransparentButton(canvas.transform, "SettingsButton", new Vector2(54f, 54f), controller.ToggleSettings, new Vector2(-178f, -248f));
        AddTransparentButton(canvas.transform, "ExitButton", new Vector2(92f, 48f), controller.ExitGame, new Vector2(110f, -248f));

        GameObject settingsPanel = CreateSettingsPanel(canvas.transform, controller);
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        serializedController.ApplyModifiedPropertiesWithoutUndo();
        settingsPanel.SetActive(false);

        EditorSceneManager.SaveScene(scene, TitleScenePath);
    }

    private static void BuildInGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "InGameUIPrototype";

        CreateCamera();
        Canvas canvas = CreateCanvas("InGameUICanvas");
        CreateEventSystem();

        DogDexDatabase database = AssetDatabase.LoadAssetAtPath<DogDexDatabase>(DexDatabasePath);
        DogDexManager manager = CreateDexManager(database);

        InGameUIController controller = new GameObject("InGameUIController").AddComponent<InGameUIController>();
        CreateMainScreen(canvas.transform, controller);
        GameObject dogDexPanel = CreateDogDexPanel(canvas.transform, controller);
        GameObject settingsPanel = CreateSettingsPanel(canvas.transform, controller);

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        serializedController.FindProperty("dogDexPanel").objectReferenceValue = dogDexPanel;
        serializedController.FindProperty("dogDexView").objectReferenceValue = null;
        serializedController.FindProperty("dogDexManager").objectReferenceValue = manager;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        settingsPanel.SetActive(false);
        dogDexPanel.SetActive(false);

        EditorSceneManager.SaveScene(scene, InGameScenePath);
    }

    private static void CreateMainScreen(Transform parent, InGameUIController controller)
    {
        GameObject root = CreateRectObject("InGameUIScreen", Vector2.zero, parent);
        Stretch(root.GetComponent<RectTransform>());

        CreateFullScreenImage("NewInGameLayout", root.transform, LoadNewUiSprite("UI-02"));

        BoneScatterController boneScatter = root.AddComponent<BoneScatterController>();
        CreateSpriteImage("BoneGenerateCounterBone", root.transform, LoadNewUiSprite("Recource/자산 16"), new Vector2(116f, 70f), new Vector2(-548f, -281f));

        Text boneCountText = CreateText("BoneCountText", root.transform, "0/5", 25, TextAnchor.MiddleCenter);
        RectTransform boneCountRect = boneCountText.GetComponent<RectTransform>();
        boneCountRect.anchoredPosition = new Vector2(-548f, -281f);
        boneCountRect.sizeDelta = new Vector2(54f, 30f);
        boneCountText.color = new Color(0.25f, 0.22f, 0.15f);
        boneCountText.fontStyle = FontStyle.Bold;
        boneCountText.resizeTextForBestFit = true;
        boneCountText.resizeTextMinSize = 21;
        boneCountText.resizeTextMaxSize = 28;

        Button boneButton = AddTransparentButton(root.transform, "BoneGenerateButton", new Vector2(92f, 72f), boneScatter.SpawnBones, new Vector2(-542f, -284f));

        SerializedObject serializedScatter = new SerializedObject(boneScatter);
        serializedScatter.FindProperty("spawnRoot").objectReferenceValue = root.GetComponent<RectTransform>();
        serializedScatter.FindProperty("boneSprite").objectReferenceValue = LoadNewUiSprite("Recource/자산 16");
        serializedScatter.FindProperty("boneCountText").objectReferenceValue = boneCountText;
        serializedScatter.FindProperty("generateButton").objectReferenceValue = boneButton;
        serializedScatter.ApplyModifiedPropertiesWithoutUndo();

        AddTransparentButton(root.transform, "DogDexButton", new Vector2(74f, 82f), controller.ToggleDogDex, new Vector2(-454f, -284f));
        AddTransparentButton(root.transform, "SettingsButton", new Vector2(62f, 62f), controller.ToggleSettings, new Vector2(-586f, 286f));
    }

    private static GameObject CreateDogDexPanel(Transform parent, InGameUIController controller)
    {
        GameObject panel = CreateRectObject("DogDexPanel", Vector2.zero, parent);
        Stretch(panel.GetComponent<RectTransform>());
        CreateFullScreenImage("NewDogDexLayout", panel.transform, LoadNewUiSprite("UI-03"));

        AddTransparentButton(panel.transform, "BackButton", new Vector2(120f, 72f), controller.HideDogDex, new Vector2(-410f, -272f));
        AddTransparentButton(panel.transform, "CloseButton", new Vector2(58f, 58f), controller.HideDogDex, new Vector2(522f, 260f));

        ScrollRect scrollRect = CreateDogDexScrollView(panel.transform);
        Transform content = scrollRect.content;
        CreateLockedDexPlaceholders(content, 40);
        return panel;
    }

    private static GameObject CreateSettingsPanel(Transform parent, InGameUIController controller)
    {
        GameObject panel = CreateRectObject("SettingsPanel", Vector2.zero, parent);
        Stretch(panel.GetComponent<RectTransform>());

        Image blocker = panel.AddComponent<Image>();
        blocker.color = new Color(1f, 1f, 1f, 0.72f);
        blocker.raycastTarget = true;

        GameObject window = CreateRectObject("SettingsWindow", new Vector2(1000f, 655f), panel.transform);
        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.anchoredPosition = new Vector2(24f, -2f);
        Image windowImage = window.AddComponent<Image>();
        windowImage.sprite = LoadNewUiSprite("Recource/자산 7");
        windowImage.color = Color.white;
        windowImage.preserveAspect = true;
        windowImage.raycastTarget = true;

        AddTransparentButton(window.transform, "CloseButton", new Vector2(70f, 70f), controller.HideSettings, new Vector2(454f, 152f));

        CreateSpriteImage("SpeakerIcon", window.transform, LoadNewUiSprite("Recource/자산 14"), new Vector2(78f, 78f), new Vector2(-320f, 80f));
        CreateSettingsLabel(window.transform, "BgmLabel", "배경화면", new Vector2(-306f, 18f));

        CreateSpriteImage("MusicIcon", window.transform, LoadNewUiSprite("Recource/자산 13"), new Vector2(78f, 78f), new Vector2(-320f, -90f));
        CreateSettingsLabel(window.transform, "SfxLabel", "효과음", new Vector2(-306f, -152f));

        AddSpriteButton(window.transform, "HelpButton", LoadNewUiSprite("Recource/자산 11"), new Vector2(86f, 86f), controller.ContinueGame, new Vector2(190f, -48f));
        CreateSettingsLabel(window.transform, "HelpLabel", "도움말", new Vector2(190f, -112f));

        AddSpriteButton(window.transform, "ExitButton", LoadNewUiSprite("Recource/자산 8"), new Vector2(86f, 86f), controller.ExitGame, new Vector2(340f, -48f));
        CreateSettingsLabel(window.transform, "ExitLabel", "나가기", new Vector2(340f, -112f));

        SettingsVolumeController volumeController = panel.AddComponent<SettingsVolumeController>();
        Slider bgmSlider = CreateVolumeSlider(window.transform, "BgmSlider", new Vector2(-102f, 78f));
        Slider sfxSlider = CreateVolumeSlider(window.transform, "SfxSlider", new Vector2(-102f, -92f));

        SerializedObject serializedVolume = new SerializedObject(volumeController);
        serializedVolume.FindProperty("bgmSlider").objectReferenceValue = bgmSlider;
        serializedVolume.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
        serializedVolume.FindProperty("bgmValueText").objectReferenceValue = null;
        serializedVolume.FindProperty("sfxValueText").objectReferenceValue = null;
        serializedVolume.ApplyModifiedPropertiesWithoutUndo();
        return panel;
    }

    private static ScrollRect CreateDogDexScrollView(Transform parent)
    {
        GameObject scrollObject = CreateRectObject("DogDexScrollView", new Vector2(460f, 468f), parent);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchoredPosition = new Vector2(298f, 4f);

        Image scrollBackground = scrollObject.AddComponent<Image>();
        scrollBackground.color = new Color(0.80f, 0.81f, 0.62f, 1f);
        scrollBackground.raycastTarget = true;

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 28f;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;

        GameObject viewport = CreateRectObject("Viewport", Vector2.zero, scrollObject.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect);
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        GameObject content = CreateRectObject("Content", new Vector2(460f, 0f), viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 1f);
        contentRect.anchorMax = new Vector2(0.5f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;

        GridLayoutGroup layout = content.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(92f, 138f);
        layout.spacing = new Vector2(18f, 10f);
        layout.padding = new RectOffset(16, 0, 14, 0);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 4;
        layout.childAlignment = TextAnchor.UpperLeft;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        return scrollRect;
    }

    private static void CreateLockedDexPlaceholders(Transform parent, int count)
    {
        Sprite questionSprite = LoadNewUiSprite("Recource/자산 12");

        for (int i = 0; i < count; i++)
        {
            GameObject slot = CreateRectObject("LockedDogSlot_" + (i + 1).ToString("00"), new Vector2(92f, 138f), parent);

            GameObject iconArea = CreateRectObject("IconArea", new Vector2(92f, 86f), slot.transform);
            RectTransform iconAreaRect = iconArea.GetComponent<RectTransform>();
            iconAreaRect.anchorMin = new Vector2(0.5f, 1f);
            iconAreaRect.anchorMax = new Vector2(0.5f, 1f);
            iconAreaRect.pivot = new Vector2(0.5f, 1f);
            iconAreaRect.anchoredPosition = Vector2.zero;
            Image iconAreaImage = iconArea.AddComponent<Image>();
            iconAreaImage.color = new Color(0.61f, 0.62f, 0.48f, 1f);
            iconAreaImage.raycastTarget = false;

            GameObject labelArea = CreateRectObject("LabelArea", new Vector2(92f, 32f), slot.transform);
            RectTransform labelAreaRect = labelArea.GetComponent<RectTransform>();
            labelAreaRect.anchorMin = new Vector2(0.5f, 0f);
            labelAreaRect.anchorMax = new Vector2(0.5f, 0f);
            labelAreaRect.pivot = new Vector2(0.5f, 0f);
            labelAreaRect.anchoredPosition = new Vector2(0f, 4f);
            Image labelAreaImage = labelArea.AddComponent<Image>();
            labelAreaImage.color = new Color(0.91f, 0.91f, 0.70f, 1f);
            labelAreaImage.raycastTarget = false;

            GameObject question = CreateRectObject("QuestionMark", new Vector2(44f, 44f), iconArea.transform);
            RectTransform questionRect = question.GetComponent<RectTransform>();
            questionRect.anchorMin = new Vector2(0.5f, 0.5f);
            questionRect.anchorMax = new Vector2(0.5f, 0.5f);
            questionRect.anchoredPosition = Vector2.zero;

            Image questionImage = question.AddComponent<Image>();
            questionImage.sprite = questionSprite;
            questionImage.color = new Color(0.16f, 0.19f, 0.21f, 0.92f);
            questionImage.preserveAspect = true;
            questionImage.raycastTarget = false;
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

    private static void EnsureNewUiSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { NewUiFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (!path.EndsWith(".png"))
            {
                continue;
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 8192;
            importer.SaveAndReimport();
        }
    }

    private static Image CreateFullScreenImage(string name, Transform parent, Sprite sprite)
    {
        GameObject imageObject = CreateRectObject(name, Vector2.zero, parent);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        Stretch(rect);

        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = sprite == null ? new Color(0.95f, 0.94f, 0.88f) : Color.white;
        image.preserveAspect = false;
        image.raycastTarget = false;
        return image;
    }

    private static Image CreateSpriteImage(string name, Transform parent, Sprite sprite, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject imageObject = CreateRectObject(name, size, parent);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;

        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = sprite == null ? new Color(1f, 1f, 1f, 0.001f) : Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateSettingsLabel(Transform parent, string name, string value, Vector2 anchoredPosition)
    {
        Text label = CreateText(name, parent, value, 27, TextAnchor.MiddleCenter);
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(132f, 42f);
        rect.anchoredPosition = anchoredPosition;
        label.color = new Color(0.26f, 0.23f, 0.16f);
        label.fontStyle = FontStyle.Bold;
        return label;
    }

    private static Slider CreateVolumeSlider(Transform parent, string name, Vector2 anchoredPosition)
    {
        GameObject sliderObject = CreateRectObject(name, new Vector2(322f, 50f), parent);
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = anchoredPosition;

        Image inputArea = sliderObject.AddComponent<Image>();
        inputArea.color = new Color(1f, 1f, 1f, 0.001f);
        inputArea.raycastTarget = true;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;

        GameObject background = CreateRectObject("Background", Vector2.zero, sliderObject.transform);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        Stretch(backgroundRect);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.sprite = LoadNewUiSprite("Recource/자산 10");
        backgroundImage.color = Color.white;
        backgroundImage.preserveAspect = false;
        backgroundImage.raycastTarget = false;

        GameObject handleArea = CreateRectObject("Handle Slide Area", Vector2.zero, sliderObject.transform);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        Stretch(handleAreaRect);
        handleAreaRect.offsetMin = new Vector2(26f, 0f);
        handleAreaRect.offsetMax = new Vector2(-26f, 0f);

        GameObject handle = CreateRectObject("Handle", new Vector2(34f, 52f), handleArea.transform);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = LoadNewUiSprite("Recource/자산 9");
        handleImage.color = Color.white;
        handleImage.preserveAspect = true;
        handleImage.raycastTarget = true;

        slider.targetGraphic = handleImage;
        slider.handleRect = handle.GetComponent<RectTransform>();
        return slider;
    }

    private static Button AddSpriteButton(Transform parent, string name, Sprite sprite, Vector2 size, UnityAction action, Vector2 anchoredPosition)
    {
        GameObject buttonObject = CreateRectObject(name, size, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = sprite == null ? new Color(1f, 1f, 1f, 0.001f) : Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }

    private static Button AddTransparentButton(Transform parent, string name, Vector2 size, UnityAction action, Vector2 anchoredPosition)
    {
        GameObject buttonObject = CreateRectObject(name, size, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.001f);
        image.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }

    private static Sprite LoadNewUiSprite(string name)
    {
        string path = NewUiFolder + "/" + name;
        if (!path.EndsWith(".png"))
        {
            path += ".png";
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        string[] guids = AssetDatabase.FindAssets(name + " t:Sprite", new[] { NewUiFolder });
        for (int i = 0; i < guids.Length; i++)
        {
            string guidPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(guidPath);
            if (sprite != null)
            {
                return sprite;
            }
        }

        Debug.LogWarning("New UI sprite not found: " + name);
        return null;
    }

    private static Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name);
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
