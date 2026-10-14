using UnityEngine;
using System.Collections.Generic;

public class ColetorDePaginas : MonoBehaviour
{
    [Header("Configurações")]
    public GameObject paginaPrefab;

    // A pilha que guarda os números das páginas (Stack = Pilha em inglês)
    public Stack<int> pilhaDePaginas = new Stack<int>();

    private void OnTriggerEnter2D(Collider2D colisao)
    {
        if (colisao.CompareTag("Pagina"))
        {
            // Acede ao script da página apanhada para descobrir qual é o seu número
            DadosDaPagina dados = colisao.GetComponent<DadosDaPagina>();

            if (dados != null)
            {
                // Push: Adiciona o número específico no topo da pilha
                pilhaDePaginas.Push(dados.numeroDaPagina);
                Debug.Log("Recolheu a página " + dados.numeroDaPagina + "! Total na pilha: " + pilhaDePaginas.Count);

                Destroy(colisao.gameObject);
            }
        }
    }

    private void Update()
    {
        // Pop: Solta a página ao carregar no Espaço (se houver alguma na pilha)
        if (Input.GetKeyDown(KeyCode.Space) && pilhaDePaginas.Count > 0)
        {
            // Retira o último número que entrou na pilha
            int numeroRemovido = pilhaDePaginas.Pop();
            Debug.Log("Soltou a página " + numeroRemovido + "! Restam: " + pilhaDePaginas.Count);

            // Cria a página novamente no chão, um pouco abaixo do jogador
            Vector2 posicaoDeSoltar = (Vector2)transform.position + new Vector2(0f, -1.5f);
            GameObject paginaSolta = Instantiate(paginaPrefab, posicaoDeSoltar, Quaternion.identity);

            // Devolve a identidade correta à página que acabou de ser criada
            paginaSolta.GetComponent<DadosDaPagina>().numeroDaPagina = numeroRemovido;
            paginaSolta.name = "Pagina_" + numeroRemovido;
        }
    }
}