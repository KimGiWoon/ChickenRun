using UnityEngine;

public class moveTest : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // 이동 속도

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 입력 받기 (WASD 또는 방향키)
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D 또는 ←/→
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S 또는 ↑/↓

        moveInput.Normalize(); // 대각선 이동 속도 보정
    }

    void FixedUpdate()
    {
        // 실제 이동
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
