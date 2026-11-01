using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Game/Shop Item")]
public class ShopItem : ScriptableObject
{
    public string itemName = "Món hàng";
    [Min(0)] public int price = 10;
    public Sprite icon;
}
