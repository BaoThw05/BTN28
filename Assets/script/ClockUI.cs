using UnityEngine;

/// <summary>Cho kim đồng hồ UI chạy theo thời gian từ TimeController.</summary>
public class ClockUI : MonoBehaviour
{
    [SerializeField] private Transform clockHandTransform;
    [SerializeField] private bool rotateTwicePerDay = true;

    private void Awake()
    {
        if (clockHandTransform == null)
            clockHandTransform = FindDeepChild(transform, "ClockHand");
    }

    private void Update()
    {
        if (clockHandTransform == null || TimeController.Instance == null) return;

        float rotationsPerDay = rotateTwicePerDay ? 2f : 1f;
        float angle = -TimeController.Instance.NormalizedDay * 360f * rotationsPerDay;
        clockHandTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (string.Equals(child.name, childName, System.StringComparison.OrdinalIgnoreCase)) return child;
            Transform result = FindDeepChild(child, childName);
            if (result != null) return result;
        }
        return null;
    }
}
