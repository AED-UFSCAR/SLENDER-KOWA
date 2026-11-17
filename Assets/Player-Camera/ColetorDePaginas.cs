using UnityEngine;

public class ColetorDePaginas : MonoBehaviour
{
    [Header("Configurações")]
    public GameObject paginaPrefab;

    // A pilha que guarda os números das páginas
    public PilhaPaginas pilhaDePaginas = new PilhaPaginas(8);

    private void OnTriggerEnter2D(Collider2D colisao)
    {
        if (!colisao.CompareTag("Pagina")) return;

        // Acessa o script da página coletada para descobrir qual é o seu número
        DadosDaPagina dados = colisao.GetComponent<DadosDaPagina>();
        if (dados == null) return;

        // Push: adiciona o número da página no topo da pilha.
        // Se a pilha estiver cheia, a página fica no chão (não é destruída e perdida).
        if (!pilhaDePaginas.Push(dados.numeroDaPagina)) return;

        Debug.Log("Coletou a página " + dados.numeroDaPagina + "! Pilha (topo -> base): " + pilhaDePaginas
                  + " | Total: " + pilhaDePaginas.Count);

        // Desliga o collider na hora: o Destroy só acontece no fim do frame,
        // e sem isso a mesma página poderia ser empilhada duas vezes.
        colisao.enabled = false;
        Destroy(colisao.gameObject);
    }
}
