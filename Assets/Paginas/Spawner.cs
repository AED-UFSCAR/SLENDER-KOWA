using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Configurações")]
    public GameObject paginaPrefab; // O molde da página
    public float raioMapa = 15f;    // Deve ser o mesmo raio da sua borda circular

    [Header("Pilhas para a lanterna")]
    public int pilhasIniciais = 4;
    public int maximoPilhasNoMapa = 3;
    public float intervaloNovaPilha = 25f;
    private float proximaPilha;

    // Acesso fácil para outros scripts (ex: o Kowa jogar uma página longe)
    public static Spawner Instancia { get; private set; }

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // O loop vai rodar 8 vezes, de 1 até 8
        for (int i = 1; i <= 8; i++)
        {
            CriarPagina(i, PosicaoAleatoria());
        }

        // Pilhas espalhadas para recarregar a lanterna
        for (int i = 0; i < pilhasIniciais; i++) ItemBateria.Criar(PosicaoAleatoria());
        proximaPilha = Time.time + intervaloNovaPilha;

        // Cria a criatura, se ainda não houver uma na cena
        if (FindFirstObjectByType<Kowa>() == null)
        {
            Kowa.Criar();
        }
    }

    // De tempos em tempos aparece uma pilha nova (até um limite no mapa)
    void Update()
    {
        if (Time.time < proximaPilha) return;
        proximaPilha = Time.time + intervaloNovaPilha;

        if (FindObjectsByType<ItemBateria>(FindObjectsSortMode.None).Length < maximoPilhasNoMapa)
        {
            ItemBateria.Criar(PosicaoAleatoria());
        }
    }

    // Posição aleatória dentro do círculo (90% do raio, para não ficar colada na borda)
    public Vector2 PosicaoAleatoria()
    {
        return Random.insideUnitCircle * (raioMapa * 0.9f);
    }

    public GameObject CriarPagina(int numero, Vector2 posicao)
    {
        // Cria a página na cena
        GameObject novaPagina = Instantiate(paginaPrefab, posicao, Quaternion.identity);

        // Acessa o script da página e define o número dela
        DadosDaPagina dados = novaPagina.GetComponent<DadosDaPagina>();
        if (dados != null)
        {
            dados.numeroDaPagina = numero;
            novaPagina.name = "Pagina_" + numero; // Facilita a leitura na Hierarchy
        }
        return novaPagina;
    }
}
