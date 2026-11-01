using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [SerializeField] private string foodName = "Burger";
    [SerializeField] private bool requiresCooking;
    [SerializeField] private string cookedFoodName = "Burger";
    [SerializeField] private float cookDuration = 5f;
    [SerializeField] private bool containsBurntMeat;

    public string FoodName => foodName;
    public bool RequiresCooking => requiresCooking;
    public string CookedFoodName => cookedFoodName;
    public float CookDuration => cookDuration;
    public bool ContainsBurntMeat => containsBurntMeat;

    public void SetFoodName(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            foodName = value;
    }

    public void SetContainsBurntMeat(bool value)
    {
        containsBurntMeat = value;
    }

    public void PickUp()
    {
        foreach (Renderer itemRenderer in GetComponentsInChildren<Renderer>(true))
            itemRenderer.enabled = false;

        foreach (Collider itemCollider in GetComponentsInChildren<Collider>(true))
            itemCollider.enabled = false;
    }
}
