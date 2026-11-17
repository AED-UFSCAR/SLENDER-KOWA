using UnityEngine;
using UnityEngine.UI;

// HUD que mostra a pilha de páginas na tela.
// Não precisa ser colocado na cena: ele se cria sozinho ao dar Play
// (se existir um ColetorDePaginas) e monta o próprio Canvas.
public class HudPilha : MonoBehaviour
{
    [Header("Aparência")]
    public int capacidade = 8;
    public Vector2 tamanhoSlot = new Vector2(140f, 26f);
    public float espacamento = 4f;
    public Color corSlotVazio = new Color(1f, 1f, 1f, 0.08f);
    public Color corSlotCheio = new Color(0.85f, 0.82f, 0.70f, 0.85f);
    public Color corTopo = new Color(1f, 0.85f, 0.25f, 1f);

    private ColetorDePaginas coletor;
    private GameObject canvasGO;
    private Image[] fundos;
    private TextoUI[] textos;
    private TextoUI titulo;
    private TextoUI rodape;
    private RectTransform painel;
    private TextoUI textoOperacao;

    // Animações de Push (slot entrando) e Pop (página saindo)
    private const float DURACAO_PUSH = 0.35f;
    private const float DURACAO_POP = 0.6f;
    private const float DURACAO_OPERACAO = 2f;
    private int slotAnimado = -1;
    private float inicioPush;
    private float inicioOperacao = -10f;
    private readonly System.Collections.Generic.List<Fantasma> fantasmas = new System.Collections.Generic.List<Fantasma>();

    class Fantasma
    {
        public RectTransform rt;
        public Image img;
        public TextoUI texto;
        public float inicio;
        public Vector2 posInicial;
    }

    // Guarda o último estado desenhado para só redesenhar quando a pilha muda
    private int ultimoCount = -1;
    private int ultimoTopo = int.MinValue;

