using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
internal static class FridgeUISceneBuilder
{
    private const string KitchenScenePath = "Assets/Scenes/Kitchen.unity";

    static FridgeUISceneBuilder()
    {
        EditorApplication.update += TryBuildInKitchen;
    }

    private static void TryBuildInKitchen()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying)
            return;

        EditorApplication.update -= TryBuildInKitchen;
        Scene scene = EditorSceneManager.GetSceneByPath(KitchenScenePath);
        bool openedAdditively = false;
        if (!scene.IsValid() || !scene.isLoaded)
        {
            scene = EditorSceneManager.OpenScene(KitchenScenePath, OpenSceneMode.Additive);
            openedAdditively = true;
        }

        bool changed = false;
        GameObject canvasObject = FindInScene(scene, "Canvas_Legacy");
        if (canvasObject == null)
        {
            canvasObject = FindInScene(scene, "Canvas");
            if (canvasObject != null)
            {
                canvasObject.name = "Canvas_Legacy";
                changed = true;
            }
        }
        if (canvasObject == null)
        {
            canvasObject = BuildCanvas(scene);
            changed = true;
        }

        if (!canvasObject.activeSelf)
        {
            canvasObject.SetActive(true);
            changed = true;
        }
        Transform panel = canvasObject.transform.Find("FridgeUIPanel");
        if (panel != null && !panel.gameObject.activeSelf)
        {
            panel.gameObject.SetActive(true);
            changed = true;
        }

        GameObject duplicateCanvas = FindInScene(scene, "FridgeInventoryCanvas");
        if (duplicateCanvas != null)
        {
            Object.DestroyImmediate(duplicateCanvas);
            changed = true;
        }

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Using the upper Canvas_Legacy for the fridge inventory UI.");
        }

        if (openedAdditively)
            EditorSceneManager.CloseScene(scene, true);
    }

    private static GameObject BuildCanvas(Scene scene)
    {
        GameObject canvasObject = new GameObject("Canvas_Legacy", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        if (canvasObject.scene != scene)
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
        canvasObject.transform.localScale = Vector3.one;
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null)
            canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform panel = CreateRect("FridgeUIPanel", canvasObject.transform,
            Vector2.zero, new Vector2(850f, 510f));
        panel.gameObject.AddComponent<Image>().color = new Color(0.09f, 0.13f, 0.15f, 0.97f);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(850f, 510f);
        CreateText("Title", panel, "TU LANH", 32f, FontStyles.Bold, new Vector2(0f, 205f),
            new Vector2(500f, 55f), TextAlignmentOptions.Center);

        RectTransform close = CreateRect("CloseButton", panel, new Vector2(375f, 205f), new Vector2(48f, 48f));
        Image closeImage = close.gameObject.AddComponent<Image>();
        closeImage.color = new Color(0.65f, 0.2f, 0.16f, 1f);
        close.gameObject.AddComponent<Button>().targetGraphic = closeImage;
        CreateText("X", close, "X", 25f, FontStyles.Bold, Vector2.zero, new Vector2(48f, 48f),
            TextAlignmentOptions.Center);

        string[] names = { "RawMeat", "Lettuce", "Tomato", "Onion", "Cheese" };
        string[] labels = { "Thit song", "Rau xa lach", "Ca chua", "Hanh tay", "Pho mai" };
        float cardWidth = 148f;
        float gap = 12f;
        float totalWidth = names.Length * cardWidth + (names.Length - 1) * gap;
        float firstX = -totalWidth * 0.5f + cardWidth * 0.5f;

        for (int i = 0; i < names.Length; i++)
        {
            RectTransform card = CreateRect("Slot_" + names[i], panel,
                new Vector2(firstX + i * (cardWidth + gap), -5f), new Vector2(cardWidth, 285f));
            Image cardImage = card.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.19f, 0.25f, 0.27f, 1f);
            card.gameObject.AddComponent<Button>().targetGraphic = cardImage;

            RectTransform icon = CreateRect("Icon", card, new Vector2(0f, 45f), new Vector2(116f, 116f));
            RawImage iconImage = icon.gameObject.AddComponent<RawImage>();
            iconImage.color = new Color(0.75f, 0.8f, 0.78f, 1f);
            iconImage.raycastTarget = false;
            CreateText("Name", card, labels[i], 19f, FontStyles.Bold, new Vector2(0f, -48f),
                new Vector2(cardWidth - 12f, 35f), TextAlignmentOptions.Center);
            CreateText("Count", card, "x5", 20f, FontStyles.Normal, new Vector2(0f, -94f),
                new Vector2(cardWidth - 12f, 38f), TextAlignmentOptions.Center);
        }
        return canvasObject;
    }

    private static GameObject FindInScene(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == objectName)
                    return child.gameObject;
            }
        }
        return null;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, float size,
        FontStyles style, Vector2 position, Vector2 dimensions, TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect(name, parent, position, dimensions);
        TMP_Text text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
        return text;
    }
}
