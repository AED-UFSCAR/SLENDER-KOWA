using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Configurações")]
    public GameObject paginaPrefab; // O molde da página
    public float raioMapa = 15f;    // Deve ser o mesmo raio da sua borda circular

    void Start()
    {
        // O loop vai rodar 7 vezes, de 1 até 7
        for (int i = 1; i <= 8; i++)
        {
            // Gera uma posição aleatória dentro do círculo
            Vector2 posicaoAleatoria = Random.insideUnitCircle * raioMapa;

            // Cria a página na cena
            GameObject novaPagina = Instantiate(paginaPrefab, posicaoAleatoria, Quaternion.identity);

            // Acessa o script da página e define o número dela
            DadosDaPagina dados = novaPagina.GetComponent<DadosDaPagina>();
            if (dados != null)
            {
                dados.numeroDaPagina = i;
                novaPagina.name = "Pagina_" + i; // Muda o nome do objeto na Hierarchy para facilitar a leitura
            }
        }
    }
}