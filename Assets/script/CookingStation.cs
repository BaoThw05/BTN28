using UnityEngine;

public class CookingStation : MonoBehaviour
{
    public enum StationType { Fridge, BunCrate, AssemblyTable, CuttingBoard, Pan }
    [SerializeField] private StationType stationType;
    public StationType Type => stationType;

    public void Configure(StationType type) => stationType = type;

    public Vector3 LookTarget
    {
        get
        {
            if (TryGetBounds(out Bounds bounds))
                return bounds.center;
            return transform.position + Vector3.up;
        }
    }

    public Vector3 SurfacePoint
    {
        get
        {
            if (TryGetBounds(out Bounds bounds))
                return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            return transform.position;
        }
    }

    private bool TryGetBounds(out Bounds bounds)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            bounds = new Bounds(transform.position, Vector3.zero);
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return true;
    }

    public void Interact(PlayerOrderInteraction player)
    {
        if (player != null)
            player.InteractWithStation(this);
    }
}
