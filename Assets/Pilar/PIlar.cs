using UnityEngine;

public class Pilar : MonoBehaviour
{
    [Header("Configurações do Pilar")]
    public int numeroDoPilar; // Define qual página este pilar aceita (1 a 8)
    public bool pilarConcluido = false;

    private bool jogadorProximo = false;
    private ColetorDePaginas coletorDoJogador;

    private void OnTriggerEnter2D(Collider2D colisao)
    {
        if (colisao.CompareTag("Player"))
        {
            jogadorProximo = true;
            coletorDoJogador = colisao.GetComponent<ColetorDePaginas>();
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
        // 1. Verifica se o jogador tem alguma página na pilha
        if (coletorDoJogador.pilhaDePaginas.Count == 0)
        {
            Debug.Log("Você não tem nenhuma página na sua pilha!");
            return;
        }

        // 2. Peek() olha qual página está no TOPO da pilha sem removê-la
        int paginaNoTopo = coletorDoJogador.pilhaDePaginas.Peek();

        // 3. Se a página do topo for a correta para este pilar
        if (paginaNoTopo == numeroDoPilar)
        {
            // Pop() remove a página do topo da pilha
            coletorDoJogador.pilhaDePaginas.Pop();
            pilarConcluido = true;

            Debug.Log($"[SUCESSO] Página {numeroDoPilar} entregue ao Pilar {numeroDoPilar}!");

            // Feedback visual: Muda a cor do pilar para verde
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.green;
        }
        else
        {
            Debug.Log($"[RECUSADO] O Pilar {numeroDoPilar} exige a página {numeroDoPilar}, mas no topo da sua pilha está a página {paginaNoTopo}!");
        }
    }
}