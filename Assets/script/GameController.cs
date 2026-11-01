using UnityEngine;

/// <summary>Bật/tắt một giao diện được gán trong Inspector bằng phím Escape.</summary>
public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject escObject;
    [SerializeField] private bool hideOnStart = true;

    private bool isOpen;
    private CursorLockMode previousLockState;
    private bool previousCursorVisibility;

    private void Start()
    {
        if (escObject == null)
        {
            Debug.LogWarning("GameController chưa được gán GameObject cần hiện khi bấm Esc.", this);
            return;
        }

        if (hideOnStart) escObject.SetActive(false);
        isOpen = escObject.activeSelf;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleEscObject();
    }

    public void ToggleEscObject()
    {
        if (escObject == null) return;

        if (!isOpen)
        {
            previousLockState = Cursor.lockState;
            previousCursorVisibility = Cursor.visible;
            escObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isOpen = true;
        }
        else
        {
            escObject.SetActive(false);
            Cursor.lockState = previousLockState;
            Cursor.visible = previousCursorVisibility;
            isOpen = false;
        }
    }
}