    // Registra a criação automática sempre que uma cena carrega
    // (inclusive quando a cena é recarregada ao voltar para o menu)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Registrar()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= AoCarregarCena;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += AoCarregarCena;
    }

    private static void AoCarregarCena(UnityEngine.SceneManagement.Scene cena, UnityEngine.SceneManagement.LoadSceneMode modo)
    {
        if (FindFirstObjectByType<HudPilha>() != null) return;
        if (FindFirstObjectByType<ColetorDePaginas>() == null) return;

        new GameObject("HUD Pilha").AddComponent<HudPilha>();
    }

    private void Start()
    {
        coletor = FindFirstObjectByType<ColetorDePaginas>();
        MontarInterface();
        Atualizar(true);
    }

    private void Update()
    {
        // Fica escondido enquanto o menu estiver aberto
        if (canvasGO.activeSelf == MenuPrincipal.MenuAberto) canvasGO.SetActive(!MenuPrincipal.MenuAberto);
        Atualizar(false);
        Animar();
    }

    void Atualizar(bool forcar)
    {
        if (coletor == null || coletor.pilhaDePaginas == null) return;

        PilhaPaginas pilha = coletor.pilhaDePaginas;
        if (!forcar && pilha.Count == ultimoCount && pilha.Peek() == ultimoTopo) return;

        // Se havia uma animação de Push em andamento, termina ela antes
        if (slotAnimado >= 0)
        {
            fundos[slotAnimado].rectTransform.anchoredPosition = new Vector2(0f, PosicaoYDoSlot(slotAnimado));
            slotAnimado = -1;
        }

        // Descobre qual operação aconteceu, comparando com o último estado
        if (!forcar && ultimoCount >= 0)
        {
            if (pilha.Count > ultimoCount)
            {
                slotAnimado = pilha.Count - 1;           // o novo topo entra animado
                inicioPush = Time.time;
                MostrarOperacao($"PUSH({pilha.Peek()})", new Color(0.45f, 1f, 0.5f, 1f));
            }
            else if (pilha.Count < ultimoCount)
            {
                CriarFantasma(ultimoCount - 1, ultimoTopo); // o topo antigo sai animado
                MostrarOperacao($"POP({ultimoTopo})", new Color(1f, 0.45f, 0.45f, 1f));
            }
        }

        ultimoCount = pilha.Count;
        ultimoTopo = pilha.Peek();

        int[] itens = pilha.ToArray(); // índice 0 = topo

        // Os slots são desenhados de baixo para cima: slot 0 = base da pilha.
        for (int slot = 0; slot < capacidade; slot++)
        {
            int indiceNoArray = itens.Length - 1 - slot; // base fica no fim do array
            bool ocupado = indiceNoArray >= 0;
            bool ehTopo = ocupado && indiceNoArray == 0;

            if (ocupado)
            {
                fundos[slot].color = ehTopo ? corTopo : corSlotCheio;
                textos[slot].Texto = ehTopo ? $"<b>Página {itens[indiceNoArray]}  (TOPO)</b>" : $"Página {itens[indiceNoArray]}";
                textos[slot].Cor = Color.black;
            }
            else
            {
                fundos[slot].color = corSlotVazio;
                textos[slot].Texto = "";
            }
        }

        titulo.Texto = $"<b>PILHA  ({pilha.Count}/{capacidade})</b>";
        rodape.Texto = pilha.EstaVazia()
            ? "Colete páginas!"
            : $"Entregue a página {pilha.Peek()} no Pilar {pilha.Peek()} [E]";
    }

    // ---------- Animações ----------

    float PosicaoYDoSlot(int i)
    {
        return 38f + i * (tamanhoSlot.y + espacamento);
    }

    void MostrarOperacao(string texto, Color cor)
    {
        textoOperacao.Texto = $"<b>{texto}</b>";
        textoOperacao.Cor = cor;
        inicioOperacao = Time.time;
    }

    // Cria uma cópia do slot que sai deslizando para a direita e some
    void CriarFantasma(int slot, int pagina)
    {
        if (slot < 0 || slot >= capacidade) return;

        Fantasma f = new Fantasma();
        f.img = UIFabrica.CriarImagem("Pop", painel, corTopo);
        f.rt = f.img.rectTransform;
        f.rt.anchorMin = f.rt.anchorMax = f.rt.pivot = new Vector2(0.5f, 0f);
        f.rt.sizeDelta = tamanhoSlot;
        f.posInicial = new Vector2(0f, PosicaoYDoSlot(slot));
        f.rt.anchoredPosition = f.posInicial;
        f.texto = UIFabrica.CriarTexto("Texto", f.rt, $"<b>Página {pagina}</b>", 13);
        f.texto.Cor = Color.black;
        UIFabrica.Preencher(f.texto.rectTransform);
        f.inicio = Time.time;
        fantasmas.Add(f);
    }

    void Animar()
    {
        // Push: o novo topo desce de cima e pisca em branco
        if (slotAnimado >= 0)
        {
            float t = Mathf.Clamp01((Time.time - inicioPush) / DURACAO_PUSH);
            RectTransform rt = fundos[slotAnimado].rectTransform;
            rt.anchoredPosition = new Vector2(0f, PosicaoYDoSlot(slotAnimado) + (1f - t) * 40f);
            fundos[slotAnimado].color = Color.Lerp(Color.white, corTopo, t);
            if (t >= 1f) slotAnimado = -1;
        }

        // Pop: a página sai para a direita e desaparece
        for (int i = fantasmas.Count - 1; i >= 0; i--)
        {
            Fantasma f = fantasmas[i];
            float t = Mathf.Clamp01((Time.time - f.inicio) / DURACAO_POP);
            f.rt.anchoredPosition = f.posInicial + new Vector2(t * 140f, t * 20f);
            Color c = corTopo; c.a = 1f - t;
            f.img.color = c;
            Color ct = Color.black; ct.a = 1f - t;
            f.texto.Cor = ct;
            if (t >= 1f)
            {
                Destroy(f.rt.gameObject);
                fantasmas.RemoveAt(i);
            }
        }

        // Rótulo da última operação some aos poucos
        float to = (Time.time - inicioOperacao) / DURACAO_OPERACAO;
        Color co = textoOperacao.Cor;
        co.a = Mathf.Clamp01(1.5f - to * 1.5f);
        textoOperacao.Cor = co;
    }

    void MontarInterface()
    {
        Canvas canvas = UIFabrica.CriarCanvas("Canvas HUD Pilha", transform, 100, new Vector2(1600f, 900f));
        canvasGO = canvas.gameObject;

        // Painel no canto inferior esquerdo
        float alturaSlots = capacidade * tamanhoSlot.y + (capacidade - 1) * espacamento;
        Image fundoPainel = UIFabrica.CriarImagem("Painel", canvas.transform, new Color(0f, 0f, 0f, 0.55f));
        painel = fundoPainel.rectTransform;
        painel.anchorMin = painel.anchorMax = painel.pivot = Vector2.zero;
        painel.anchoredPosition = new Vector2(16f, 16f);
        painel.sizeDelta = new Vector2(tamanhoSlot.x + 16f, alturaSlots + 70f);

        // Rótulo da última operação (PUSH / POP), logo acima do painel
        textoOperacao = UIFabrica.CriarTexto("Operacao", painel, "", 18);
        RectTransform ro = textoOperacao.rectTransform;
        ro.anchorMin = new Vector2(0f, 1f); ro.anchorMax = new Vector2(1f, 1f); ro.pivot = new Vector2(0.5f, 0f);
        ro.anchoredPosition = new Vector2(0f, 4f);
        ro.sizeDelta = new Vector2(0f, 26f);
        Color semCor = Color.white; semCor.a = 0f;
        textoOperacao.Cor = semCor;

        titulo = UIFabrica.CriarTexto("Titulo", painel, "", 16);
        RectTransform rt = titulo.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -4f);
        rt.sizeDelta = new Vector2(0f, 24f);

        rodape = UIFabrica.CriarTexto("Rodape", painel, "", 11);
        rodape.Cor = new Color(1f, 1f, 1f, 0.8f);
        rt = rodape.rectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = new Vector2(1f, 0f); rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 6f);
        rt.sizeDelta = new Vector2(-8f, 30f);

        // Slots (do slot 0 = base, embaixo, até o último = em cima)
        fundos = new Image[capacidade];
        textos = new TextoUI[capacidade];
        for (int i = 0; i < capacidade; i++)
        {
            fundos[i] = UIFabrica.CriarImagem("Slot " + i, painel, corSlotVazio);
            RectTransform slot = fundos[i].rectTransform;
            slot.anchorMin = slot.anchorMax = slot.pivot = new Vector2(0.5f, 0f);
            slot.sizeDelta = tamanhoSlot;
            slot.anchoredPosition = new Vector2(0f, PosicaoYDoSlot(i));

            textos[i] = UIFabrica.CriarTexto("Texto", slot, "", 13);
            UIFabrica.Preencher(textos[i].rectTransform);
        }
    }
}
