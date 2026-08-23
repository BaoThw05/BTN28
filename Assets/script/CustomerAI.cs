using UnityEngine;
using UnityEngine.AI;

public class CustomerAI : MonoBehaviour
{
    private Animator animator;
    public NavMeshAgent agent;
    public Transform orderPoint;

    private bool hasOrdered = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

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
            }
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
}