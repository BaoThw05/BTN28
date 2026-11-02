using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Mỗi slot hiển thị một ShopItem khác nhau và số lượng đang sở hữu.</summary>
public class InventoryDisplay : MonoBehaviour
{
    [Tooltip("Object cha chứa trực tiếp slot1, slot2... Để trống sẽ dùng object gắn script.")]
    [SerializeField] private Transform slotsRoot;

    private sealed class SlotView
    {
        public GameObject root;
        public Item itemView;
        public Image itemImage;
        public TMP_Text itemName;
        public TMP_Text quantity;
    }

    private readonly List<SlotView> slots = new List<SlotView>();
    private PlayerEconomy subscribedEconomy;

    private void Awake()
    {
        CacheSlots();
    }

    private void OnEnable()
    {
        BindEconomy();
        Refresh();
    }

    private void Update()
    {
        // PlayerEconomy có thể được tạo sau InventoryDisplay khi scene bắt đầu.
        BindEconomy();
    }

    private void OnDisable()
    {
        UnbindEconomy();
    }

    private void CacheSlots()
    {
        slots.Clear();
        Transform root = slotsRoot != null ? slotsRoot : transform;

        // Nếu script gắn nhầm vào object con, thử tìm cha có slot1, slot2...
        if (!HasSlotChildren(root) && slotsRoot == null && root.parent != null)
            root = root.parent;

        foreach (Transform child in root)
        {
            if (!IsNumberedSlot(child.name)) continue;

            Transform item = child.Find("item");
            Transform quantity = child.Find("quantity");
            if (item == null || quantity == null)
            {
                Debug.LogWarning($"{child.name} cần có child tên 'item' và 'quantity'.", child);
                continue;
            }

            slots.Add(new SlotView
            {
                root = child.gameObject,
                itemView = item.GetComponent<Item>(),
                itemImage = item.GetComponent<Image>(),
                itemName = item.GetComponent<TMP_Text>(),
                quantity = quantity.GetComponent<TMP_Text>()
            });
        }

        slots.Sort((a, b) => GetSlotNumber(a.root.name).CompareTo(GetSlotNumber(b.root.name)));
        if (slots.Count == 0)
            Debug.LogWarning("InventoryDisplay không tìm thấy slot có child 'item' và 'quantity'.", this);
    }

    private static bool HasSlotChildren(Transform root)
    {
        foreach (Transform child in root)
            if (IsNumberedSlot(child.name)) return true;
        return false;
    }

    private static bool IsNumberedSlot(string objectName)
    {
        return objectName.StartsWith("slot", StringComparison.OrdinalIgnoreCase) &&
               int.TryParse(objectName.Substring(4), out _);
    }

    private static int GetSlotNumber(string objectName)
    {
        return int.TryParse(objectName.Substring(4), out int number) ? number : 0;
    }

    private void BindEconomy()
    {
        PlayerEconomy current = PlayerEconomy.Instance;
        if (subscribedEconomy == current) return;

        UnbindEconomy();
        subscribedEconomy = current;
        if (subscribedEconomy != null)
            subscribedEconomy.Changed += Refresh;
        Refresh();
    }

    private void UnbindEconomy()
    {
        if (subscribedEconomy != null)
            subscribedEconomy.Changed -= Refresh;
        subscribedEconomy = null;
    }

    public void Refresh()
    {
        var ownedItems = new List<KeyValuePair<ShopItem, int>>();
        PlayerEconomy economy = PlayerEconomy.Instance;
        if (economy != null)
        {
            foreach (ShopItem item in economy.InventoryOrder)
            {
                int count = economy.GetItemCount(item);
                if (item != null && count > 0)
                    ownedItems.Add(new KeyValuePair<ShopItem, int>(item, count));
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            SlotView slot = slots[i];
            bool hasItem = i < ownedItems.Count;
            ShopItem item = hasItem ? ownedItems[i].Key : null;
            int quantity = hasItem ? ownedItems[i].Value : 0;

            // Giữ slot/background luôn hiện; chỉ bật phần item và quantity khi slot có món.
            if (slot.itemView != null)
            {
                slot.itemView.SetItem(item, quantity);
            }
            else if (slot.itemImage != null)
            {
                slot.itemImage.sprite = hasItem ? item.icon : null;
                slot.itemImage.enabled = hasItem && item.icon != null;
            }
            if (slot.itemName != null)
            {
                slot.itemName.text = hasItem ? item.itemName : string.Empty;
                slot.itemName.gameObject.SetActive(hasItem);
            }
            if (slot.quantity != null)
            {
                slot.quantity.text = hasItem ? quantity.ToString() : string.Empty;
                slot.quantity.gameObject.SetActive(hasItem);
            }
        }

        if (ownedItems.Count > slots.Count)
            Debug.LogWarning($"Inventory có {ownedItems.Count} món khác nhau nhưng chỉ có {slots.Count} slot.", this);
    }
}
