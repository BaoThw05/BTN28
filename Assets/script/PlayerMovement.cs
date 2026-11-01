using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraTransform;

    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private readonly Collider[] groundHits = new Collider[8];

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller == null) return;

        // CharacterController.isGrounded detects TerrainCollider directly and does not
        // depend on the scene's groundMask layer being configured correctly.
        bool isGrounded = controller.isGrounded || CheckGroundBelow();

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        Transform movementReference = cameraTransform != null ? cameraTransform : transform;
        Vector3 cameraForward = movementReference.forward;
        Vector3 cameraRight = movementReference.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 move = cameraRight * x + cameraForward * z;

        controller.Move(move * speed * Time.deltaTime);

        //check if the player is on the ground so he can jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            //the equation for jumping
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private bool CheckGroundBelow()
    {
        Bounds bounds = controller.bounds;
        float radius = Mathf.Max(0.12f, controller.radius * 0.5f);
        Vector3 probeCenter = new Vector3(bounds.center.x, bounds.min.y + radius * 0.5f, bounds.center.z);

        // Dùng mọi layer để nhận Terrain ở cả Town lẫn Kitchen; bỏ qua collider của player.
        int hitCount = Physics.OverlapSphereNonAlloc(
            probeCenter,
            radius,
            groundHits,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = groundHits[i];
            if (hit != null && hit != controller && !hit.transform.IsChildOf(transform))
                return true;
        }

        return false;
    }
}
