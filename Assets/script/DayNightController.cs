using UnityEngine;

public class DayNightController : MonoBehaviour
{
    [SerializeField] private float dayDuration = 30f; // Duration of a full day in seconds

    // Update is called once per frame
    void Update()
    {
        float rotationSpeed = 360f / dayDuration; // Calculate rotation speed based on day duration
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime); // Rotate the object around the x-axis
    }
}
