using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FridgeUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    private class ItemSlot
    {
        public string itemName;
        public string label;
        public GameObject prefab;
        public int count = 5;
        public Button button;
        public TMP_Text countText;
        public RawImage icon;
    }

    private const int IconLayer = 30;
    private readonly List<ItemSlot> slots = new List<ItemSlot>();
    private readonly List<RenderTexture> iconTextures = new List<RenderTexture>();
    private FridgeInteraction fridge;
    private PlayerOrderInteraction player;
    private Camera playerCamera;
    private GameObject canvasObject;
    private GameObject panelObject;
    private MouseMovement mouseLook;
    private PlayerMovement playerMovement;
    private bool wasMouseLookEnabled;
    private bool wasMovementEnabled;
    private bool initialized;

    public bool IsVisible => panelObject != null && panelObject.activeSelf;

    public void Initialize(FridgeInteraction fridgeInteraction, PlayerOrderInteraction orderPlayer,
        Camera cameraToUse, string[] itemNames, GameObject[] itemPrefabs)
    {
        if (initialized)
            return;

        fridge = fridgeInteraction;
        player = orderPlayer;
        playerCamera = cameraToUse;
        if (playerCamera == null || itemNames == null || itemPrefabs == null)
            return;

        int count = Mathf.Min(itemNames.Length, itemPrefabs.Length);
        for (int i = 0; i < count; i++)
        {
            if (itemPrefabs[i] == null)
                continue;
            slots.Add(new ItemSlot
            {
                itemName = itemNames[i],
                label = GetLabel(itemNames[i]),
                prefab = itemPrefabs[i]
            });
        }

        BuildUI();
        initialized = true;
    }

    private void Update()
    {
        if (IsVisible && Input.GetKeyDown(KeyCode.Escape))
            CloseFridgeUI();
    }

    public void OpenFridgeUI()
    {
        if (!initialized || panelObject == null)
            return;

        panelObject.SetActive(true);
        IsOpen = true;
        SetPlayerControls(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseFridgeUI()
    {
        if (panelObject != null)
            panelObject.SetActive(false);
        IsOpen = false;
        if (fridge != null)
            fridge.CloseFridge();
        SetPlayerControls(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void BuildUI()
    {
        EnsureEventSystem();
        canvasObject = GameObject.Find("Canvas_Legacy");
        if (canvasObject == null)
            canvasObject = new GameObject("Canvas_Legacy", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.SetActive(true);
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

        Transform existingPanel = canvasObject.transform.Find("FridgeUIPanel");
        if (existingPanel == null)
        {
            panelObject = CreateRect("FridgeUIPanel", canvasObject.transform).gameObject;
            RectTransform newPanel = panelObject.GetComponent<RectTransform>();
            SetRect(newPanel, Vector2.zero, new Vector2(850f, 510f));
            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0.09f, 0.13f, 0.15f, 0.97f);
        }
        else
        {
            panelObject = existingPanel.gameObject;
        }
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(850f, 510f);
        Transform oldButton = panel.Find("Button");
        if (oldButton != null)
            Destroy(oldButton.gameObject);

        if (panel.Find("Title") == null)
        {
            TMP_Text title = CreateText("Title", panel, "FRIDGE", 32f, FontStyles.Bold,
                new Vector2(0f, 205f), new Vector2(500f, 55f), TextAlignmentOptions.Center);
            title.color = Color.white;
        }

        CreateCloseButton(panel);
        BuildItemSlots(panel);

        panelObject.SetActive(false);
    }

    private void BuildItemSlots(RectTransform panel)
    {
        float cardWidth = 148f;
        float cardHeight = 285f;
        float gap = 12f;
        float totalWidth = slots.Count * cardWidth + Mathf.Max(0, slots.Count - 1) * gap;
        float firstX = -totalWidth * 0.5f + cardWidth * 0.5f;

        for (int i = 0; i < slots.Count; i++)
        {
            ItemSlot slot = slots[i];
            Transform existingCard = panel.Find("Slot_" + slot.itemName);
            RectTransform card;
            if (existingCard != null)
            {
                card = existingCard.GetComponent<RectTransform>();
            }
            else
            {
                card = CreateRect("Slot_" + slot.itemName, panel);
                SetRect(card, new Vector2(firstX + i * (cardWidth + gap), -5f), new Vector2(cardWidth, cardHeight));
                Image cardBackground = card.gameObject.AddComponent<Image>();
                cardBackground.color = new Color(0.19f, 0.25f, 0.27f, 1f);
                card.gameObject.AddComponent<Button>();
                RectTransform iconPlaceholder = CreateRect("Icon", card);
                SetRect(iconPlaceholder, new Vector2(0f, 45f), new Vector2(116f, 116f));
                iconPlaceholder.gameObject.AddComponent<RawImage>().raycastTarget = false;
                CreateText("Name", card, slot.label, 19f, FontStyles.Bold,
                    new Vector2(0f, -48f), new Vector2(cardWidth - 12f, 35f), TextAlignmentOptions.Center);
                CreateText("Count", card, "x" + slot.count, 20f, FontStyles.Normal,
                    new Vector2(0f, -94f), new Vector2(cardWidth - 12f, 38f), TextAlignmentOptions.Center);
            }
            Image background = card.GetComponent<Image>();
            slot.button = card.GetComponent<Button>();
            if (slot.button == null)
                slot.button = card.gameObject.AddComponent<Button>();
            slot.button.targetGraphic = background;
            slot.button.onClick.AddListener(() => SelectItem(slot));

            Transform iconTransform = card.Find("Icon");
            if (iconTransform == null)
            {
                RectTransform iconRect = CreateRect("Icon", card);
                SetRect(iconRect, new Vector2(0f, 45f), new Vector2(116f, 116f));
                iconTransform = iconRect;
            }
            slot.icon = iconTransform.GetComponent<RawImage>();
            if (slot.icon == null)
                slot.icon = iconTransform.gameObject.AddComponent<RawImage>();
            slot.icon.raycastTarget = false;
            slot.icon.texture = CreateIngredientIcon(slot.prefab);
            slot.icon.color = Color.white;

            TMP_Text label = card.Find("Name").GetComponent<TMP_Text>();
            label.text = slot.label;
            label.color = Color.white;
            slot.countText = card.Find("Count").GetComponent<TMP_Text>();
            slot.countText.text = "x" + slot.count;
            slot.countText.color = new Color(0.95f, 0.82f, 0.4f);
        }
    }

    private void CreateCloseButton(RectTransform panel)
    {
        Transform existing = panel.Find("CloseButton");
        if (existing != null)
        {
            Button existingButton = existing.GetComponent<Button>();
            if (existingButton == null)
                existingButton = existing.gameObject.AddComponent<Button>();
            existingButton.onClick.AddListener(CloseFridgeUI);
            return;
        }
        RectTransform closeRect = CreateRect("CloseButton", panel);
        SetRect(closeRect, new Vector2(375f, 205f), new Vector2(48f, 48f));
        Image image = closeRect.gameObject.AddComponent<Image>();
        image.color = new Color(0.65f, 0.2f, 0.16f, 1f);
        Button button = closeRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(CloseFridgeUI);
        CreateText("X", closeRect, "X", 25f, FontStyles.Bold, Vector2.zero,
            new Vector2(48f, 48f), TextAlignmentOptions.Center);
    }

    private Texture CreateIngredientIcon(GameObject prefab)
    {
        UnityEngine.Object spawned = UnityEngine.Object.Instantiate((UnityEngine.Object)prefab);
        GameObject model = spawned as GameObject;
        if (model == null)
        {
            if (spawned != null)
                Destroy(spawned);
            Debug.LogError("Ingredient icon source is not a GameObject: " + prefab.name, this);
            return Texture2D.whiteTexture;
        }

        Vector3 iconOrigin = new Vector3(2000f + iconTextures.Count * 5f, 2000f, 2000f);
        model.transform.position = iconOrigin;
        SetLayerRecursively(model.transform, IconLayer);
        if (!TryGetRendererBounds(model.transform, out Bounds bounds))
            bounds = new Bounds(iconOrigin, Vector3.one * 0.5f);

        RenderTexture iconTexture = new RenderTexture(192, 192, 16, RenderTextureFormat.ARGB32);
        iconTexture.name = "FridgeIcon_" + prefab.name;
        iconTexture.Create();
        iconTextures.Add(iconTexture);

        GameObject cameraObject = new GameObject("FridgeIconCamera_" + prefab.name);
        Camera iconCamera = cameraObject.AddComponent<Camera>();
        iconCamera.clearFlags = CameraClearFlags.SolidColor;
        iconCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        iconCamera.cullingMask = 1 << IconLayer;
        iconCamera.orthographic = true;
        iconCamera.aspect = 1f;
        iconCamera.nearClipPlane = 0.01f;
        iconCamera.farClipPlane = 20f;
        iconCamera.targetTexture = iconTexture;
        iconCamera.transform.position = bounds.center + new Vector3(0.8f, 0.65f, -1.6f);
        iconCamera.transform.LookAt(bounds.center);
        iconCamera.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.85f;

        GameObject lightObject = new GameObject("FridgeIconLight_" + prefab.name);
        Light iconLight = lightObject.AddComponent<Light>();
        iconLight.type = LightType.Directional;
        iconLight.intensity = 1.2f;
        iconLight.cullingMask = 1 << IconLayer;
        lightObject.transform.rotation = Quaternion.Euler(35f, -35f, 0f);

        iconCamera.Render();
        iconCamera.targetTexture = null;
        Destroy(cameraObject);
        Destroy(lightObject);
        Destroy(model);
        return iconTexture;
    }

    private void SelectItem(ItemSlot slot)
    {
        if (slot.count <= 0 || player == null)
            return;
        if (!player.TryPickUpFridgeIngredient(slot.itemName, slot.prefab))
            return;

        slot.count--;
        slot.countText.text = "x" + slot.count;
        slot.button.interactable = slot.count > 0;
        CloseFridgeUI();
    }

    private void SetPlayerControls(bool enabled)
    {
        if (playerCamera != null)
        {
            if (mouseLook == null)
                mouseLook = playerCamera.GetComponent<MouseMovement>();
            if (mouseLook != null)
            {
                if (!enabled)
                    wasMouseLookEnabled = mouseLook.enabled;
                mouseLook.enabled = enabled && wasMouseLookEnabled;
            }
        }

        if (player != null)
        {
            if (playerMovement == null)
                playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                if (!enabled)
                    wasMovementEnabled = playerMovement.enabled;
                playerMovement.enabled = enabled && wasMovementEnabled;
            }
        }
    }

    private static RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject item = new GameObject(objectName, typeof(RectTransform));
        item.transform.SetParent(parent, false);
        return item.GetComponent<RectTransform>();
    }

    private static TMP_Text CreateText(string objectName, Transform parent, string value,
        float size, FontStyles style, Vector2 position, Vector2 dimensions, TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect(objectName, parent);
        SetRect(rect, position, dimensions);
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

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 dimensions)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(eventSystem);
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        foreach (Transform child in root)
            SetLayerRecursively(child, layer);
    }

    private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            bounds = new Bounds(root.position, Vector3.zero);
            return false;
        }
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return true;
    }

    private static string GetLabel(string itemName)
    {
        switch (itemName)
        {
            case "RawMeat": return "Thit song";
            case "Lettuce": return "Rau xa lach";
            case "Tomato": return "Ca chua";
            case "Onion": return "Hanh tay";
            case "Cheese": return "Pho mai";
            default: return itemName;
        }
    }

    private void OnDestroy()
    {
        if (IsOpen)
        {
            IsOpen = false;
            SetPlayerControls(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        foreach (RenderTexture texture in iconTextures)
        {
            if (texture != null)
            {
                texture.Release();
                Destroy(texture);
            }
        }
    }
}
