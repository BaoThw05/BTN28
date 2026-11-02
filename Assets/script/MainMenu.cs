using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    private void OnEnable()
    {
        // Menu cần con trỏ để người chơi chọn nút sau khi quay lại từ scene khác.
        CursorUIState.RequestFreeCursor(this);
    }

    private void OnDisable()
    {
        CursorUIState.ReleaseFreeCursor(this);
    }

    public void PlayGame()
    {
        // Scene gameplay sẽ khóa chuột để điều khiển camera.
        CursorUIState.ForceLockCursor();
        SceneManager.LoadScene("Kitchen");
    }
}
