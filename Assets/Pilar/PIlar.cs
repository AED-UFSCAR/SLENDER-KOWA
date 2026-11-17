using UnityEngine;

public class Pilar : MonoBehaviour
{
    [Header("Configurações do Pilar")]
    public int numeroDoPilar; // Define qual página este pilar aceita (1 a 8)
    public bool pilarConcluido = false;

    private bool jogadorProximo = false;
    private ColetorDePaginas coletorDoJogador;

    private void Awake()
    {
        // O pilar não deve se mover. Alguns pilares estavam com Rigidbody2D
        // dinâmico e gravidade 1, e caíam para fora do mapa ao dar Play.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D colisao)
    {
        if (colisao.CompareTag("Player"))
        {
            jogadorProximo = true;
            // GetComponentInParent também funciona se o collider estiver num filho do Player
            coletorDoJogador = colisao.GetComponentInParent<ColetorDePaginas>();
        }
    }

    private void OnTriggerExit2D(Collider2D colisao)
    {
        if (colisao.CompareTag("Player"))
        {
            jogadorProximo = false;
            coletorDoJogador = null;
        }
    }

    private void Update()
    {
        // Pressione a tecla 'E' perto do pilar para tentar entregar
        if (jogadorProximo && Input.GetKeyDown(KeyCode.E) && !pilarConcluido && coletorDoJogador != null)
        {
            TentarEntregarPagina();
        }
    }

    void TentarEntregarPagina()
    {
        PilhaPaginas pilha = coletorDoJogador.pilhaDePaginas;

        // 1. Verifica se o jogador tem alguma página na pilha
        if (pilha.EstaVazia())
        {
            Debug.Log("Você não tem nenhuma página na sua pilha!");
            return;
        }

        // 2. Peek() olha qual página está no TOPO da pilha sem removê-la
        int paginaNoTopo = pilha.Peek();

        // 3. Se a página do topo for a correta para este pilar
        if (paginaNoTopo == numeroDoPilar)
        {
            // Pop() remove a página do topo da pilha
            pilha.Pop();
            pilarConcluido = true;

            Debug.Log($"[SUCESSO] Página {numeroDoPilar} entregue ao Pilar {numeroDoPilar}! Pilha agora: {pilha}");

            // Cada página entregue vale 1 ponto para a árvore de habilidades
            ArvoreHabilidades.GanharPonto();

            // Feedback visual: muda a cor do pilar para verde
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.green;

            VerificarVitoria();
        }
        else
        {
            Debug.Log($"[RECUSADO] O Pilar {numeroDoPilar} exige a página {numeroDoPilar}, mas no topo da sua pilha está a página {paginaNoTopo}!");
        }
    }

    // Quando todos os pilares da cena estiverem concluídos, o jogador venceu
    void VerificarVitoria()
    {
        Pilar[] pilares = FindObjectsByType<Pilar>(FindObjectsSortMode.None);
        foreach (Pilar p in pilares)
        {
            if (!p.pilarConcluido) return;
        }

        Debug.Log($"[VITÓRIA] Todos os {pilares.Length} pilares receberam suas páginas!");
        if (MenuPrincipal.Instancia != null)
        {
            MenuPrincipal.Instancia.MostrarFimDeJogo(true, $"Você entregou as {pilares.Length} páginas!");
        }
    }
}
