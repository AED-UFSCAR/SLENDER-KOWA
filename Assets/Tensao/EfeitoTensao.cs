using UnityEngine;
using UnityEngine.UI;

// Estática na tela e chiado que ficam mais fortes conforme o Kowa se aproxima.
// Criado automaticamente pelo Kowa. O som é gerado por código (ruído branco),
// mas dá para colocar um AudioClip próprio no campo "somEstatica".
public class EfeitoTensao : MonoBehaviour
{
    public static EfeitoTensao Instancia { get; private set; }

    [Header("Distância")]
    [Tooltip("A partir desta distância o efeito começa")]
    public float distanciaMaxima = 12f;

    [Header("Força")]
    public float opacidadeMaxima = 0.22f;
    public float volumeMaximo = 0.4f;

    [Header("Som (opcional)")]
    public AudioClip somEstatica;

    private Transform kowa;
    private Transform jogador;
    private RawImage tela;
    private Texture2D ruido;
    private Color32[] pixels;
    private AudioSource audioFonte;
    private float intensidade;
    private float explosaoAte = -1f;
    private float proximoQuadro;

    private const int LARGURA = 192, ALTURA = 108;

    public static EfeitoTensao Criar(Transform kowa)
    {
        GameObject go = new GameObject("Efeito Tensao");
        EfeitoTensao e = go.AddComponent<EfeitoTensao>();
        e.kowa = kowa;
        return e;
    }

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) jogador = pc.transform;
        if (kowa == null)
        {
            Kowa k = FindFirstObjectByType<Kowa>();
            if (k != null) kowa = k.transform;
        }

        // Camada de estática por cima do jogo (abaixo dos HUDs)
        Canvas canvas = UIFabrica.CriarCanvas("Canvas Estatica", transform, 90, new Vector2(1600f, 900f));
        tela = UIFabrica.CriarRect("Estatica", canvas.transform).gameObject.AddComponent<RawImage>();
        UIFabrica.Preencher(tela.rectTransform);
        tela.raycastTarget = false;

        ruido = new Texture2D(LARGURA, ALTURA, TextureFormat.RGBA32, false);
        ruido.filterMode = FilterMode.Point;
        pixels = new Color32[LARGURA * ALTURA];
        tela.texture = ruido;
        tela.color = new Color(1f, 1f, 1f, 0f);

        // Chiado em loop, com volume controlado pela distância
        audioFonte = gameObject.AddComponent<AudioSource>();
        audioFonte.clip = somEstatica != null ? somEstatica : GerarRuidoBranco();
        audioFonte.loop = true;
        audioFonte.playOnAwake = false;
        audioFonte.spatialBlend = 0f;
        audioFonte.volume = 0f;
        audioFonte.Play();
    }

    // Pausa o som junto com o jogo (o menu desliga este script)
    void OnDisable()
    {
        if (audioFonte != null) audioFonte.Pause();
    }

    void OnEnable()
    {
        if (audioFonte != null) audioFonte.UnPause();
    }

    // Estática no máximo por um instante (quando o Kowa pega o jogador)
    public void Explosao(float duracao)
    {
        explosaoAte = Time.time + duracao;
    }

    void Update()
    {
        float alvo = 0f;
        if (kowa != null && jogador != null)
        {
            float dist = Vector2.Distance(kowa.position, jogador.position);
            alvo = Mathf.Clamp01(1f - dist / distanciaMaxima);
            alvo = alvo * alvo; // cresce mais rápido quando está bem perto
        }
        if (Time.time < explosaoAte) alvo = 1f;

        intensidade = Mathf.MoveTowards(intensidade, alvo, Time.deltaTime * 2.5f);

        tela.color = new Color(1f, 1f, 1f, intensidade * opacidadeMaxima);
        audioFonte.volume = intensidade * volumeMaximo;

        // Gera um novo quadro de ruído ~20 vezes por segundo (só se estiver visível)
        if (intensidade > 0.01f && Time.time >= proximoQuadro)
        {
            proximoQuadro = Time.time + 0.05f;
            GerarQuadro();
        }
    }

    void GerarQuadro()
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            byte v = (byte)Random.Range(0, 256);
            byte a = (byte)Random.Range(40, 200);
            pixels[i] = new Color32(v, v, v, a);
        }

        // Algumas faixas horizontais escuras, como TV com interferência
        int faixas = Random.Range(0, 3);
        for (int f = 0; f < faixas; f++)
        {
            int y = Random.Range(0, ALTURA);
            for (int x = 0; x < LARGURA; x++) pixels[y * LARGURA + x] = new Color32(0, 0, 0, 160);
        }

        ruido.SetPixels32(pixels);
        ruido.Apply(false);
    }

    // 1 segundo de ruído branco, tocado em loop
    static AudioClip GerarRuidoBranco()
    {
        const int taxa = 44100;
        float[] dados = new float[taxa];
        System.Random rnd = new System.Random();
        for (int i = 0; i < dados.Length; i++)
        {
            dados[i] = (float)(rnd.NextDouble() * 2.0 - 1.0) * 0.6f;
        }
        AudioClip clip = AudioClip.Create("Estatica", taxa, 1, taxa, false);
        clip.SetData(dados, 0);
        return clip;
    }
}
