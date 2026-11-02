using UnityEngine;

/// <summary>Bật/tắt một giao diện được gán trong Inspector bằng phím Escape.</summary>
public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject escObject;
    [SerializeField] private bool hideOnStart = true;

    private bool isOpen;

    private void Start()
    {
        if (escObject == null)
        {
            Debug.LogWarning("GameController chưa được gán GameObject cần hiện khi bấm Esc.", this);
            return;
        }

        if (hideOnStart) escObject.SetActive(false);
        isOpen = escObject.activeSelf;
        SetOptionExitVisible(isOpen);
        if (isOpen) CursorUIState.RequestFreeCursor(this);
    }

    private void Update()
    {
        // ShopUI cũng có thể bật/tắt OP. Đồng bộ biến nội bộ với trạng thái
        // thật của GameObject để lần nhấn Esc kế tiếp không bị lệch.
        bool shopSessionActive = ShopUI.Instance != null && ShopUI.Instance.HasActiveSession;
        if (shopSessionActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ShopUI.Instance.CloseAll();
                isOpen = false;
                CursorUIState.ForceLockCursor();
            }
            return;
        }

        if (escObject != null && isOpen != escObject.activeSelf)
        {
            isOpen = escObject.activeSelf;
            SetOptionExitVisible(isOpen);
            if (isOpen) CursorUIState.RequestFreeCursor(this);
            else CursorUIState.ReleaseFreeCursor(this);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) ToggleEscObject();
    }

    private void OnDestroy()
    {
        CursorUIState.ReleaseFreeCursor(this);
    }

    public void ToggleEscObject()
    {
        if (escObject == null) return;

        if (!isOpen)
        {
            escObject.SetActive(true);
            SetOptionExitVisible(true);
            CursorUIState.RequestFreeCursor(this);
            isOpen = true;
        }
        else
        {
            CloseEscObject();
        }
    }

    public void CloseEscObject()
    {
        if (escObject != null) escObject.SetActive(false);
        SetOptionExitVisible(false);
        isOpen = false;
        CursorUIState.ForceLockCursor();
    }

    private void SetOptionExitVisible(bool visible)
    {
        if (ShopUI.Instance != null) ShopUI.Instance.SetOptionExitVisible(visible);
    }
}
