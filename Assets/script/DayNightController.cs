using UnityEngine;

/// <summary>Điều khiển hướng mặt trời theo cùng giờ với TimeController.</summary>
public class DayNightController : MonoBehaviour
{
    [SerializeField] private TimeController timeController;

    public void SetTimeController(TimeController controller)
    {
        timeController = controller;
    }

    private void Start()
    {
        if (timeController == null)
            timeController = TimeController.Instance;
    }

    private void Update()
    {
        if (timeController == null)
            timeController = TimeController.Instance;
        if (timeController == null) return;

        transform.rotation = Quaternion.AngleAxis(timeController.SunRotationAngle, Vector3.right);
    }
}
