using UnityEngine;

// A criatura que persegue o jogador.
// - Anda sempre em direção ao jogador (atravessa obstáculos, como no Slender).
// - Ao alcançar o jogador: tira a página do TOPO da pilha (Pop) e a joga
//   em um lugar aleatório do mapa. Se a pilha estiver vazia, o jogador perde.
// - Depois de alcançar, teleporta para longe e espera um pouco antes de voltar a caçar.
// - Pode ser atordoada pelo FLASH da lanterna (IAtordoavel).
// Se não houver um Kowa na cena, o Spawner cria um automaticamente.
public class Kowa : MonoBehaviour, IAtordoavel
{
    [Header("Movimento")]
    public float velocidade = 5.5f;
    public float aumentoPorPilar = 0.25f;   // fica mais rápido a cada página entregue
    public float distanciaParaPegar = 0.8f;

    [Header("Teleporte após pegar o jogador")]
    public float distanciaMinimaTeleporte = 22f;
    public float esperaAposTeleporte = 2.5f;

    private Transform jogador;
    private ColetorDePaginas coletor;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float paradoAte = 0f;   // atordoado ou esperando após teleporte
    private bool atordoado = false;

    // Cria um Kowa com um sprite simples gerado por código
    public static Kowa Criar()
    {
        GameObject go = new GameObject("Kowa");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GerarSprite();
        sr.sortingOrder = 50;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        return go.AddComponent<Kowa>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            jogador = pc.transform;
            coletor = pc.GetComponent<ColetorDePaginas>();
        }

        // Efeito de estática/chiado que cresce quando o Kowa se aproxima
        if (EfeitoTensao.Instancia == null) EfeitoTensao.Criar(transform);

        // Começa longe do jogador e dá um tempo antes de começar a caçar
        TeleportarParaLonge();
        paradoAte = Time.time + 4f;
    }

    void FixedUpdate()
    {
        if (jogador == null) return;

        if (Time.time < paradoAte) return;
        if (atordoado)
        {
            atordoado = false;
            if (sr != null) sr.color = Color.white;
        }

        float vel = velocidade + aumentoPorPilar * ContarPilaresConcluidos();
        Vector2 novaPos = Vector2.MoveTowards(rb.position, jogador.position, vel * Time.fixedDeltaTime);
        rb.MovePosition(novaPos);

        if (Vector2.Distance(novaPos, jogador.position) <= distanciaParaPegar)
        {
            PegarJogador();
        }
    }

    void PegarJogador()
    {
        PilhaPaginas pilha = coletor != null ? coletor.pilhaDePaginas : null;

        // Sem páginas na pilha: derrota
        if (pilha == null || pilha.EstaVazia())
        {
            Debug.Log("[DERROTA] O Kowa pegou o jogador sem páginas!");
            if (MenuPrincipal.Instancia != null)
                MenuPrincipal.Instancia.MostrarFimDeJogo(false, "O Kowa te pegou sem nenhuma página.");
            enabled = false;
            return;
        }

        if (EfeitoTensao.Instancia != null) EfeitoTensao.Instancia.Explosao(0.8f);

        // Pop: tira a página do topo e joga em um lugar aleatório do mapa
        int pagina = pilha.Pop();
        if (Spawner.Instancia != null)
        {
            Spawner.Instancia.CriarPagina(pagina, Spawner.Instancia.PosicaoAleatoria());
        }
        Debug.Log($"[KOWA] Pegou o jogador e jogou a página {pagina} longe! Pilha agora: {pilha}");

        TeleportarParaLonge();
        paradoAte = Time.time + esperaAposTeleporte;
    }

    // Procura um ponto do mapa bem longe do jogador
    void TeleportarParaLonge()
    {
        if (jogador == null || Spawner.Instancia == null) return;

        Vector2 melhor = transform.position;
        float melhorDist = -1f;
        for (int i = 0; i < 30; i++)
        {
            Vector2 p = Spawner.Instancia.PosicaoAleatoria();
            float d = Vector2.Distance(p, jogador.position);
            if (d >= distanciaMinimaTeleporte) { melhor = p; break; }
            if (d > melhorDist) { melhorDist = d; melhor = p; } // guarda o mais longe achado
        }

        transform.position = melhor;
        if (rb != null) rb.position = melhor;
    }

    int ContarPilaresConcluidos()
    {
        int n = 0;
        foreach (Pilar p in FindObjectsByType<Pilar>(FindObjectsSortMode.None))
        {
            if (p.pilarConcluido) n++;
        }
        return n;
    }

    // Chamado pelo FLASH da lanterna
    public void Atordoar(float duracao)
    {
        paradoAte = Mathf.Max(paradoAte, Time.time + duracao);
        atordoado = true;
        if (sr != null) sr.color = new Color(0.5f, 0.5f, 1f, 1f);
        Debug.Log($"[KOWA] Atordoado por {duracao}s!");
    }

    // Figura alta e escura, sem rosto (16x48 pixels, 1x3 unidades)
    static Sprite GerarSprite()
    {
        const int w = 16, h = 48;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color vazio = new Color(0, 0, 0, 0);
        Color corpo = new Color(0.05f, 0.05f, 0.07f, 1f);
        Color cabeca = new Color(0.92f, 0.92f, 0.92f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = vazio;
                bool pernas = y < 16 && (x == 5 || x == 6 || x == 9 || x == 10);
                bool tronco = y >= 16 && y < 36 && x >= 4 && x <= 11;
                bool bracos = y >= 14 && y < 34 && (x == 2 || x == 13);
                bool cabecaPx = y >= 37 && y < 46 && x >= 5 && x <= 10;
                if (pernas || tronco || bracos) c = corpo;
                if (cabecaPx) c = cabeca;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.15f), 16f);
    }
}
