using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Chỉ điều khiển UI shop đã dựng sẵn trong scene; không tự tạo Canvas hay panel.</summary>
public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("Các panel đã tạo trong scene")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private GameObject optionExitButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button optionExitButtonComponent;

    [Header("Hiển thị tiền (tùy chọn)")]
    [SerializeField] private TMP_Text hudMoneyText;
    [SerializeField] private Text hudLegacyMoneyText;

    private PlayerEconomy subscribedEconomy;
    private bool sessionOpen;
    private bool shopPanelOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Chỉ loại component trùng, không hủy cả GameObject chứa các script khác.
            Destroy(this);
            return;
        }

        Instance = this;
        FindPanelsIfUnassigned();
        FindHudMoneyText();
        if (closeButton == null && shopPanel != null)
        {
            Transform close = FindDeepChild(shopPanel.transform, "exit");
            if (close != null) closeButton = close.GetComponent<Button>();
        }
        if (optionExitButton == null && optionPanel != null)
        {
            Transform exit = FindDeepChild(optionPanel.transform, "exit");
            if (exit != null) optionExitButton = exit.gameObject;
        }
        if (optionExitButtonComponent == null && optionExitButton != null)
            optionExitButtonComponent = optionExitButton.GetComponent<Button>();

        if (closeButton != null) closeButton.onClick.AddListener(CloseShopPanelKeepOptionOpen);
        if (optionExitButtonComponent != null) optionExitButtonComponent.onClick.AddListener(CloseAll);

        // UI-Manager trên object này phải luôn active; chỉ ẩn các panel con.
        if (shopPanel == gameObject || optionPanel == gameObject)
            Debug.LogError("Shop Panel/Option Panel không được là chính GameObject chứa ShopUI script.", this);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);
        if (optionExitButton != null) optionExitButton.SetActive(false);

        BindEconomy();
        UpdateHudMoney();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (subscribedEconomy != null) subscribedEconomy.Changed -= UpdateHudMoney;
        if (closeButton != null) closeButton.onClick.RemoveListener(CloseShopPanelKeepOptionOpen);
        if (optionExitButtonComponent != null) optionExitButtonComponent.onClick.RemoveListener(CloseAll);
        CursorUIState.ReleaseFreeCursor(this);
    }

    private void OnDisable()
    {
        // Nếu có UnityEvent cũ vô tình tắt object quản lý, đóng phiên sạch sẽ
        // để lần tương tác tiếp theo vẫn mở shop và khóa/mở chuột đúng trạng thái.
        if (sessionOpen) CloseAll();
    }

    private void Update()
    {
        // Cho phép nút Exit của OP hoặc UnityEvent khác tắt OP trực tiếp mà
        // không để sessionOpen và quyền chuột bị kẹt ở trạng thái cũ.
        if (sessionOpen && optionPanel != null && !optionPanel.activeSelf)
            CloseAll();

        BindEconomy();
        UpdateHudMoney();
    }

    public void Open()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        sessionOpen = true;
        shopPanelOpen = true;
        if (optionPanel != null) optionPanel.SetActive(true);
        if (shopPanel != null) shopPanel.SetActive(true);
        if (optionExitButton != null) optionExitButton.SetActive(false);
        CursorUIState.RequestFreeCursor(this);
        UpdateHudMoney();
    }

    /// <summary>Đóng riêng shop; giữ Option và con trỏ mở để người chơi bấm nút Exit.</summary>
    public void CloseShopPanelKeepOptionOpen()
    {
        if (!sessionOpen) return;
        if (shopPanel != null) shopPanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(true);
        if (optionExitButton != null) optionExitButton.SetActive(true);
        shopPanelOpen = false;
    }

    /// <summary>Đóng cả shop lẫn Option và trả quyền điều khiển chuột cho camera.</summary>
    public void CloseAll()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);
        SetOptionExitVisible(false);
        sessionOpen = false;
        shopPanelOpen = false;
        CursorUIState.ForceLockCursor();
    }

    /// <summary>Dùng khi OP được mở độc lập bằng Esc thay vì mở từ NPC.</summary>
    public void SetOptionExitVisible(bool visible)
    {
        if (optionExitButton != null) optionExitButton.SetActive(visible);
    }

    public void Close()
    {
        CloseAll();
    }

    public bool HasActiveSession => sessionOpen;
    public bool IsShopPanelOpen => shopPanelOpen;

    private void FindPanelsIfUnassigned()
    {
        if (shopPanel == null)
        {
            Transform panel = transform.Find("UI");
            if (panel != null) shopPanel = panel.gameObject;
        }

        if (optionPanel == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform op = canvas.transform.Find("OP");
                if (op != null) optionPanel = op.gameObject;
            }
        }
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (string.Equals(child.name, childName, System.StringComparison.OrdinalIgnoreCase)) return child;
            Transform found = FindDeepChild(child, childName);
            if (found != null) return found;
        }
        return null;
    }

    private void FindHudMoneyText()
    {
        if (hudMoneyText == null)
        {
            foreach (TMP_Text text in FindObjectsOfType<TMP_Text>(true))
                if (string.Equals(text.gameObject.name, "moneyText", System.StringComparison.OrdinalIgnoreCase))
                {
                    hudMoneyText = text;
                    break;
                }
        }

        if (hudLegacyMoneyText == null)
        {
            foreach (Text text in FindObjectsOfType<Text>(true))
                if (string.Equals(text.gameObject.name, "moneyText", System.StringComparison.OrdinalIgnoreCase))
                {
                    hudLegacyMoneyText = text;
                    break;
                }
        }
    }

    private void BindEconomy()
    {
        PlayerEconomy current = PlayerEconomy.Instance;
        if (subscribedEconomy == current) return;
        if (subscribedEconomy != null) subscribedEconomy.Changed -= UpdateHudMoney;
        subscribedEconomy = current;
        if (subscribedEconomy != null) subscribedEconomy.Changed += UpdateHudMoney;
    }

    private void UpdateHudMoney()
    {
        if (PlayerEconomy.Instance == null) return;
        string value = PlayerEconomy.Instance.Money.ToString("N0");
        if (hudMoneyText != null) hudMoneyText.text = value;
        if (hudLegacyMoneyText != null) hudLegacyMoneyText.text = value;
    }

}
