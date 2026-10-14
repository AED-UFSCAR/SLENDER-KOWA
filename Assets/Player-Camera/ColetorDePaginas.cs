using UnityEngine;
using System.Collections.Generic;

public class ColetorDePaginas : MonoBehaviour
{
    [Header("Configurações")]
    public GameObject paginaPrefab;

    // A pilha que guarda os números das páginas (Stack = Pilha em inglês)
    public PilhaPaginas pilhaDePaginas = new PilhaPaginas(8);

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

}