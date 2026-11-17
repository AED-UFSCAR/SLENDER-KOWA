using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 ultimaDirecao = Vector2.up;

    // Dash (habilidade especial da trilha de velocidade)
    private float dashAte = -1f;
    private float velocidadeDash;
    private Vector2 direcaoDash;

    public bool EmDash => Time.time < dashAte;
    public Vector2 UltimaDirecao => ultimaDirecao;

    void Start()
    {
        // Pega a referência do Rigidbody2D anexado ao Player
        rb = GetComponent<Rigidbody2D>();
        ArvoreHabilidades.Inicializar();

        // Garante que o jogador tenha o controle das habilidades especiais
        if (GetComponent<HabilidadesEspeciais>() == null)
        {
            gameObject.AddComponent<HabilidadesEspeciais>();
        }
    }

    void Update()
    {
        // Captura as teclas W, A, S, D ou as setas do teclado
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Evita que o personagem ande mais rápido ao se mover na diagonal
        moveInput = moveInput.normalized;
        if (moveInput != Vector2.zero) ultimaDirecao = moveInput;
    }

    void FixedUpdate()
    {
        if (EmDash)
        {
            rb.linearVelocity = direcaoDash * velocidadeDash;
            return;
        }

        // A velocidade final usa o multiplicador vindo da árvore de habilidades
        rb.linearVelocity = moveInput * moveSpeed * ArvoreHabilidades.Status.multVelocidade;
    }

    // Avança rápido na direção em que o jogador está andando
    public void IniciarDash(float multiplicador, float duracao)
    {
        direcaoDash = ultimaDirecao;
        velocidadeDash = moveSpeed * ArvoreHabilidades.Status.multVelocidade * multiplicador;
        dashAte = Time.time + duracao;
    }
}
