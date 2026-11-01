using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class CustomerAI : MonoBehaviour
{
    private enum OrderState
    {
        WalkingToCounter,
        WaitingForAcceptance,
        Preparing,
        Leaving
    }

    private Animator animator;
    public NavMeshAgent agent;
    public Transform orderPoint;

    [Header("Order Settings")]
    [Tooltip("Burger (đầy đủ), BeefBurger, CheeseBurger, hoặc BurgerLettuceTomato")]
    [SerializeField] private string requestedFood = "Burger";
    [SerializeField] private string customerDisplayName = "Linh";
    [Tooltip("Hình món cần hiển thị trong khung order của khách này")]
    [SerializeField] private Sprite foodImage;
    [SerializeField] private Sprite beefBurgerImage;
    [SerializeField] private Sprite cheeseBurgerImage;
    [SerializeField] private Sprite beefLettuceTomatoImage;
    [SerializeField] private Sprite fullBurgerImage;
    [SerializeField] private float orderWaitTime = 60f;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private int rewardMoney = 10;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform player;

    [Header("UI References")]
    [SerializeField] private GameObject orderCanvasObject;
    [SerializeField] private TMP_Text orderText;
    [SerializeField] private Image foodImageDisplay;
    private Button orderCardButton;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Image timerFillImage;

    private OrderState state = OrderState.WalkingToCounter;
    private float remainingWaitTime;
    private Vector3 spawnPosition;
    private GameObject reviewPopup;
    private GameObject ingredientsPopupCanvas;
    private GameObject ingredientsPanel;
    private readonly List<Sprite> generatedIngredientIcons = new List<Sprite>();

    public Vector3 LookTarget
    {
        get
        {
            if (!TryGetCharacterBounds(out Bounds bounds))
                return transform.position + Vector3.up * 1.5f;
            return bounds.center;
        }
    }

    public string RequestedFood => requestedFood;

    public string[] GetRequiredIngredients()
    {
        switch (NormalizeOrderKey(requestedFood))
        {
            case "beefburger":
                return new[] { "BunBottom", "CookedMeat", "BunTop" };
            case "cheeseburger":
                return new[] { "BunBottom", "CookedMeat", "ChoppedCheese", "BunTop" };
            case "burgerlettucetomato":
                return new[] { "BunBottom", "CookedMeat", "ChoppedLettuce", "ChoppedTomato", "BunTop" };
            default:
                return new[]
                {
                    "BunBottom", "CookedMeat", "ChoppedLettuce", "ChoppedTomato",
                    "ChoppedOnion", "ChoppedCheese", "BunTop"
                };
        }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        SetupOrderCardButton();

        spawnPosition = transform.position;
        if (orderCanvasObject != null)
            orderCanvasObject.SetActive(false);

        if (agent != null && orderPoint != null)
        {
            agent.SetDestination(orderPoint.position);
            StartWalking();
        }
        else
        {
            ArriveAtCounter();
        }
    }

    private void Update()
    {
        if (ingredientsPopupCanvas != null && ingredientsPopupCanvas.activeSelf && Input.GetKeyDown(KeyCode.X))
            ingredientsPopupCanvas.SetActive(false);

        if (state == OrderState.WalkingToCounter && HasReachedDestination())
            ArriveAtCounter();

        if (state == OrderState.Preparing)
        {
            remainingWaitTime = Mathf.Max(0f, remainingWaitTime - Time.deltaTime);
            UpdateTimerUI();
            if (remainingWaitTime <= 0f)
                FinishOrder(false, null);
        }
        else if (state == OrderState.Leaving && HasReachedDestination())
        {
            Destroy(gameObject);
        }
    }

    public bool IsNearPlayer(Transform playerTransform)
    {
        return playerTransform != null &&
               Vector3.Distance(playerTransform.position, transform.position) <= interactionDistance;
    }

    public void Interact(PlayerOrderInteraction playerInteraction)
    {
        if (playerInteraction == null || !IsNearPlayer(playerInteraction.transform))
            return;

        if (state == OrderState.WaitingForAcceptance)
        {
            state = OrderState.Preparing;
            playerInteraction.AcceptOrder(this);
            remainingWaitTime = Mathf.Max(0f, orderWaitTime);
            ShowOrderDisplay();
            UpdateTimerUI();
            return;
        }

        if (state != OrderState.Preparing)
            return;

        if (!playerInteraction.IsFoodReady)
        {
            ToggleIngredientsPanel();
            return;
        }

        string deliveredFood = playerInteraction.ConsumeFood();
        bool isCorrectFood = string.Equals(
            NormalizeFoodName(deliveredFood),
            NormalizeFoodName(requestedFood),
            System.StringComparison.OrdinalIgnoreCase);
        bool containsBurntMeat = playerInteraction.LastDeliveredFoodContainsBurntMeat;

        FinishOrder(isCorrectFood && !containsBurntMeat, playerInteraction, containsBurntMeat);
    }

    private void ArriveAtCounter()
    {
        if (state != OrderState.WalkingToCounter)
            return;

        state = OrderState.WaitingForAcceptance;
        StopAndWait();
        if (animator != null)
            animator.SetTrigger("IsOrdering");

        // Keep the existing dish frame intact, but reveal it after the player accepts.
        if (orderCanvasObject != null)
            orderCanvasObject.SetActive(false);
    }

    private void ShowOrderDisplay()
    {
        if (orderText != null)
            orderText.gameObject.SetActive(false);

        if (foodImageDisplay != null)
        {
            foodImageDisplay.sprite = GetOrderImage();
            foodImageDisplay.preserveAspect = true;
            foodImageDisplay.gameObject.SetActive(foodImageDisplay.sprite != null);
        }
        if (timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = Mathf.Max(0.01f, orderWaitTime);
            timerSlider.value = timerSlider.maxValue;
            timerSlider.gameObject.SetActive(true);
        }
        if (ingredientsPopupCanvas != null)
            ingredientsPopupCanvas.SetActive(false);
        if (orderCanvasObject != null)
            orderCanvasObject.SetActive(true);

    }

    private void SetupOrderCardButton()
    {
        if (foodImageDisplay == null)
            return;

        orderCardButton = foodImageDisplay.GetComponent<Button>();
        if (orderCardButton == null)
            orderCardButton = foodImageDisplay.gameObject.AddComponent<Button>();

        orderCardButton.transition = Selectable.Transition.None;
        orderCardButton.targetGraphic = foodImageDisplay;
        orderCardButton.onClick.RemoveListener(OnOrderCardClicked);
        orderCardButton.onClick.AddListener(OnOrderCardClicked);
    }

    private void OnOrderCardClicked()
    {
        PlayerOrderInteraction playerInteraction = player != null
            ? player.GetComponent<PlayerOrderInteraction>()
            : null;
        if (playerInteraction == null)
            playerInteraction = FindObjectOfType<PlayerOrderInteraction>();
        Interact(playerInteraction);
    }

    private Sprite GetOrderImage()
    {
        Sprite orderImage;
        switch (NormalizeOrderKey(requestedFood))
        {
            case "beefburger": orderImage = beefBurgerImage; break;
            case "cheeseburger": orderImage = cheeseBurgerImage; break;
            case "burgerlettucetomato": orderImage = beefLettuceTomatoImage; break;
            default: orderImage = fullBurgerImage; break;
        }
        return orderImage != null ? orderImage : foodImage;
    }

    private void FinishOrder(bool success, PlayerOrderInteraction playerInteraction, bool containsBurntMeat = false)
    {
        if (state == OrderState.Leaving)
            return;

        state = OrderState.Leaving;
        if (success)
        {
            if (playerInteraction != null)
                playerInteraction.AddMoney(rewardMoney);
        }

        ShowAutomaticReview(success, containsBurntMeat);

        if (timerSlider != null)
            timerSlider.gameObject.SetActive(false);
        if (ingredientsPopupCanvas != null)
            ingredientsPopupCanvas.SetActive(false);
        if (orderCanvasObject != null)
            Invoke(nameof(HideOrderDisplay), 4.5f);

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            Vector3 destination = exitPoint != null ? exitPoint.position : spawnPosition;
            agent.SetDestination(destination);
            StartWalking();
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }

    private void ShowAutomaticReview(bool success, bool containsBurntMeat)
    {
        int stars = success ? Random.Range(4, 6) : Random.Range(1, 4);
        string[] positiveReviews =
        {
            "Burger ngon, đúng món tôi gọi!",
            "Thịt chín vừa, phần ăn rất ổn!",
            "Đúng yêu cầu, tôi sẽ quay lại!"
        };
        string[] failedReviews = containsBurntMeat
            ? new[] { "Thịt bị cháy, tôi không hài lòng.", "Burger có thịt cháy, lần sau chú ý nhé.", "Món bị khét nên tôi không thể đánh giá cao." }
            : new[] { "Món này không đúng đơn tôi gọi.", "Bạn đã làm nhầm hamburger của tôi.", "Sai nguyên liệu nên món chưa đúng yêu cầu." };

        string review = success
            ? positiveReviews[Random.Range(0, positiveReviews.Length)]
            : failedReviews[Random.Range(0, failedReviews.Length)];
        if (orderText != null)
        {
            orderText.text = new string('★', stars) + new string('☆', 5 - stars) + "\n" + review;
            orderText.gameObject.SetActive(true);
        }

        ShowReviewPopup(stars, review);
    }

    private void ShowReviewPopup(int stars, string review)
    {
        if (reviewPopup != null)
            Destroy(reviewPopup);

        reviewPopup = new GameObject("AutomaticOrderReview", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = reviewPopup.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = reviewPopup.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("ReviewPanel", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(reviewPopup.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 115f);
        panelRect.sizeDelta = new Vector2(760f, 145f);
        panel.GetComponent<Image>().color = new Color(0.18f, 0.13f, 0.09f, 0.94f);

        GameObject label = new GameObject("ReviewText", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        label.transform.SetParent(panel.transform, false);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(20f, 12f);
        labelRect.offsetMax = new Vector2(-20f, -12f);

        TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = 28f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.94f, 0.78f);
        text.raycastTarget = false;
        text.text = new string('★', stars) + new string('☆', 5 - stars) + "\n" + review;

        Destroy(reviewPopup, 4.5f);
    }

    private bool HasReachedDestination()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return true;

        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance + 0.25f;
    }

    private void UpdateTimerUI()
    {
        if (timerSlider == null)
            return;

        timerSlider.value = remainingWaitTime;
        if (timerFillImage != null)
        {
            float progress = remainingWaitTime / Mathf.Max(0.01f, orderWaitTime);
            timerFillImage.color = progress > 0.5f ? Color.green :
                                   progress > 0.2f ? Color.yellow : Color.red;
        }
    }

    private void ToggleIngredientsPanel()
    {
        if (ingredientsPanel == null)
            CreateIngredientsPanel();

        if (ingredientsPopupCanvas == null)
            return;

        ingredientsPopupCanvas.SetActive(!ingredientsPopupCanvas.activeSelf);
    }

    private void CreateIngredientsPanel()
    {
        if (orderCanvasObject == null)
            return;

        ingredientsPopupCanvas = new GameObject("IngredientsPopupCanvas", typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas popupCanvas = ingredientsPopupCanvas.GetComponent<Canvas>();
        popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        popupCanvas.sortingOrder = 300;

        CanvasScaler scaler = ingredientsPopupCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        ingredientsPanel = new GameObject("RequiredIngredients", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        ingredientsPanel.transform.SetParent(ingredientsPopupCanvas.transform, false);

        RectTransform panelRect = ingredientsPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(500f, 410f);

        Image panelImage = ingredientsPanel.GetComponent<Image>();
        Transform orderFrame = orderCanvasObject.transform.Find("FramePanel");
        Image frameImage = orderFrame != null ? orderFrame.GetComponent<Image>() : null;
        if (frameImage != null)
            panelImage.sprite = frameImage.sprite;
        panelImage.color = Color.white;
        panelImage.raycastTarget = false;

        CreatePanelLabel("NGUY\u00CAN LI\u1EC6U C\u1EA6N", new Vector2(0f, 168f),
            new Vector2(440f, 42f), 28f, FontStyles.Bold);
        CreateIngredientGrid();
        CreatePanelLabel("Nh\u1EA5n X \u0111\u1EC3 \u0111\u00F3ng", new Vector2(0f, -183f),
            new Vector2(440f, 32f), 20f, FontStyles.Normal);

        ingredientsPopupCanvas.SetActive(false);
    }

    private void CreateIngredientGrid()
    {
        PlayerOrderInteraction playerInteraction = player != null
            ? player.GetComponent<PlayerOrderInteraction>()
            : null;
        if (playerInteraction == null)
            playerInteraction = FindObjectOfType<PlayerOrderInteraction>();

        const int previewLayer = 31;
        RenderTexture previewTexture = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32);
        previewTexture.Create();

        GameObject cameraObject = new GameObject("IngredientPreviewCamera");
        Camera previewCamera = cameraObject.AddComponent<Camera>();
        previewCamera.enabled = false;
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        previewCamera.cullingMask = 1 << previewLayer;
        previewCamera.orthographic = true;
        previewCamera.orthographicSize = 0.55f;
        previewCamera.nearClipPlane = 0.05f;
        previewCamera.farClipPlane = 10f;
        previewCamera.targetTexture = previewTexture;
        previewCamera.transform.position = new Vector3(0f, 5000f, -2f);
        previewCamera.transform.rotation = Quaternion.identity;

        GameObject lightObject = new GameObject("IngredientPreviewLight");
        Light previewLight = lightObject.AddComponent<Light>();
        previewLight.type = LightType.Directional;
        previewLight.intensity = 1.3f;
        previewLight.cullingMask = 1 << previewLayer;
        lightObject.transform.rotation = Quaternion.Euler(35f, -30f, 0f);

        string[] requiredIngredients = GetRequiredIngredients();
        for (int i = 0; i < requiredIngredients.Length; i++)
        {
            int column = i % 3;
            int row = i / 3;
            float x = (column - 1) * 145f;
            float iconY = 100f - row * 100f;
            GameObject prefab = playerInteraction != null
                ? playerInteraction.GetIngredientDisplayPrefab(requiredIngredients[i])
                : null;
            Sprite icon = RenderIngredientIcon(requiredIngredients[i], prefab,
                previewCamera, previewTexture, previewLayer);

            GameObject iconObject = new GameObject("IngredientIcon_" + i, typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(ingredientsPanel.transform, false);
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(x - 35f, iconY);
            iconRect.sizeDelta = new Vector2(84f, 84f);
            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            CreatePanelLabel("x1", new Vector2(x + 48f, iconY),
                new Vector2(44f, 30f), 22f, FontStyles.Bold);
        }

        Destroy(cameraObject);
        Destroy(lightObject);
        previewTexture.Release();
        Destroy(previewTexture);
    }

    private Sprite RenderIngredientIcon(string ingredient, GameObject prefab, Camera previewCamera,
        RenderTexture target, int layer)
    {
        if (prefab == null)
            return null;

        Vector3 previewCenter = new Vector3(0f, 5000f, 0f);
        GameObject preview = Instantiate(prefab, previewCenter, Quaternion.Euler(18f, 25f, 0f));
        SetLayerRecursively(preview.transform, layer);
        foreach (Collider itemCollider in preview.GetComponentsInChildren<Collider>(true))
            itemCollider.enabled = false;
        foreach (Rigidbody itemBody in preview.GetComponentsInChildren<Rigidbody>(true))
        {
            itemBody.useGravity = false;
            itemBody.isKinematic = true;
        }
        if (ingredient == "CookedMeat")
        {
            foreach (Renderer renderer in preview.GetComponentsInChildren<Renderer>(true))
                renderer.material.color = new Color(0.42f, 0.19f, 0.06f);
        }

        Bounds bounds = GetPreviewBounds(preview.transform);
        float largestDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (largestDimension > 0.001f)
            preview.transform.localScale *= 0.9f / largestDimension;

        bounds = GetPreviewBounds(preview.transform);
        preview.transform.position += previewCenter - bounds.center;
        previewCamera.targetTexture = target;
        previewCamera.Render();

        RenderTexture previousTarget = RenderTexture.active;
        RenderTexture.active = target;
        Texture2D texture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0f, 0f, target.width, target.height), 0, 0);
        texture.Apply();
        RenderTexture.active = previousTarget;
        preview.SetActive(false);
        Destroy(preview);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), 100f);
        generatedIngredientIcons.Add(sprite);
        return sprite;
    }

    private void CreatePanelLabel(string text, Vector2 position, Vector2 size,
        float fontSize, FontStyles fontStyle)
    {
        GameObject labelObject = new GameObject("PanelLabel", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(ingredientsPanel.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = position;
        labelRect.sizeDelta = size;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.color = new Color(0.22f, 0.15f, 0.09f, 1f);
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
    }

    private static Bounds GetPreviewBounds(Transform root)
    {
        Renderer[] allRenderers = root.GetComponentsInChildren<Renderer>(true);
        List<Renderer> renderers = new List<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                renderers.Add(renderer);
        }
        if (renderers.Count == 0)
            return new Bounds(root.position, Vector3.one * 0.5f);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Count; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        foreach (Transform child in root)
            SetLayerRecursively(child, layer);
    }

    private void OnDestroy()
    {
        foreach (Sprite icon in generatedIngredientIcons)
        {
            if (icon == null)
                continue;
            Texture2D texture = icon.texture;
            Destroy(icon);
            if (texture != null)
                Destroy(texture);
        }
    }

    private bool TryGetCharacterBounds(out Bounds bounds)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        bool foundRenderer = false;
        bounds = new Bounds(transform.position, Vector3.zero);

        foreach (Renderer childRenderer in renderers)
        {
            if (!foundRenderer)
            {
                bounds = childRenderer.bounds;
                foundRenderer = true;
            }
            else
            {
                bounds.Encapsulate(childRenderer.bounds);
            }
        }

        return foundRenderer;
    }

    private void HideOrderDisplay()
    {
        if (ingredientsPopupCanvas != null)
            ingredientsPopupCanvas.SetActive(false);
        if (orderCanvasObject != null)
            orderCanvasObject.SetActive(false);
    }

    private static string NormalizeFoodName(string foodName)
    {
        if (string.IsNullOrWhiteSpace(foodName))
            return string.Empty;

        string normalized = foodName.Trim();
        if (normalized.StartsWith("food_", System.StringComparison.OrdinalIgnoreCase))
            normalized = normalized.Substring(5);
        return normalized.Replace("_", " ").Trim();
    }

    private void StartWalking()
    {
        if (animator != null)
            animator.SetBool("IsWalking", true);
    }

    private void StopAndWait()
    {
        if (animator != null)
            animator.SetBool("IsWalking", false);
    }

    private void LateUpdate()
    {
        if (Camera.main != null && orderCanvasObject != null && orderCanvasObject.activeSelf)
            orderCanvasObject.transform.rotation = Camera.main.transform.rotation;
    }

    private static string NormalizeOrderKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "burger";

        return value.Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }
}
