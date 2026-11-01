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

    private readonly Dictionary<ShopItem, int> inventory = new Dictionary<ShopItem, int>();
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
        if (!CanAfford(item)) return false;
        Money -= item.price;
        inventory[item] = inventory.TryGetValue(item, out int count) ? count + 1 : 1;
        Changed?.Invoke();
        return true;
    }

    public int GetItemCount(ShopItem item) => item != null && inventory.TryGetValue(item, out int count) ? count : 0;

    public bool TryAddMoney(int amount)
    {
        if (amount < 0 || Money > int.MaxValue - amount) return false;
        Money += amount;
        Changed?.Invoke();
        return true;
    }
}
