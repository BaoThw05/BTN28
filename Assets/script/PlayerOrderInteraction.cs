using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerOrderInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float foodPickupDistance = 3f;
    [SerializeField] private float stationLookDistance = 8f;
    [SerializeField] private float stationLookAngle = 35f;
    [SerializeField] private float stationUseDistance = 5.5f;
    [SerializeField] private float customerLookDistance = 10f;
    [SerializeField] private float customerLookAngle = 25f;

    [Header("Preparation animation (optional)")]
    [SerializeField] private Animator preparationAnimator;
    [SerializeField] private float choppingDuration = 4f;

    [Header("Restaurant Prefabs (giữ model gốc)")]
    [SerializeField] private GameObject bunCratePrefab;
    [SerializeField] private GameObject rawMeatPrefab;
    [SerializeField] private GameObject lettucePrefab;
    [SerializeField] private GameObject tomatoPrefab;
    [SerializeField] private GameObject onionPrefab;
    [SerializeField] private GameObject cheesePrefab;
    [SerializeField] private GameObject choppingLettucePrefab;
    [SerializeField] private GameObject choppingOnionPrefab;
    [SerializeField] private GameObject choppingCheesePrefab;
    [SerializeField] private GameObject choppedLettucePrefab;
    [SerializeField] private GameObject slicedTomatoPrefab;
    [SerializeField] private GameObject choppedOnionPrefab;
    [SerializeField] private GameObject choppedCheesePrefab;
    [SerializeField] private GameObject bunBottomPrefab;
    [SerializeField] private GameObject bunTopPrefab;
    [SerializeField] private GameObject assembledBurgerPrefab;
    [SerializeField] private GameObject packagedBurgerPrefab;
    [SerializeField] private GameObject servingTrayPrefab;

    [Header("Money")]
    [SerializeField] private int startingMoney;
    [SerializeField] private TMP_Text moneyText;

    private readonly HashSet<string> ingredients = new HashSet<string>();
    private readonly HashSet<string> ingredientsOnCuttingBoard = new HashSet<string>();
    private readonly Dictionary<string, GameObject> ingredientVisuals = new Dictionary<string, GameObject>();
    private string carriedFood;
    private GameObject carriedVisual;
    private string carriedIngredient;
    private GameObject carriedIngredientVisual;
    private Vector3 carriedIngredientOriginalScale;
    private Transform assemblyTable;
    private Transform cuttingBoard;
    private Transform preparationModelRoot;
    private CustomerAI activeOrder;
    private bool lastDeliveredFoodContainsBurntMeat;
    private int money;
    private bool meatOnPan;
    private bool meatCooked;
    private bool meatBurnt;
    private bool isChopping;
    private float meatCookTimer;
    private float meatBurnTimer;
    private GameObject panMeatVisual;
    private Renderer panMeatRenderer;
    private string actionMessage;
    private float actionMessageUntil;

    public bool HasFood => !string.IsNullOrEmpty(carriedFood);
    public bool IsFoodReady => HasFood;
    public int Money => money;
    public bool LastDeliveredFoodContainsBurntMeat => lastDeliveredFoodContainsBurntMeat;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        FindPreparationAnimator();
        money = startingMoney;
        UpdateMoneyText();
        ConfigureCookingStations();
    }

    private void ConfigureCookingStations()
    {
        GameObject fridge = GameObject.Find("fridge_A_decorated");
        if (fridge != null)
        {
            GetOrAddStation(fridge, CookingStation.StationType.Fridge);
            FridgeInteraction fridgeInteraction = fridge.GetComponent<FridgeInteraction>();
            if (fridgeInteraction == null)
                fridgeInteraction = fridge.AddComponent<FridgeInteraction>();
            fridgeInteraction.Initialize(this, playerCamera, new string[]
            {
                "RawMeat", "Lettuce", "Tomato", "Onion", "Cheese"
            }, new GameObject[]
            {
                rawMeatPrefab, lettucePrefab, tomatoPrefab, onionPrefab, cheesePrefab
            });
        }
        ConfigureStation("pan_B", CookingStation.StationType.Pan);

        GameObject[] assemblyLids =
        {
            GameObject.Find("crate_lid"),
            GameObject.Find("crate_lid (1)")
        };
        foreach (GameObject lid in assemblyLids)
        {
            if (lid == null)
                continue;

            if (this.assemblyTable == null)
                this.assemblyTable = lid.transform;
            GetOrAddStation(lid, CookingStation.StationType.AssemblyTable);
            EnsureClickableSurfaceCollider(lid);
        }

        GameObject cuttingBoardObject = GameObject.Find("cuttingboard");
        if (cuttingBoardObject != null)
        {
            cuttingBoard = cuttingBoardObject.transform;
            GetOrAddStation(cuttingBoardObject, CookingStation.StationType.CuttingBoard);
        }

        GameObject bunCrate = GameObject.Find("crate_buns");
        if (bunCrate == null)
        {
            GameObject fridgeObject = GameObject.Find("fridge_A_decorated");
            GameObject stove = GameObject.Find("stove_multi");
            GameObject kitchenRoot = GameObject.Find("Kitchen");
            if (fridgeObject != null)
            {
                Transform placement = stove != null ? stove.transform : fridgeObject.transform;
                if (bunCratePrefab == null)
                    return;
                bunCrate = Instantiate(bunCratePrefab,
                    placement.position + placement.right * 1.45f,
                    placement.rotation);
                bunCrate.name = "crate_buns";
                if (kitchenRoot != null)
                    bunCrate.transform.SetParent(kitchenRoot.transform, true);
                GetOrAddStation(bunCrate, CookingStation.StationType.BunCrate);
            }
        }
        else
        {
            GetOrAddStation(bunCrate, CookingStation.StationType.BunCrate);
        }
    }

    private void FindPreparationAnimator()
    {
        if (preparationAnimator == null)
        {
            Animator[] animators = GetComponentsInChildren<Animator>(true);
            int cuttingState = Animator.StringToHash("Cutting Vegetables");
            foreach (Animator candidate in animators)
            {
                if (candidate.name.Contains("PFB_Chibi_Base") || candidate.HasState(0, cuttingState))
                {
                    preparationAnimator = candidate;
                    break;
                }
            }

            if (preparationAnimator == null && animators.Length > 0)
                preparationAnimator = animators[0];
        }

        if (preparationAnimator == null)
            return;

        preparationModelRoot = preparationAnimator.transform;
        while (preparationModelRoot.parent != null && preparationModelRoot.parent != transform)
            preparationModelRoot = preparationModelRoot.parent;

    }

    private static void ConfigureStation(string objectName, CookingStation.StationType type)
    {
        GameObject stationObject = GameObject.Find(objectName);
        if (stationObject != null)
            GetOrAddStation(stationObject, type);
    }

    private static void GetOrAddStation(GameObject target, CookingStation.StationType type)
    {
        CookingStation station = target.GetComponent<CookingStation>();
        if (station == null)
            station = target.AddComponent<CookingStation>();
        station.Configure(type);
    }

    private void Update()
    {
        UpdateMeatCooking();
        if (FridgeUI.IsOpen || isChopping)
            return;
        bool pressedInteract = Input.GetKeyDown(KeyCode.E);
        bool clicked = Input.GetMouseButtonDown(0);
        if (!pressedInteract && !clicked)
            return;

        if (clicked && !HasFood && string.IsNullOrEmpty(carriedIngredient))
        {
            if (TryPickUpFood() || TryPickUpPlacedIngredient())
                return;
        }

        if (clicked)
        {
            CustomerAI clickedCustomer = FindClickedCustomer();
            if (clickedCustomer != null)
            {
                clickedCustomer.Interact(this);
                return;
            }
        }

        CookingStation station = clicked ? FindClickedStation() : null;
        if (station == null && (pressedInteract || clicked))
            station = FindLookedAtStation();
        if (station != null)
        {
            station.Interact(this);
            return;
        }

        if (clicked)
        {
            CustomerAI lookedAtCustomer = FindLookedAtCustomer();
            if (lookedAtCustomer != null)
            {
                lookedAtCustomer.Interact(this);
                return;
            }
        }

        if (clicked && !string.IsNullOrEmpty(carriedIngredient))
            ShowActionMessage("Nhắm vào mặt bàn, thớt hoặc chảo rồi click để thả nguyên liệu.", 2f);

        if (pressedInteract && !HasFood && TryPickUpFood())
            return;

        if (pressedInteract)
        {
            CustomerAI customer = FindLookedAtCustomer();
            if (customer != null)
                customer.Interact(this);
        }
    }

    public string ConsumeFood()
    {
        string food = carriedFood;
        FoodItem deliveredItem = carriedVisual != null ? carriedVisual.GetComponent<FoodItem>() : null;
        lastDeliveredFoodContainsBurntMeat = deliveredItem != null && deliveredItem.ContainsBurntMeat;
        carriedFood = null;
        if (carriedVisual != null)
            Destroy(carriedVisual);
        carriedVisual = null;
        activeOrder = null;
        return food;
    }

    public void AcceptOrder(CustomerAI customer)
    {
        activeOrder = customer;
    }

    public void AddMoney(int amount)
    {
        money += Mathf.Max(0, amount);
        UpdateMoneyText();
        Debug.Log("Tiền hiện có: " + money + "$", this);
    }

    public void InteractWithStation(CookingStation station)
    {
        switch (station.Type)
        {
            case CookingStation.StationType.Fridge:
                FridgeInteraction fridge = station.GetComponent<FridgeInteraction>();
                if (fridge != null)
                    fridge.OpenFridgeUI();
                break;
            case CookingStation.StationType.BunCrate:
                TakeBuns();
                break;
            case CookingStation.StationType.AssemblyTable:
                assemblyTable = station.transform;
                if (!string.IsNullOrEmpty(carriedIngredient))
                    PlaceCarriedIngredientOnTable();
                else
                    AssembleBurger();
                break;
            case CookingStation.StationType.CuttingBoard:
                if (!string.IsNullOrEmpty(carriedIngredient))
                    PlaceCarriedIngredientOnCuttingBoard();
                else if (!TryStartChopping())
                    ShowActionMessage("Lấy rau, cà chua, hành hoặc phô mai đặt lên thớt trước.", 2.5f);
                break;
            case CookingStation.StationType.Pan:
                UsePan(station);
                break;
        }
    }

    private CookingStation FindLookedAtStation()
    {
        if (playerCamera == null)
            return null;

        CookingStation best = null;
        float bestAngle = stationLookAngle;
        foreach (CookingStation station in FindObjectsOfType<CookingStation>())
        {
            if (station == null)
                continue;
            if (Vector3.Distance(transform.position, station.transform.position) > stationUseDistance)
                continue;
            Vector3 toStation = station.LookTarget - playerCamera.transform.position;
            float distance = toStation.magnitude;
            if (distance <= 0.01f || distance > stationLookDistance)
                continue;
            float angle = Vector3.Angle(playerCamera.transform.forward, toStation);
            if (angle <= bestAngle)
            {
                best = station;
                bestAngle = angle;
            }
        }
        return best;
    }

    private CookingStation FindClickedStation()
    {
        if (playerCamera == null)
            return null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, stationLookDistance, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            CookingStation station = hit.collider.GetComponentInParent<CookingStation>();
            if (station != null && Vector3.Distance(transform.position, station.transform.position) <= stationUseDistance)
                return station;
        }

        return null;
    }

    private void TakeFridgeIngredient()
    {
        if (!string.IsNullOrEmpty(carriedIngredient) || HasFood)
        {
            ShowActionMessage("Place the ingredient you are carrying first.", 2f);
            return;
        }
        string[] fridgeItems = { "RawMeat", "Lettuce", "Tomato", "Onion", "Cheese" };
        foreach (string item in fridgeItems)
        {
            if (ingredients.Contains(item))
                continue;
            GameObject prefab = GetIngredientPrefab(item);
            if (prefab == null)
            {
                ShowActionMessage("Missing ingredient prefab: " + item, 2f);
                return;
            }
            ingredients.Add(item);
            PickUpIngredient(item, prefab);
            ShowActionMessage("Carry " + IngredientLabel(item) + " to the assembly table.", 2f);
            return;
        }
        ShowActionMessage("All fridge ingredients have been collected.", 2f);
    }

    public bool TryPickUpFridgeIngredient(string item, GameObject prefab)
    {
        if (string.IsNullOrEmpty(item) || prefab == null ||
            !string.IsNullOrEmpty(carriedIngredient) || HasFood || ingredients.Contains(item))
            return false;

        ingredients.Add(item);
        PickUpIngredient(item, prefab);
        ShowActionMessage("Đã lấy " + IngredientLabel(item) + ". Nhìn tới trạm rồi click chuột trái hoặc nhấn E để đặt.", 3f);
        return true;
    }

    private void TakeBuns()
    {
        if (!string.IsNullOrEmpty(carriedIngredient) || HasFood)
        {
            ShowActionMessage("Place the ingredient you are carrying first.", 2f);
            return;
        }
        string bun = !ingredients.Contains("BunBottom") ? "BunBottom" :
                     !ingredients.Contains("BunTop") ? "BunTop" : null;
        if (bun == null)
        {
            ShowActionMessage("Both burger buns have been collected.", 2f);
            return;
        }
        GameObject prefab = bun == "BunBottom" ? bunBottomPrefab : bunTopPrefab;
        if (prefab == null)
        {
            ShowActionMessage("Missing burger bun prefab: " + bun, 2f);
            return;
        }
        ingredients.Add(bun);
        PickUpIngredient(bun, prefab);
        ShowActionMessage("Carry " + IngredientLabel(bun) + " to the assembly table.", 2f);
    }

    private void UsePan(CookingStation station)
    {
        if (!meatOnPan)
        {
            if (carriedIngredient != "RawMeat" || carriedIngredientVisual == null)
            {
                ShowActionMessage("Take raw meat from the fridge first.", 2f);
                return;
            }
            ingredients.Remove("RawMeat");
            meatOnPan = true;
            meatCooked = false;
            meatBurnt = false;
            meatCookTimer = 5f;
            meatBurnTimer = 3f;
            panMeatVisual = carriedIngredientVisual;
            carriedIngredientVisual = null;
            carriedIngredient = null;
            panMeatVisual.transform.SetParent(null, true);
            panMeatVisual.transform.localScale = carriedIngredientOriginalScale;
            panMeatRenderer = panMeatVisual.GetComponentInChildren<Renderer>();
            MoveVisualToPan(panMeatVisual, station);
            ShowActionMessage("Frying meat: red is raw, brown is cooked, black is burnt.", 3f);
            return;
        }

        if (!meatCooked && !meatBurnt)
        {
            ShowActionMessage("The meat is still raw. Wait for it to turn brown.", 2f);
            return;
        }
        if (!string.IsNullOrEmpty(carriedIngredient) || HasFood)
        {
            ShowActionMessage("Place what you are carrying before taking the meat.", 2f);
            return;
        }

        meatOnPan = false;
        if (meatBurnt)
        {
            ingredients.Add("BurntMeat");
            if (panMeatVisual != null)
            {
                carriedIngredientOriginalScale = panMeatVisual.transform.localScale;
                CarryExistingIngredient("BurntMeat", panMeatVisual);
                ShowActionMessage("Thịt bị cháy. Bạn vẫn có thể ráp và giao, nhưng khách sẽ đánh giá thấp.", 3f);
            }
            else
            {
                ShowActionMessage("Thịt bị cháy. Lấy thịt mới từ tủ lạnh.", 2f);
            }
        }
        else
        {
            ingredients.Add("CookedMeat");
            if (panMeatVisual != null)
            {
                carriedIngredientOriginalScale = panMeatVisual.transform.localScale;
                CarryExistingIngredient("CookedMeat", panMeatVisual);
            }
            ShowActionMessage("Thịt đã chín, click khung giữa để đặt.", 2f);
        }
        panMeatVisual = null;
        panMeatRenderer = null;
    }

    private void UpdateMeatCooking()
    {
        if (!meatOnPan)
            return;
        if (!meatCooked)
        {
            meatCookTimer -= Time.deltaTime;
            if (meatCookTimer <= 0f)
            {
                meatCooked = true;
                SetPanMeatColor(new Color(0.42f, 0.19f, 0.06f));
                ShowActionMessage("Thịt đã chín màu nâu! Nhấn E lấy khỏi chảo.", 3f);
            }
        }
        else if (!meatBurnt)
        {
            meatBurnTimer -= Time.deltaTime;
            if (meatBurnTimer <= 0f)
            {
                meatBurnt = true;
                SetPanMeatColor(new Color(0.06f, 0.045f, 0.035f));
                ShowActionMessage("Thịt cháy đen! Nhấn E lấy khỏi chảo.", 3f);
            }
        }
    }

    private void MoveVisualToPan(GameObject meatVisual, CookingStation station)
    {
        meatVisual.transform.SetParent(null, true);
        Bounds panBounds = GetBounds(station.transform);
        Vector3 panCenter = station.transform.position;
        Renderer[] panParts = station.GetComponentsInChildren<Renderer>(true);
        float largestRoundPart = 0f;
        foreach (Renderer panPart in panParts)
        {
            if (panPart == null || !panPart.enabled)
                continue;

            Bounds partBounds = panPart.bounds;
            float wide = Mathf.Max(partBounds.size.x, partBounds.size.z);
            float narrow = Mathf.Min(partBounds.size.x, partBounds.size.z);
            if (wide <= 0.001f)
                continue;

            float roundness = narrow / wide;
            if (roundness >= 0.72f && narrow > largestRoundPart)
            {
                largestRoundPart = narrow;
                panBounds = partBounds;
            }
        }

        meatVisual.transform.rotation = Quaternion.Euler(0f, station.transform.eulerAngles.y, 0f);
        Bounds rawMeatBounds = GetBounds(meatVisual.transform);
        // Leave room for the inner rim so the patty sits inside the pan instead of above it.
        float panDiameter = Mathf.Min(panBounds.size.x, panBounds.size.z) * 0.62f;
        float meatDiameter = Mathf.Max(rawMeatBounds.size.x, rawMeatBounds.size.z);
        if (meatDiameter > 0.001f && panDiameter > 0f)
        {
            float fitScale = panDiameter / meatDiameter;
            meatVisual.transform.localScale *= fitScale;
        }

        Vector3 surface = panCenter;
        surface.y = panBounds.min.y + panBounds.size.y * 0.08f;
        meatVisual.transform.position = surface;
        Bounds meatBounds = GetBounds(meatVisual.transform);
        meatVisual.transform.position += new Vector3(
            panCenter.x - meatBounds.center.x,
            surface.y - meatBounds.min.y + 0.005f,
            panCenter.z - meatBounds.center.z);
        SetPanMeatColor(new Color(0.72f, 0.08f, 0.06f));
    }

    private void PickUpIngredient(string item, GameObject prefab)
    {
        carriedIngredient = item;
        carriedIngredientVisual = InstantiatePrefab(prefab);
        if (carriedIngredientVisual == null)
        {
            carriedIngredient = null;
            return;
        }
        carriedIngredientVisual.name = item + "_Carried";
        carriedIngredientOriginalScale = carriedIngredientVisual.transform.localScale;
        foreach (Collider itemCollider in carriedIngredientVisual.GetComponentsInChildren<Collider>())
            itemCollider.enabled = false;
        foreach (Rigidbody itemBody in carriedIngredientVisual.GetComponentsInChildren<Rigidbody>())
        {
            itemBody.useGravity = false;
            itemBody.detectCollisions = false;
            itemBody.isKinematic = true;
        }
        if (playerCamera != null)
        {
            carriedIngredientVisual.transform.SetParent(playerCamera.transform, false);
            carriedIngredientVisual.transform.localPosition = new Vector3(0.4f, -0.3f, 1.15f);
            carriedIngredientVisual.transform.localRotation = Quaternion.identity;
            Bounds heldBounds = GetBounds(carriedIngredientVisual.transform);
            float largestDimension = Mathf.Max(heldBounds.size.x, heldBounds.size.y, heldBounds.size.z);
            if (largestDimension > 0.35f)
                carriedIngredientVisual.transform.localScale *= 0.35f / largestDimension;
        }
    }

    private void CarryExistingIngredient(string item, GameObject visual)
    {
        if (visual == null || playerCamera == null)
            return;

        carriedIngredient = item;
        carriedIngredientVisual = visual;
        visual.transform.SetParent(playerCamera.transform, false);
        visual.transform.localPosition = new Vector3(0.4f, -0.3f, 1.15f);
        visual.transform.localRotation = Quaternion.identity;
        foreach (Collider itemCollider in visual.GetComponentsInChildren<Collider>(true))
            itemCollider.enabled = false;
    }

    private void PlaceCarriedIngredientOnTable()
    {
        if (assemblyTable == null)
        {
            ShowActionMessage("Assembly table not found.", 2f);
            return;
        }
        if (carriedIngredientVisual != null)
        {
            carriedIngredientVisual.transform.SetParent(null, true);
            carriedIngredientVisual.transform.localScale = carriedIngredientOriginalScale;
        }
        if (carriedIngredientVisual != null)
            ingredientVisuals[carriedIngredient] = PlaceVisualOnSurface(carriedIngredient, carriedIngredientVisual, assemblyTable);
        ingredients.Add(carriedIngredient);
        ArrangeBurgerLayersOnLid();
        ShowActionMessage("Đã đặt " + IngredientLabel(carriedIngredient) + " lên nắp thùng để ráp burger.", 2f);
        carriedIngredient = null;
        carriedIngredientVisual = null;
    }

    private void PlaceCarriedIngredientOnCuttingBoard()
    {
        if (!IsChoppableIngredient(carriedIngredient))
        {
            ShowActionMessage("Thớt dùng để thái rau, cà chua, hành và phô mai.", 2f);
            return;
        }
        if (cuttingBoard == null)
        {
            ShowActionMessage("Không tìm thấy thớt trong Kitchen.", 2f);
            return;
        }

        string item = carriedIngredient;
        if (carriedIngredientVisual != null)
        {
            carriedIngredientVisual.transform.SetParent(null, true);
            carriedIngredientVisual.transform.localScale = carriedIngredientOriginalScale;
            ingredientVisuals[item] = PlaceVisualOnSurface(item, carriedIngredientVisual, cuttingBoard);
        }
        ingredientsOnCuttingBoard.Add(item);
        ingredients.Add(item);
        ShowActionMessage("Đặt " + IngredientLabel(item) + " lên thớt. Nhấn E để dùng dao thái.", 2.5f);
        carriedIngredient = null;
        carriedIngredientVisual = null;
    }

    private bool TryStartChopping()
    {
        string[] rawIngredients = { "Lettuce", "Tomato", "Onion", "Cheese" };
        foreach (string rawItem in rawIngredients)
        {
            if (!ingredientsOnCuttingBoard.Contains(rawItem))
                continue;

            StartCoroutine(ChopIngredient(rawItem));
            return true;
        }
        return false;
    }

    private System.Collections.IEnumerator ChopIngredient(string rawItem)
    {
        isChopping = true;
        string choppedItem = "Chopped" + rawItem;
        ShowActionMessage("Dang thai " + IngredientLabel(rawItem) + "...", choppingDuration);

        if (preparationAnimator == null)
            FindPreparationAnimator();

        PlayerMovement movement = GetComponent<PlayerMovement>();
        bool movementWasEnabled = movement != null && movement.enabled;
        bool canMovePreparationModel = preparationModelRoot != null &&
                                       preparationModelRoot != transform &&
                                       playerCamera != null &&
                                       !playerCamera.transform.IsChildOf(preparationModelRoot);
        Vector3 originalModelPosition = canMovePreparationModel
            ? preparationModelRoot.position
            : Vector3.zero;
        Quaternion originalModelRotation = canMovePreparationModel
            ? preparationModelRoot.rotation
            : Quaternion.identity;
        Renderer[] modelRenderers = preparationModelRoot != null
            ? preparationModelRoot.GetComponentsInChildren<Renderer>(true)
            : new Renderer[0];
        bool[] originalRendererStates = new bool[modelRenderers.Length];
        for (int i = 0; i < modelRenderers.Length; i++)
        {
            originalRendererStates[i] = modelRenderers[i].enabled;
            string meshName = modelRenderers[i].gameObject.name;
            bool isHandMesh = meshName.Contains("CHR_Chibi_Body_Arm") ||
                              meshName.Contains("CHR_Chibi_Body_Hand");
            modelRenderers[i].enabled = isHandMesh;
        }

        if (canMovePreparationModel && cuttingBoard != null)
        {
            Bounds boardBounds = GetBounds(cuttingBoard);
            preparationModelRoot.position = new Vector3(boardBounds.center.x, boardBounds.max.y, boardBounds.center.z)
                + cuttingBoard.forward * 0.8f;
            preparationModelRoot.rotation = Quaternion.LookRotation(-cuttingBoard.forward, Vector3.up);
        }

        if (movement != null)
            movement.enabled = false;

        GameObject choppingPrefab = GetChoppingPrefab(rawItem);
        if (choppingPrefab != null &&
            ingredientVisuals.TryGetValue(rawItem, out GameObject rawVisual) && rawVisual != null)
        {
            GameObject choppingVisual = InstantiatePrefab(choppingPrefab);
            if (choppingVisual != null)
            {
                choppingVisual = PlaceVisualOnSurface(rawItem, choppingVisual, cuttingBoard);
                ingredientVisuals[rawItem] = choppingVisual;
                Destroy(rawVisual);
            }
        }

        if (preparationAnimator != null)
        {
            int stateHash = Animator.StringToHash("Cutting Vegetables");
            if (preparationAnimator.HasState(0, stateHash))
                preparationAnimator.Play(stateHash, 0, 0f);
            else
                Debug.LogWarning("Preparation Animator has no 'Cutting Vegetables' state. Assign the ChibiAnimator controller.", preparationAnimator);
        }

        yield return new WaitForSeconds(Mathf.Max(0.1f, choppingDuration));

        if (preparationAnimator != null)
        {
            int idleStateHash = Animator.StringToHash("HumanM@Idle01");
            if (preparationAnimator.HasState(0, idleStateHash))
                preparationAnimator.Play(idleStateHash, 0, 0f);
        }

        if (canMovePreparationModel)
        {
            preparationModelRoot.position = originalModelPosition;
            preparationModelRoot.rotation = originalModelRotation;
        }

        ingredients.Remove(rawItem);
        ingredients.Add(choppedItem);
        ingredientsOnCuttingBoard.Remove(rawItem);
        if (ingredientVisuals.TryGetValue(rawItem, out GameObject visual) && visual != null)
        {
            ingredientVisuals.Remove(rawItem);
            GameObject choppedPrefab = GetChoppedPrefab(rawItem);
            GameObject choppedVisual = choppedPrefab != null ? InstantiatePrefab(choppedPrefab) : null;
            if (choppedVisual != null)
            {
                Transform resultSurface = cuttingBoard != null ? cuttingBoard : assemblyTable;
                if (resultSurface != null)
                    choppedVisual = PlaceVisualOnSurface(choppedItem, choppedVisual, resultSurface);
                else
                    choppedVisual.transform.SetPositionAndRotation(visual.transform.position, visual.transform.rotation);
                choppedVisual.name = choppedItem + "_Ingredient";
                ingredientVisuals[choppedItem] = choppedVisual;
                Destroy(visual);
            }
            else
            {
                ingredientVisuals[choppedItem] = visual;
                visual.name = choppedItem + "_Ingredient";
            }
        }

        for (int i = 0; i < modelRenderers.Length; i++)
        {
            if (modelRenderers[i] != null)
                modelRenderers[i].enabled = originalRendererStates[i];
        }

        if (movement != null)
            movement.enabled = movementWasEnabled;

        ShowActionMessage("Da thai xong " + IngredientLabel(rawItem) + ".", 2f);
        isChopping = false;
    }

    private GameObject GetIngredientPrefab(string item)
    {
        switch (item)
        {
            case "RawMeat": return rawMeatPrefab;
            case "Lettuce": return lettucePrefab;
            case "Tomato": return tomatoPrefab;
            case "Onion": return onionPrefab;
            case "Cheese": return cheesePrefab;
            default: return null;
        }
    }

    private GameObject GetChoppedPrefab(string rawItem)
    {
        switch (rawItem)
        {
            case "Lettuce": return choppedLettucePrefab;
            case "Tomato": return slicedTomatoPrefab;
            case "Onion": return choppedOnionPrefab;
            case "Cheese": return choppedCheesePrefab;
            default: return null;
        }
    }

    private GameObject GetChoppingPrefab(string rawItem)
    {
        switch (rawItem)
        {
            case "Lettuce": return choppingLettucePrefab;
            case "Onion": return choppingOnionPrefab;
            case "Cheese": return choppingCheesePrefab;
            default: return null;
        }
    }

    public GameObject GetIngredientDisplayPrefab(string item)
    {
        switch (item)
        {
            case "RawMeat":
            case "CookedMeat": return rawMeatPrefab;
            case "Lettuce": return lettucePrefab;
            case "Tomato": return tomatoPrefab;
            case "Onion": return onionPrefab;
            case "Cheese": return cheesePrefab;
            case "ChoppedLettuce": return choppedLettucePrefab;
            case "ChoppedTomato": return slicedTomatoPrefab;
            case "ChoppedOnion": return choppedOnionPrefab;
            case "ChoppedCheese": return choppedCheesePrefab;
            case "BunBottom": return bunBottomPrefab;
            case "BunTop": return bunTopPrefab;
            default: return null;
        }
    }

    private GameObject SpawnIngredientOnTable(string item, GameObject prefab)
    {
        if (assemblyTable == null)
            return InstantiatePrefab(prefab);

        GameObject visual = InstantiatePrefab(prefab);
        if (visual == null)
            return null;
        return PlaceVisualOnTable(item, visual);
    }

    private GameObject PlaceVisualOnTable(string item, GameObject visual)
    {
        return PlaceVisualOnSurface(item, visual, assemblyTable);
    }

    private void ArrangeBurgerLayersOnLid()
    {
        if (assemblyTable == null)
            return;

        string[] recipeOrder =
        {
            "BunBottom", "ChoppedLettuce", "ChoppedTomato", "ChoppedOnion",
            "ChoppedCheese", "CookedMeat", "BunTop"
        };
        Bounds lidBounds = GetBounds(assemblyTable);
        float nextLayerY = lidBounds.max.y + 0.025f;

        foreach (string item in recipeOrder)
        {
            if (!ingredientVisuals.TryGetValue(item, out GameObject visual) || visual == null)
                continue;

            Bounds layerBounds = GetBounds(visual.transform);
            visual.transform.position = new Vector3(lidBounds.center.x, visual.transform.position.y, lidBounds.center.z);
            layerBounds = GetBounds(visual.transform);
            visual.transform.position += Vector3.up * (nextLayerY - layerBounds.min.y);
            nextLayerY = GetBounds(visual.transform).max.y + 0.005f;
        }
    }

    private static void EnsureClickableSurfaceCollider(GameObject surface)
    {
        if (surface == null || surface.GetComponentInChildren<Collider>(true) != null)
            return;

        Bounds bounds = GetBounds(surface.transform);
        BoxCollider box = surface.AddComponent<BoxCollider>();
        box.center = surface.transform.InverseTransformPoint(bounds.center);
        Vector3 localSize = surface.transform.InverseTransformVector(bounds.size);
        box.size = new Vector3(Mathf.Max(0.1f, Mathf.Abs(localSize.x)),
            Mathf.Max(0.1f, Mathf.Abs(localSize.y)), Mathf.Max(0.1f, Mathf.Abs(localSize.z)));
    }

    private GameObject PlaceVisualOnSurface(string item, GameObject visual, Transform surface)
    {
        visual.name = item + "_Ingredient";
        visual.transform.SetParent(null, true);
        Bounds tableBounds = GetBounds(surface);
        Vector3[] slots =
        {
            new Vector3(-0.45f, 0f, -0.32f), new Vector3(0f, 0f, -0.32f), new Vector3(0.45f, 0f, -0.32f),
            new Vector3(-0.45f, 0f, 0.32f), new Vector3(0f, 0f, 0.32f), new Vector3(0.45f, 0f, 0.32f),
            new Vector3(0f, 0f, 0f)
        };
        int slot = ingredientVisuals.Count % slots.Length;
        bool isBoard = surface == cuttingBoard;
        Vector3 slotOffset = isBoard || item == "Burger"
            ? Vector3.zero
            : surface.right * slots[slot].x + surface.forward * slots[slot].z;
        Vector3 tableSurface = new Vector3(tableBounds.center.x, tableBounds.max.y + 0.02f, tableBounds.center.z);
        visual.transform.position = tableSurface + slotOffset;
        visual.transform.rotation = isBoard ? surface.rotation : Quaternion.identity;
        if (item == "Burger")
        {
            Bounds burgerBounds = GetBounds(visual.transform);
            float burgerFootprint = Mathf.Max(burgerBounds.size.x, burgerBounds.size.z);
            float lidFootprint = Mathf.Min(tableBounds.size.x, tableBounds.size.z);
            float targetFootprint = lidFootprint * 0.82f;
            if (burgerFootprint > 0.001f && targetFootprint > 0f)
            {
                float fitScale = Mathf.Clamp(targetFootprint / burgerFootprint, 0.8f, 5f);
                visual.transform.localScale *= fitScale;
                burgerBounds = GetBounds(visual.transform);
                visual.transform.position += new Vector3(
                    tableBounds.center.x - burgerBounds.center.x,
                    0f,
                    tableBounds.center.z - burgerBounds.center.z);
            }
        }
        if (isBoard)
        {
            Bounds ingredientBounds = GetBounds(visual.transform);
            float ingredientFootprint = Mathf.Max(ingredientBounds.size.x, ingredientBounds.size.z);
            float boardFootprint = Mathf.Min(tableBounds.size.x, tableBounds.size.z);
            float targetFootprint = boardFootprint * 0.62f;
            if (ingredientFootprint > targetFootprint && ingredientFootprint > 0.001f)
                visual.transform.localScale *= targetFootprint / ingredientFootprint;
        }
        Bounds visualBounds = GetBounds(visual.transform);
        visual.transform.position += Vector3.up * (tableSurface.y - visualBounds.min.y);

        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        foreach (Collider itemCollider in colliders)
            Destroy(itemCollider);
        AddClickableCollider(visual);

        return visual;
    }

    private static void AddClickableCollider(GameObject visual)
    {
        if (visual == null)
            return;

        Bounds bounds = GetBounds(visual.transform);
        BoxCollider box = visual.AddComponent<BoxCollider>();
        box.center = visual.transform.InverseTransformPoint(bounds.center);
        Vector3 localSize = visual.transform.InverseTransformVector(bounds.size);
        box.size = new Vector3(Mathf.Max(0.08f, Mathf.Abs(localSize.x)),
            Mathf.Max(0.08f, Mathf.Abs(localSize.y)), Mathf.Max(0.08f, Mathf.Abs(localSize.z)));
    }

    private static bool IsChoppableIngredient(string item)
    {
        return item == "Lettuce" || item == "Tomato" || item == "Onion" || item == "Cheese";
    }

    private static Bounds GetBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(root.position, Vector3.one);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private void SetPanMeatColor(Color color)
    {
        if (panMeatRenderer != null)
            panMeatRenderer.material.color = color;
    }

    private void AssembleBurger()
    {
        if (HasFood)
        {
            ShowActionMessage("Tay đang cầm món. Giao món cho khách trước.", 2f);
            return;
        }
        if (!ingredients.Contains("BunBottom") || !ingredients.Contains("BunTop"))
        {
            ShowActionMessage("Cần có vỏ bánh dưới và vỏ bánh trên để ráp hamburger.", 2.5f);
            return;
        }
        bool usesBurntMeat = ingredients.Contains("BurntMeat");
        string meatIngredient = usesBurntMeat ? "BurntMeat" : "CookedMeat";
        if (!ingredients.Contains(meatIngredient))
        {
            ShowActionMessage("Cần có thịt chín để ráp hamburger.", 2.5f);
            return;
        }

        List<string> recipe = new List<string> { "BunBottom", meatIngredient };
        string[] toppings = { "ChoppedLettuce", "ChoppedTomato", "ChoppedOnion", "ChoppedCheese" };
        foreach (string topping in toppings)
        {
            if (ingredients.Contains(topping))
                recipe.Add(topping);
        }
        recipe.Add("BunTop");
        string builtFoodName = GetBuiltBurgerName(recipe);
        if (assemblyTable == null || (packagedBurgerPrefab == null && assembledBurgerPrefab == null))
        {
            ShowActionMessage("Assembly table or burger prefab is missing.", 2.5f);
            return;
        }
        foreach (string item in recipe)
            ingredients.Remove(item);
        foreach (string item in recipe)
        {
            if (ingredientVisuals.TryGetValue(item, out GameObject visual) && visual != null)
                Destroy(visual);
            ingredientVisuals.Remove(item);
        }
        GameObject finishedBurger = InstantiatePrefab(
            packagedBurgerPrefab != null ? packagedBurgerPrefab : assembledBurgerPrefab);
        if (finishedBurger != null)
        {
            if (servingTrayPrefab != null)
                AddServingTray(finishedBurger);
            finishedBurger = PlaceVisualOnSurface("Burger", finishedBurger, assemblyTable);
            FoodItem foodItem = finishedBurger.GetComponent<FoodItem>();
            if (foodItem == null)
                foodItem = finishedBurger.AddComponent<FoodItem>();
            foodItem.SetFoodName(builtFoodName);
            foodItem.SetContainsBurntMeat(usesBurntMeat);
        }
        ShowActionMessage("Burger đã được đóng hộp và đặt lên đĩa. Click để mang giao khách.", 3f);
    }

    private static string GetBuiltBurgerName(List<string> recipe)
    {
        bool lettuce = recipe.Contains("ChoppedLettuce");
        bool tomato = recipe.Contains("ChoppedTomato");
        bool onion = recipe.Contains("ChoppedOnion");
        bool cheese = recipe.Contains("ChoppedCheese");

        if (lettuce && tomato && onion && cheese)
            return "Burger";
        if (lettuce && tomato && !onion && !cheese)
            return "BurgerLettuceTomato";
        if (cheese && !lettuce && !tomato && !onion)
            return "CheeseBurger";
        if (!lettuce && !tomato && !onion && !cheese)
            return "BeefBurger";

        return "CustomBurger";
    }

    private void AddServingTray(GameObject packagedFood)
    {
        GameObject tray = InstantiatePrefab(servingTrayPrefab);
        if (tray == null)
            return;

        Bounds foodBounds = GetBounds(packagedFood.transform);
        Bounds trayBounds = GetBounds(tray.transform);
        float foodFootprint = Mathf.Max(foodBounds.size.x, foodBounds.size.z);
        float trayFootprint = Mathf.Min(trayBounds.size.x, trayBounds.size.z);
        if (foodFootprint > 0.001f && trayFootprint > 0.001f)
        {
            float fitScale = Mathf.Clamp(trayFootprint * 0.76f / foodFootprint, 0.75f, 3f);
            packagedFood.transform.localScale *= fitScale;
            foodBounds = GetBounds(packagedFood.transform);
        }

        Vector3 trayPosition = tray.transform.position;
        trayPosition.x += foodBounds.center.x - trayBounds.center.x;
        trayPosition.z += foodBounds.center.z - trayBounds.center.z;
        trayPosition.y += foodBounds.min.y - trayBounds.max.y + 0.01f;
        tray.transform.position = trayPosition;
        tray.transform.SetParent(packagedFood.transform, true);
    }

    private bool TryPickUpFood()
    {
        if (playerCamera == null)
            return false;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, foodPickupDistance, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            FoodItem item = hit.collider.GetComponentInParent<FoodItem>();
            if (item == null)
                continue;

            carriedFood = item.FoodName;
            carriedVisual = item.gameObject;
            carriedVisual.transform.SetParent(playerCamera.transform, false);
            carriedVisual.transform.localPosition = new Vector3(0.35f, -0.3f, 0.7f);
            carriedVisual.transform.localRotation = Quaternion.identity;
            foreach (Collider itemCollider in carriedVisual.GetComponentsInChildren<Collider>(true))
                itemCollider.enabled = false;
            ShowActionMessage("Đã lấy " + carriedFood + ".", 2f);
            return true;
        }
        return false;
    }

    private bool TryPickUpPlacedIngredient()
    {
        if (playerCamera == null || ingredientVisuals.Count == 0)
            return false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, foodPickupDistance, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            foreach (KeyValuePair<string, GameObject> entry in ingredientVisuals)
            {
                GameObject visual = entry.Value;
                if (visual == null || (hit.transform != visual.transform && !hit.transform.IsChildOf(visual.transform)))
                    continue;

                string item = entry.Key;
                ingredients.Remove(item);
                ingredientsOnCuttingBoard.Remove(item);
                carriedIngredientOriginalScale = visual.transform.localScale;
                ingredientVisuals.Remove(item);
                CarryExistingIngredient(item, visual);
                float largestDimension = Mathf.Max(GetBounds(visual.transform).size.x,
                    GetBounds(visual.transform).size.y, GetBounds(visual.transform).size.z);
                if (largestDimension > 0.35f)
                    visual.transform.localScale *= 0.35f / largestDimension;
                ShowActionMessage("Đã lấy " + IngredientLabel(item) + ". Click chuột trái vào bàn giữa để đặt.", 2.5f);
                return true;
            }
        }
        return false;
    }

    private CustomerAI FindLookedAtCustomer()
    {
        CustomerAI best = null;
        float bestAngle = customerLookAngle;
        foreach (CustomerAI customer in FindObjectsOfType<CustomerAI>())
        {
            if (customer == null || !customer.IsNearPlayer(transform))
                continue;
            Vector3 toCustomer = customer.LookTarget - playerCamera.transform.position;
            float distance = toCustomer.magnitude;
            if (distance <= 0.01f || distance > customerLookDistance)
                continue;
            float angle = Vector3.Angle(playerCamera.transform.forward, toCustomer);
            if (angle <= bestAngle)
            {
                best = customer;
                bestAngle = angle;
            }
        }
        return best;
    }

    private CustomerAI FindClickedCustomer()
    {
        if (playerCamera == null)
            return null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, customerLookDistance, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            CustomerAI customer = hit.collider.GetComponentInParent<CustomerAI>();
            if (customer != null && customer.IsNearPlayer(transform))
                return customer;
        }

        return null;
    }

    private static string IngredientLabel(string item)
    {
        switch (item)
        {
            case "RawMeat": return "thịt sống";
            case "Lettuce": return "rau";
            case "Tomato": return "cà chua";
            case "Onion": return "hành tây";
            case "Cheese": return "phô mai";
            case "ChoppedLettuce": return "rau đã thái";
            case "ChoppedTomato": return "cà chua đã thái";
            case "ChoppedOnion": return "hành tây đã thái";
            case "ChoppedCheese": return "phô mai đã thái";
            case "BunBottom": return "vỏ bánh dưới";
            case "BunTop": return "vỏ bánh trên";
            case "CookedMeat": return "thịt chín";
            default: return item;
        }
    }

    private void ShowActionMessage(string message, float duration)
    {
        actionMessage = message;
        actionMessageUntil = Time.time + duration;
    }

    private void OnGUI()
    {
        DrawAimDot();

        string message = meatOnPan
            ? "Thịt trên chảo: " + (meatBurnt ? "CHÁY ĐEN" : meatCooked ? "CHÍN NÂU" : "SỐNG ĐỎ")
            : Time.time < actionMessageUntil ? actionMessage : null;
        if (string.IsNullOrEmpty(message))
            return;
        Rect panel = new Rect(Screen.width * 0.5f - 260f, Screen.height - 85f, 520f, 42f);
        GUI.Box(panel, GUIContent.none);
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 18;
        style.normal.textColor = meatOnPan ? meatBurnt ? Color.black : meatCooked ? new Color(0.45f, 0.2f, 0.05f) : Color.red : Color.white;
        GUI.Label(panel, message, style);
    }

    private void DrawAimDot()
    {
        if (FridgeUI.IsOpen)
            return;

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        GUIStyle dotStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };
        dotStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(Screen.width * 0.5f - 6f, Screen.height * 0.5f - 8f, 12f, 16f), "●", dotStyle);
    }

    private void UpdateMoneyText()
    {
        if (moneyText != null)
            moneyText.text = money + "$";
    }

    private static GameObject InstantiatePrefab(GameObject prefab)
    {
        if (prefab == null)
            return null;

        UnityEngine.Object instance = UnityEngine.Object.Instantiate((UnityEngine.Object)prefab);
        GameObject instanceObject = instance as GameObject;
        if (instanceObject == null)
        {
            Debug.LogError("Could not instantiate prefab as a GameObject: " + prefab.name);
            if (instance != null)
                Destroy(instance);
        }
        return instanceObject;
    }
}
