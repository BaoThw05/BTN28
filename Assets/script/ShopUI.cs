using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Tạo giao diện shop và xử lý mua hàng. Có thể dùng một ShopUI cho mọi NPC.</summary>
public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [SerializeField] private ShopItem[] items;
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;
    [Tooltip("Canvas UI có sẵn để chứa shop. Để trống thì ShopUI tự tạo Canvas.")]
    [SerializeField] private GameObject optionCanvas;
    [Tooltip("TMP_Text trên HUD để luôn hiển thị tiền. Nếu để trống, script tìm object tên moneyText.")]
    [SerializeField] private TMP_Text hudMoneyText;
    [SerializeField] private Text hudLegacyMoneyText;

    private GameObject panel;
    private TMP_Text moneyText;
    private readonly List<(ShopItem item, TMP_Text price)> rows = new List<(ShopItem, TMP_Text)>();
    private bool isOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        FindHudMoneyText();
        BuildUI();
        Close();
        Refresh();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (PlayerEconomy.Instance != null) PlayerEconomy.Instance.Changed -= Refresh;
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(closeKey)) Close();
        UpdateHudMoney();
    }

    public void Open()
    {
        if (panel == null) BuildUI();
        if (optionCanvas != null) optionCanvas.gameObject.SetActive(true);
        panel.SetActive(true);
        isOpen = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Refresh();
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        isOpen = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void BuildUI()
    {
        if (optionCanvas == null)
        {
            var canvasObject = new GameObject("Shop Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            optionCanvas = canvasObject;
            optionCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            optionCanvas.GetComponent<Canvas>().sortingOrder = 100;
        }
        optionCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        optionCanvas.GetComponent<Canvas>().sortingOrder = Mathf.Max(optionCanvas.GetComponent<Canvas>().sortingOrder, 100);

        var scaler = optionCanvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        if (optionCanvas.GetComponent<GraphicRaycaster>() == null)
            optionCanvas.gameObject.AddComponent<GraphicRaycaster>();

        // Bảng chiếm khu vực bên phải và co theo chiều cao màn hình.
        panel = MakeRect("Shop Panel", optionCanvas.transform, new Vector2(1f, 0.04f), new Vector2(1f, 0.96f), new Vector2(470, 0));
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-20f, 0f);
        panel.AddComponent<Image>().color = new Color(0.10f, 0.12f, 0.16f, 0.97f);

        var title = MakeText("Cửa hàng", panel.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 74), 34, TextAlignmentOptions.Center, Color.white);
        title.rectTransform.anchoredPosition = new Vector2(0, -42);
        moneyText = MakeText("Tiền", panel.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -62), 24, TextAlignmentOptions.Center, new Color(1f, .85f, .25f));
        moneyText.rectTransform.anchoredPosition = new Vector2(0, -91);

        var close = MakeButton("Đóng", panel.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(100, 48));
        close.GetComponent<RectTransform>().anchoredPosition = new Vector2(-62, -32);
        close.onClick.AddListener(Close);

        var list = MakeRect("Item List", panel.transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero);
        var listRect = list.GetComponent<RectTransform>();
        listRect.offsetMin = new Vector2(14, 18);
        listRect.offsetMax = new Vector2(-14, -135);
        var layout = list.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        rows.Clear();

        if (items != null)
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                var row = MakeRect(item.itemName, list.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 76));
                row.AddComponent<Image>().color = new Color(.18f, .21f, .27f, 1f);
                var name = MakeText("Name", row.transform, new Vector2(0, 0), new Vector2(.43f, 1), Vector2.zero, 19, TextAlignmentOptions.MidlineLeft, Color.white);
                name.text = item.itemName;
                name.margin = new Vector4(16, 0, 0, 0);
                var price = MakeText("Price", row.transform, new Vector2(.43f, 0), new Vector2(.75f, 1), Vector2.zero, 17, TextAlignmentOptions.Center, Color.white);
                var buy = MakeButton("Mua", row.transform, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(105, 48));
                buy.GetComponent<RectTransform>().anchoredPosition = new Vector2(-53, 0);
                buy.onClick.AddListener(() => Buy(item));
                rows.Add((item, price));
            }
        }
        var evt = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (evt == null) new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        if (PlayerEconomy.Instance != null) PlayerEconomy.Instance.Changed += Refresh;
    }

    private void Buy(ShopItem item)
    {
        if (PlayerEconomy.Instance == null || !PlayerEconomy.Instance.TryPurchase(item)) { Refresh(); return; }
        Refresh();
    }

    private void Refresh()
    {
        if (PlayerEconomy.Instance == null) return;
        if (moneyText != null) moneyText.text = $"Tiền: {PlayerEconomy.Instance.Money:N0}";
        UpdateHudMoney();
        foreach (var row in rows)
            if (row.price != null)
            {
                bool canBuy = PlayerEconomy.Instance.CanAfford(row.item);
                row.price.text = $"{row.item.price:N0}  (x{PlayerEconomy.Instance.GetItemCount(row.item)})";
                row.price.color = canBuy ? Color.white : new Color(1f, .2f, .2f);
            }
    }

    private void FindHudMoneyText()
    {
        if (hudMoneyText == null)
        {
            TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                if (string.Equals(text.gameObject.name, "moneyText", System.StringComparison.OrdinalIgnoreCase))
                {
                    hudMoneyText = text;
                    break;
                }
            }
        }
        if (hudLegacyMoneyText == null)
        {
            Text[] texts = FindObjectsOfType<Text>(true);
            foreach (Text text in texts)
            {
                if (string.Equals(text.gameObject.name, "moneyText", System.StringComparison.OrdinalIgnoreCase))
                {
                    hudLegacyMoneyText = text;
                    break;
                }
            }
        }
    }

    private void UpdateHudMoney()
    {
        if (hudMoneyText != null && PlayerEconomy.Instance != null)
            hudMoneyText.text = $"{PlayerEconomy.Instance.Money:N0}";
        if (hudLegacyMoneyText != null && PlayerEconomy.Instance != null)
            hudLegacyMoneyText.text = $"{PlayerEconomy.Instance.Money:N0}";
    }

    private TMP_Text MakeText(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        var go = MakeRect(objectName, parent, anchorMin, anchorMax, size);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.text = objectName;
        text.raycastTarget = false;
        return text;
    }

    private Button MakeButton(string label, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        var go = MakeRect(label + " Button", parent, anchorMin, anchorMax, size);
        go.AddComponent<Image>().color = new Color(.24f, .48f, .78f);
        var button = go.AddComponent<Button>();
        var text = MakeText("Label", go.transform, Vector2.zero, Vector2.one, Vector2.zero, 18, TextAlignmentOptions.Center, Color.white);
        text.text = label;
        return button;
    }

    private GameObject MakeRect(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = size;
        return go;
    }
}
