using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Điều khiển khung chi tiết: nhập số lượng, tính tổng và mua một ShopItem.</summary>
public class ShopItemPurchaseUI : MonoBehaviour
{
    [Header("Item cần bán")]
    [SerializeField] private ShopItem item;

    [Header("UI - có thể gán trực tiếp trong Inspector")]
    [SerializeField] private TMP_InputField quantityInput;
    [SerializeField] private TMP_Text unitPriceText;
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject panelToClose;

    [Header("Màu tổng tiền")]
    [SerializeField] private Color affordableColor = Color.black;
    [SerializeField] private Color unaffordableColor = Color.red;

    private PlayerEconomy subscribedEconomy;
    private bool isUpdatingInput;

    private void OnEnable()
    {
        FindUIReferences();
        if (quantityInput != null)
        {
            quantityInput.onValueChanged.AddListener(OnQuantityChanged);
            quantityInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            if (string.IsNullOrWhiteSpace(quantityInput.text)) quantityInput.text = "1";
        }
        if (buyButton != null) buyButton.onClick.AddListener(Buy);
        if (cancelButton != null) cancelButton.onClick.AddListener(Cancel);
        BindEconomy();
        Refresh();
    }

    private void Update()
    {
        // PlayerEconomy có thể được tạo sau UI khi scene bắt đầu.
        BindEconomy();
    }

    private void OnDisable()
    {
        if (quantityInput != null) quantityInput.onValueChanged.RemoveListener(OnQuantityChanged);
        if (buyButton != null) buyButton.onClick.RemoveListener(Buy);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(Cancel);
        if (subscribedEconomy != null) subscribedEconomy.Changed -= Refresh;
        subscribedEconomy = null;
    }

    private void FindUIReferences()
    {
        if (quantityInput == null) quantityInput = GetComponentInChildren<TMP_InputField>(true);
        if (buyButton == null) buyButton = FindButton("buy");
        if (cancelButton == null) cancelButton = FindButton("cancel");
        if (panelToClose == null) panelToClose = gameObject;

        if (unitPriceText == null) unitPriceText = FindText("price (1)") ?? FindText("price");
        if (totalText == null) totalText = FindText("sum");
    }

    private Button FindButton(string objectName)
    {
        Transform target = FindDeepChild(transform, objectName);
        return target != null ? target.GetComponent<Button>() : null;
    }

    private TMP_Text FindText(string objectName)
    {
        Transform target = FindDeepChild(transform, objectName);
        return target != null ? target.GetComponent<TMP_Text>() : null;
    }

    private static Transform FindDeepChild(Transform parent, string objectName)
    {
        foreach (Transform child in parent)
        {
            if (string.Equals(child.name, objectName, System.StringComparison.OrdinalIgnoreCase)) return child;
            Transform found = FindDeepChild(child, objectName);
            if (found != null) return found;
        }
        return null;
    }

    private void BindEconomy()
    {
        PlayerEconomy current = PlayerEconomy.Instance;
        if (subscribedEconomy == current) return;
        if (subscribedEconomy != null) subscribedEconomy.Changed -= Refresh;
        subscribedEconomy = current;
        if (subscribedEconomy != null) subscribedEconomy.Changed += Refresh;
    }

    private void OnQuantityChanged(string value)
    {
        if (!isUpdatingInput) Refresh();
    }

    public void Refresh()
    {
        int quantity = ReadQuantity();
        long total = item != null && quantity > 0 ? (long)item.price * quantity : 0;
        bool canBuy = PlayerEconomy.Instance != null && item != null && quantity > 0 &&
                      total <= int.MaxValue && PlayerEconomy.Instance.Money >= total;

        if (unitPriceText != null)
            unitPriceText.text = item != null ? item.price.ToString("N0") : "0";

        if (totalText != null)
        {
            totalText.text = total.ToString("N0");
            totalText.color = canBuy ? affordableColor : unaffordableColor;
        }

        if (buyButton != null) buyButton.interactable = canBuy;
    }

    private int ReadQuantity()
    {
        if (quantityInput == null || !int.TryParse(quantityInput.text, out int value)) return 0;
        return Mathf.Max(0, value);
    }

    private void Buy()
    {
        if (item == null || PlayerEconomy.Instance == null) return;
        int quantity = ReadQuantity();
        if (!PlayerEconomy.Instance.TryPurchase(item, quantity))
        {
            Refresh();
            return;
        }

        if (quantityInput != null) quantityInput.text = "1";
        Refresh();
    }

    private void Cancel()
    {
        if (panelToClose != null) panelToClose.SetActive(false);
    }
}
