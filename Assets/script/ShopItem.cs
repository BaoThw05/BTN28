using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Game/Shop Item")]
public class ShopItem : ScriptableObject
{
    [Tooltip("ID duy nhất của vật phẩm; tự tạo nếu để trống.")]
    public string itemId;
    public string itemName = "Món hàng";
    [Min(0)] public int price = 10;
    public Sprite icon;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(itemId))
            itemId = Guid.NewGuid().ToString("N");
    }
}
