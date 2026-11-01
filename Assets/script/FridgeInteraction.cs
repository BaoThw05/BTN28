using UnityEngine;

public class FridgeInteraction : MonoBehaviour
{
    [SerializeField] private float useDistance = 5f;
    [SerializeField] private float doorOpenAngle = 105f;
    [SerializeField] private float doorAnimationSpeed = 240f;

    private class Door
    {
        public Transform transform;
        public Quaternion closedRotation;
    }

    private readonly System.Collections.Generic.List<Door> doors = new System.Collections.Generic.List<Door>();
    private PlayerOrderInteraction player;
    private Camera playerCamera;
    private FridgeUI fridgeUI;
    private BoxCollider interactionCollider;
    private Bounds fridgeBounds;
    private bool isOpen;
    private bool initialized;

    public void Initialize(PlayerOrderInteraction orderPlayer, Camera cameraToUse,
        string[] itemNames, GameObject[] itemPrefabs)
    {
        if (initialized)
            return;

        player = orderPlayer;
        playerCamera = cameraToUse != null ? cameraToUse : Camera.main;
        if (playerCamera == null)
            return;

        if (!TryGetRendererBounds(transform, out fridgeBounds))
            fridgeBounds = new Bounds(transform.position + transform.up, Vector3.one);

        FindDoors();
        AddInteractionCollider();
        fridgeUI = GetComponent<FridgeUI>();
        if (fridgeUI == null)
            fridgeUI = gameObject.AddComponent<FridgeUI>();
        fridgeUI.Initialize(this, player, playerCamera, itemNames, itemPrefabs);
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        AnimateDoors();
        if (fridgeUI != null && fridgeUI.IsVisible)
            return;

        if (Input.GetMouseButtonDown(0) && IsNearFridge() && IsLookingAtFridge())
            OpenFridgeUI();
    }

    private void FindDoors()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child == transform || !child.name.ToLowerInvariant().Contains("door"))
                continue;

            doors.Add(new Door { transform = child, closedRotation = child.localRotation });
        }
    }

    private void AddInteractionCollider()
    {
        interactionCollider = GetComponent<BoxCollider>();
        if (interactionCollider == null)
            interactionCollider = gameObject.AddComponent<BoxCollider>();

        interactionCollider.isTrigger = true;
        interactionCollider.center = transform.InverseTransformPoint(fridgeBounds.center);
        Vector3 localSize = transform.InverseTransformVector(fridgeBounds.size);
        interactionCollider.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
    }

    private bool IsNearFridge()
    {
        return playerCamera != null &&
               Vector3.Distance(playerCamera.transform.position, fridgeBounds.center) <= useDistance;
    }

    private bool IsLookingAtFridge()
    {
        Vector3 pointer = Cursor.lockState == CursorLockMode.Locked
            ? new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
            : Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(pointer);
        RaycastHit[] hits = Physics.RaycastAll(ray, useDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                return true;
        }
        return false;
    }

    public void OpenFridgeUI()
    {
        if (!initialized || !IsNearFridge() || fridgeUI == null)
            return;

        isOpen = true;
        fridgeUI.OpenFridgeUI();
    }

    public void CloseFridge()
    {
        isOpen = false;
    }

    private void AnimateDoors()
    {
        foreach (Door door in doors)
        {
            if (door.transform == null)
                continue;

            Quaternion target = door.closedRotation;
            if (isOpen)
                target *= Quaternion.Euler(0f, -doorOpenAngle, 0f);
            door.transform.localRotation = Quaternion.RotateTowards(
                door.transform.localRotation, target, doorAnimationSpeed * Time.deltaTime);
        }
    }

    private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            bounds = new Bounds(root.position, Vector3.zero);
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return true;
    }
}
