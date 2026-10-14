using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        // Pega a referência do Rigidbody2D anexado ao Player
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Captura as teclas W, A, S, D ou as setas do teclado
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Evita que o personagem ande mais rápido ao se mover na diagonal
        moveInput = moveInput.normalized;
    }

    void FixedUpdate()
    {
        // Aplica a velocidade diretamente na física do Rigidbody2D
        rb.linearVelocity = moveInput * moveSpeed;
    }
}