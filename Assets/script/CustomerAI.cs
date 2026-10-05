using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class CustomerAI : MonoBehaviour
{
    private Animator animator;
    public NavMeshAgent agent;
    public Transform orderPoint;

    [Header("Order")]
    [SerializeField] private string requestedFood = "Burger";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform player;
    [SerializeField] private TextMeshPro orderText;

    private bool hasOrdered = false;
    private bool orderAccepted = false;

    void Start()
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

        CreateOrderText();

        if (orderPoint != null && agent != null)
        {
            agent.SetDestination(orderPoint.position);
            StartWalking();
        }
    }

    void Update()
    {
        if (agent != null && !agent.pathPending && !hasOrdered)
        {
            // Kiểm tra nếu khoảng cách còn lại nhỏ hơn hoặc bằng stoppingDistance (hoặc khoảng nhỏ 0.8)
            if (agent.remainingDistance <= agent.stoppingDistance + 0.3f)
            {
                // Dừng di chuyển và phát Animation gọi món
                StopAndWait();
                OrderFood();
                hasOrdered = true; // Đánh dấu đã nhận lệnh gọi món
                ShowOrder();
            }
        }

        if (hasOrdered && !orderAccepted && IsPlayerInRange() && Input.GetKeyDown(KeyCode.E))
        {
            AcceptOrder();
        }
    }

    public void StartWalking()
    {
        if (animator != null) animator.SetBool("IsWalking", true);
    }

    public void StopAndWait()
    {
        if (animator != null) animator.SetBool("IsWalking", false);
    }

    public void OrderFood()
    {
        if (animator != null)
        {
            animator.SetTrigger("IsOrdering");
        }
    }

    private bool IsPlayerInRange()
    {
        return player != null &&
               Vector3.Distance(player.position, transform.position) <= interactionDistance;
    }

    private void CreateOrderText()
    {
        if (orderText != null)
            return;

        GameObject textObject = new GameObject("OrderText");
        textObject.transform.SetParent(transform);
        textObject.transform.localPosition = new Vector3(0f, 2.4f, 0f);

        orderText = textObject.AddComponent<TextMeshPro>();
        orderText.alignment = TextAlignmentOptions.Center;
        orderText.fontSize = 2.5f;
        orderText.color = Color.yellow;
        orderText.rectTransform.sizeDelta = new Vector2(4f, 1f);
        orderText.gameObject.SetActive(false);
    }

    private void ShowOrder()
    {
        if (orderText == null)
            return;

        orderText.text = "Order: " + requestedFood + "\nPress E";
        orderText.gameObject.SetActive(true);
    }

    private void AcceptOrder()
    {
        orderAccepted = true;

        if (orderText != null)
        {
            orderText.text = "Order accepted";
            Invoke(nameof(HideOrder), 1.5f);
        }
    }

    private void HideOrder()
    {
        if (orderText != null)
            orderText.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (orderText == null || !orderText.gameObject.activeSelf || Camera.main == null)
            return;

        orderText.transform.rotation = Camera.main.transform.rotation;
    }
}