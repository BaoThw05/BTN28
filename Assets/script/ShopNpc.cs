using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>Gắn lên NPC bán hàng có Collider. Bấm chuột trái vào NPC để mở shop.</summary>
[RequireComponent(typeof(Collider))]
public class ShopNpc : MonoBehaviour
{
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
        bool pointingAtNpc = IsPointerOverNpc();
        SetPromptVisible(pointingAtNpc);

        if (pointingAtNpc && Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
            var ui = shopUI != null ? shopUI : ShopUI.Instance;
            if (ui != null) ui.Open();
            else Debug.LogWarning("Chưa có ShopUI trong scene.", this);
        }
    }

    private bool IsPointerOverNpc()
    {
        Camera cam = ClickCamera;
        if (cam == null) return false;

        // Khi đang chơi góc nhìn thứ ba, cursor thường bị khóa giữa màn hình.
        // Ngắm bằng tâm màn hình giúp không phụ thuộc vị trí cursor ẩn.
        Vector3 screenPoint = Cursor.lockState == CursorLockMode.Locked
            ? new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
            : Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(screenPoint);
        if (!Physics.Raycast(ray, out RaycastHit hit, clickDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return false;

        // Collider thường nằm ở object con của model NPC, nên tìm ShopNpc ở các parent.
        return hit.collider.GetComponentInParent<ShopNpc>() == this;
    }

    private void EnsurePrompt()
    {
        if (promptText != null)
        {
            if (promptCanvas == null) promptCanvas = promptText.GetComponentInParent<Canvas>();
            if (promptText.text != promptMessage) promptText.text = promptMessage;
            return;
        }

        var canvasObject = new GameObject("Shop Prompt Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        promptCanvas = canvasObject.GetComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.WorldSpace;
        promptCanvas.transform.SetParent(transform, false);
        promptCanvas.transform.localPosition = Vector3.up * promptHeight;
        promptCanvas.transform.localRotation = Quaternion.identity;
        promptCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 60);
        promptCanvas.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;

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
