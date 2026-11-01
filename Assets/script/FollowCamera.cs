using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Cài đặt mục tiêu")]
    [SerializeField] private Transform target; // Kéo nhân vật chính (Player) vào đây

    [Header("Cài đặt vị trí Camera")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f);
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);
    [Header("Xoay camera bằng chuột")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Cải tiến chuyển động và va chạm")]
    [SerializeField] private float lookAhead = 2.5f; // Dự đoán hướng di chuyển của nhân vật
    [SerializeField] private LayerMask obstacles;   // Layer các vật cản (tường, nhà...) để chống xuyên tường

    private Vector3 velocity;
    private CharacterController targetCC;
    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x > 180f ? angles.x - 360f : angles.x;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Chỉ xoay camera, tuyệt đối không thay đổi rotation của target/player.
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);

        // Tự động tìm CharacterController của nhân vật để tính vận tốc di chuyển
        if (targetCC == null)
        {
            targetCC = target.GetComponent<CharacterController>();
        }

        // 1. Tính toán điểm dự đoán phía trước nhân vật khi di chuyển
        Vector3 ahead = Vector3.zero;
        if (targetCC != null)
        {
            ahead = targetCC.velocity.normalized * lookAhead;
            ahead.y = 0f;
        }

        // 2. Tính toán vị trí muốn camera đứng tới (Wanted Position)
        Vector3 wanted = target.position + cameraRotation * offset + ahead;

        // 3. Xử lý va chạm chống xuyên tường
        Vector3 pivot = target.position + lookAtOffset;
        Vector3 dir = (wanted - pivot).normalized;
        float dist = Vector3.Distance(pivot, wanted);

        if (Physics.SphereCast(pivot, 0.3f, dir, out RaycastHit hit, dist, obstacles))
        {
            wanted = hit.point - dir * 0.25f; // Kéo camera ra trước tường một chút để không bị kẹt hay lọt vào tường
        }

        // 4. Cập nhật vị trí và góc nhìn mượt mà cho Camera
        transform.position = Vector3.SmoothDamp(transform.position, wanted, ref velocity, smoothTime);
        transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
    }
}
