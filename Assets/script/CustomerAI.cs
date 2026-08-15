using UnityEngine;

public class CustomerAI : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Lấy component Animator gắn trên nhân vật
        animator = GetComponent<Animator>();

        // Vừa xuất hiện -> Cho đi bộ vào quầy
        StartWalking();
    }

    // Gọi hàm này khi khách bắt đầu đi bộ
    public void StartWalking()
    {
        animator.SetBool("IsWalking", true);
    }

    // Gọi hàm này khi khách đã đi tới quầy xếp hàng (dừng lại)
    public void StopAndWait()
    {
        animator.SetBool("IsWalking", false);
    }

    // Gọi hàm này khi đến lượt khách gọi món
    public void OrderFood()
    {
        animator.SetTrigger("IsOrdering");
    }
}