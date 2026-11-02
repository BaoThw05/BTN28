using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>Gắn lên NPC bán hàng có Collider. Bấm chuột trái vào NPC để mở shop.</summary>
[RequireComponent(typeof(Collider))]
public class ShopNpc : MonoBehaviour
{
    private const float MinimumClickDistance = 12f;
    private const float MaximumRayDistance = 100f;
    private static int lastHandledClickFrame = -1;

    [SerializeField] private ShopUI shopUI;
    [SerializeField] private Camera playerCamera;
    [SerializeField, Min(1f)] private float clickDistance = 10f;
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string promptMessage = "Bấm để mở shop";
    [SerializeField] private float promptHeight = 2.2f;

    private Camera ClickCamera => playerCamera != null ? playerCamera : Camera.main;

    private void Awake()
    {
        EnsurePrompt();
        SetPromptVisible(false);
    }

    private void Update()
    {
        bool pointingAtNpc = FindAimedShopNpc(ClickCamera) == this;
        SetPromptVisible(pointingAtNpc);

        if (pointingAtNpc && Input.GetMouseButtonDown(0))
        {
            if (Cursor.lockState != CursorLockMode.Locked &&
                UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
            TryOpenShopAtCamera(ClickCamera);
        }
    }

    /// <summary>Được gọi cả từ PlayerOrderInteraction để shop nhận click trước thao tác lấy đồ.</summary>
    public static bool TryOpenShopAtCamera(Camera cam)
    {
        if (cam == null || !Input.GetMouseButtonDown(0)) return false;
        if (lastHandledClickFrame == Time.frameCount) return true;

        ShopNpc npc = FindAimedShopNpc(cam);
        if (npc == null) return false;
        if (Cursor.lockState != CursorLockMode.Locked &&
            UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return false;

        ShopUI ui = npc.shopUI != null ? npc.shopUI : ShopUI.Instance;
        if (ui == null)
        {
            Debug.LogWarning("Chưa có ShopUI trong scene.", npc);
            return false;
        }

        lastHandledClickFrame = Time.frameCount;
        ui.Open();
        return true;
    }

    private static ShopNpc FindAimedShopNpc(Camera cam)
    {
        if (cam == null) return null;

        // Khi đang chơi góc nhìn thứ ba, cursor thường bị khóa giữa màn hình.
        // Ngắm bằng tâm màn hình giúp không phụ thuộc vị trí cursor ẩn.
        Vector3 screenPoint = Cursor.lockState == CursorLockMode.Locked
            ? new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
            : Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(screenPoint);
        RaycastHit[] hits = Physics.RaycastAll(ray, MaximumRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            // Bỏ qua collider nhân vật/camera để không tự chặn raycast.
            if (hit.collider.transform == cam.transform.root ||
                hit.collider.transform.IsChildOf(cam.transform.root) ||
                hit.collider.GetComponentInParent<PlayerMovement>() != null ||
                hit.collider.GetComponentInParent<PlayerOrderInteraction>() != null)
                continue;

            // Collider có thể nằm ở object con của model NPC.
            ShopNpc npc = hit.collider.GetComponentInParent<ShopNpc>();
            if (npc != null)
                return hit.distance <= Mathf.Max(MinimumClickDistance, npc.clickDistance) ? npc : null;
        }

        return null;
    }

    private void EnsurePrompt()
    {
        if (promptText != null)
        {
            if (promptCanvas == null) promptCanvas = promptText.GetComponentInParent<Canvas>();
            if (promptText.text != promptMessage) promptText.text = promptMessage;
            return;
        }

        if (promptCanvas == null || !promptCanvas.transform.IsChildOf(transform))
        {
            var canvasObject = new GameObject("Shop Prompt Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            promptCanvas = canvasObject.GetComponent<Canvas>();
            promptCanvas.transform.SetParent(transform, false);
            promptCanvas.transform.localPosition = Vector3.up * promptHeight;
            promptCanvas.transform.localRotation = Quaternion.identity;
            promptCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 60);
            promptCanvas.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;
        }
        promptCanvas.renderMode = RenderMode.WorldSpace;

        var textObject = new GameObject("Prompt Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(promptCanvas.transform, false);
        var rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        promptText = textObject.GetComponent<TextMeshProUGUI>();
        promptText.text = promptMessage;
        promptText.fontSize = 30;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = Color.white;
        promptText.outlineWidth = 0.2f;
        promptText.outlineColor = Color.black;
        promptText.raycastTarget = false;
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptCanvas != null && promptCanvas.gameObject.activeSelf != visible)
            promptCanvas.gameObject.SetActive(visible);
    }

    private void LateUpdate()
    {
        if (promptCanvas == null || !promptCanvas.gameObject.activeSelf || ClickCamera == null) return;
        promptCanvas.transform.rotation = Quaternion.LookRotation(
            promptCanvas.transform.position - ClickCamera.transform.position,
            ClickCamera.transform.up);
    }
}
