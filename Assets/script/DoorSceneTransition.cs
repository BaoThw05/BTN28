using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Nhìn vào cửa và click chuột trái để chuyển sang scene được chọn.</summary>
[RequireComponent(typeof(Collider))]
public class DoorSceneTransition : MonoBehaviour
{
    [SerializeField] private string destinationSceneName = "Kitchen";
    [SerializeField] private Camera playerCamera;
    [SerializeField, Min(0.5f)] private float clickDistance = 5f;
    [SerializeField] private LayerMask raycastLayers = Physics.DefaultRaycastLayers;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || string.IsNullOrWhiteSpace(destinationSceneName)) return;
        if (CursorUIState.IsUIOpen) return;
        if (Cursor.lockState != CursorLockMode.Locked && EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject()) return;

        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) return;

        Vector3 screenPoint = Cursor.lockState == CursorLockMode.Locked
            ? new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
            : Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(screenPoint);

        if (!Physics.Raycast(ray, out RaycastHit hit, clickDistance, raycastLayers,
                QueryTriggerInteraction.Ignore)) return;

        // Cho phép script ở object gốc bắt va chạm từ collider của object con.
        if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return;

        if (!Application.CanStreamedLevelBeLoaded(destinationSceneName))
        {
            Debug.LogError($"Scene '{destinationSceneName}' không có trong Build Settings hoặc tên scene sai.", this);
            return;
        }

        CursorUIState.ForceLockCursor();
        SceneManager.LoadScene(destinationSceneName);
    }
}
