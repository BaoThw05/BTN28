using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Tiền và số lượng vật phẩm của người chơi trong phiên chơi hiện tại.</summary>
public class PlayerEconomy : MonoBehaviour
{
    public static PlayerEconomy Instance { get; private set; }

    [Min(0)] public int startingMoney = 100;
    public int Money { get; private set; }
    public IReadOnlyDictionary<ShopItem, int> Inventory => inventory;
    public IReadOnlyList<ShopItem> InventoryOrder => inventoryOrder;

    private readonly Dictionary<ShopItem, int> inventory = new Dictionary<ShopItem, int>();
    private readonly List<ShopItem> inventoryOrder = new List<ShopItem>();
    public event Action Changed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Money = startingMoney;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool CanAfford(ShopItem item) => item != null && item.price >= 0 && Money >= item.price;

    public bool TryPurchase(ShopItem item)
    {
        return TryPurchase(item, 1);
    }

    public bool TryPurchase(ShopItem item, int quantity)
    {
        if (item == null || quantity <= 0 || item.price < 0) return false;

        long totalPrice = (long)item.price * quantity;
        if (totalPrice > int.MaxValue || Money < totalPrice) return false;

        int currentCount = GetItemCount(item);
        if (currentCount > int.MaxValue - quantity) return false;

        if (currentCount == 0) inventoryOrder.Add(item);
        Money -= (int)totalPrice;
        inventory[item] = currentCount + quantity;
        Changed?.Invoke();
        return true;
    }

    public int GetItemCount(ShopItem item) => item != null && inventory.TryGetValue(item, out int count) ? count : 0;

    public bool HasItem(ShopItem item, int quantity = 1)
    {
        return item != null && quantity > 0 && GetItemCount(item) >= quantity;
    }

    /// <summary>Thêm số lượng vật phẩm, ví dụ phần thưởng hoặc vật phẩm nhặt được.</summary>
    public bool TryAddItem(ShopItem item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        int currentCount = GetItemCount(item);
        if (currentCount > int.MaxValue - quantity) return false;

        if (currentCount == 0) inventoryOrder.Add(item);
        inventory[item] = currentCount + quantity;
        Changed?.Invoke();
        return true;
    }

    /// <summary>Xóa số lượng vật phẩm; thất bại nếu inventory không đủ số lượng yêu cầu.</summary>
    public bool TryRemoveItem(ShopItem item, int quantity = 1)
    {
        if (!HasItem(item, quantity)) return false;

        int remaining = GetItemCount(item) - quantity;
        if (remaining == 0)
        {
            inventory.Remove(item);
            inventoryOrder.Remove(item);
        }
        else
            inventory[item] = remaining;

        Changed?.Invoke();
        return true;
    }

    public void ClearInventory()
    {
        if (inventory.Count == 0) return;
        inventory.Clear();
        inventoryOrder.Clear();
        Changed?.Invoke();
    }

    public bool TryAddMoney(int amount)
    {
        if (amount < 0 || Money > int.MaxValue - amount) return false;
        Money += amount;
        Changed?.Invoke();
        return true;
    }

    /// <summary>Dùng khi tạo economy lúc chạy để giữ số tiền khởi đầu của hệ thống cũ.</summary>
    public void InitializeMoney(int amount)
    {
        Money = Mathf.Max(0, amount);
        Changed?.Invoke();
    }
}
