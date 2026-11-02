using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>UI component gắn lên prefab item để hiển thị dữ liệu vật phẩm.</summary>
public class Item : MonoBehaviour
{
    [Header("Thông tin có thể nhập trực tiếp trong Inspector")]
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField, Min(0)] private int quantity;
    [SerializeField] private Sprite icon;

    [Header("Nguồn dữ liệu tùy chọn")]
    [SerializeField] private ShopItem itemData;

    [Header("Thành phần UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text quantityText;

    public string ItemId => itemId;
    public string ItemName => itemName;
    public int Quantity => quantity;
    public ShopItem Data => itemData;

    private void Awake()
    {
        FindUIReferences();
        CopyMissingDataFromShopItem();
        ApplyDisplay(quantity > 0);
    }

    private void OnValidate()
    {
        FindUIReferences();
        CopyMissingDataFromShopItem();
        ApplyDisplay(quantity > 0);
    }

    /// <summary>InventoryDisplay gọi hàm này để gán món và số lượng đang sở hữu.</summary>
    public void SetItem(ShopItem data, int amount)
    {
        itemData = data;
        itemId = data != null ? data.itemId : string.Empty;
        itemName = data != null ? data.itemName : string.Empty;
        icon = data != null ? data.icon : null;
        quantity = Mathf.Max(0, amount);

        bool hasItem = data != null && quantity > 0;
        FindUIReferences();
        ApplyDisplay(hasItem);
        gameObject.SetActive(hasItem);
    }

    private void CopyMissingDataFromShopItem()
    {
        if (itemData == null) return;
        if (string.IsNullOrWhiteSpace(itemId)) itemId = itemData.itemId;
        if (string.IsNullOrWhiteSpace(itemName)) itemName = itemData.itemName;
        if (icon == null) icon = itemData.icon;
    }

    private void ApplyDisplay(bool showQuantity)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (itemNameText != null)
        {
            itemNameText.text = itemName;
            itemNameText.gameObject.SetActive(!string.IsNullOrWhiteSpace(itemName));
        }

        if (quantityText != null)
        {
            quantityText.text = showQuantity ? quantity.ToString() : string.Empty;
            quantityText.gameObject.SetActive(showQuantity);
        }
    }

    private void FindUIReferences()
    {
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
            if (iconImage == null && transform.Find("icon") != null)
                iconImage = transform.Find("icon").GetComponent<Image>();
        }

        if (itemNameText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                if (!text.gameObject.name.ToLowerInvariant().Contains("quantity"))
                {
                    itemNameText = text;
                    break;
                }
            }
        }

        if (quantityText == null)
        {
            Transform quantityTransform = transform.Find("quantity");
            if (quantityTransform == null && transform.parent != null)
                quantityTransform = transform.parent.Find("quantity");
            if (quantityTransform != null)
                quantityText = quantityTransform.GetComponent<TMP_Text>();
        }
    }
}
