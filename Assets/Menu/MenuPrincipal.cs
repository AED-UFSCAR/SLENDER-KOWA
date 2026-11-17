using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu inicial com a tela da árvore de habilidades.
// Não precisa ser colocado na cena: ele se cria sozinho ao dar Play.
// Enquanto o menu está aberto, todos os scripts do jogo ficam desligados
// (nada se move, nenhuma página nasce); tudo só começa ao clicar em "Jogar".
// A árvore só pode ser alterada aqui, antes da partida.
public class MenuPrincipal : MonoBehaviour
{
    public static bool MenuAberto { get; private set; }
    public static MenuPrincipal Instancia { get; private set; }

    private GameObject painelPrincipal;
    private GameObject painelArvore;
    private GameObject painelFim;
    private TextoUI tituloFim;
    private TextoUI mensagemFim;
    private bool fimMostrado = false;
    private TextoUI textoPontos;
    private TextoUI textoStatus;
    private TextoUI textoDetalhe;
    private readonly Dictionary<NoHabilidade, Image> fundoDosNos = new Dictionary<NoHabilidade, Image>();
    private readonly Dictionary<NoHabilidade, TextoUI> textoDosNos = new Dictionary<NoHabilidade, TextoUI>();
    // Para mostrar as operações da árvore na apresentação
    private readonly Dictionary<NoHabilidade, Image> molduraDosNos = new Dictionary<NoHabilidade, Image>();
    private readonly Dictionary<NoHabilidade, TextoUI> numeroDosNos = new Dictionary<NoHabilidade, TextoUI>();
    private readonly Dictionary<string, Image> linhas = new Dictionary<string, Image>();
    private TextoUI textoCaminho;
    private Coroutine animacaoPreOrdem;
    private static readonly Color corLinha = new Color(1f, 1f, 1f, 0.22f);
    private static readonly Color corCaminho = new Color(1f, 0.85f, 0.25f, 1f);
    private static readonly Color corPercurso = new Color(0.3f, 0.85f, 1f, 1f);
    private static readonly Color corPercursoVisitado = new Color(0.3f, 0.85f, 1f, 0.35f);
    private readonly List<MonoBehaviour> scriptsPausados = new List<MonoBehaviour>();

    private static readonly Color corFundo = new Color(0.03f, 0.03f, 0.05f, 1f);
    private static readonly Color corBotao = new Color(0.16f, 0.16f, 0.2f, 1f);
    private static readonly Color corDesbloqueado = new Color(0.22f, 0.55f, 0.28f, 1f);
    private static readonly Color corDisponivel = new Color(0.72f, 0.56f, 0.12f, 1f);
    private static readonly Color corBloqueado = new Color(0.17f, 0.17f, 0.2f, 1f);
    private static readonly Color corEspecialBloqueado = new Color(0.3f, 0.12f, 0.12f, 1f);

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
        if (FindFirstObjectByType<MenuPrincipal>() != null) return;
        if (FindFirstObjectByType<PlayerController>() == null) return;

        new GameObject("Menu Principal").AddComponent<MenuPrincipal>();
    }

    private void Awake()
    {
        Instancia = this;
        ArvoreHabilidades.Inicializar();
        GarantirEventSystem();
        MontarInterface();
        AbrirMenu();
    }

    // ---------- Pausar / liberar o jogo ----------

    // Desliga todos os scripts do projeto que estão na cena (jogador, lanterna,
    // spawner, pilares, câmera, HUD...). Scripts desligados não rodam Start/Update.
    void PausarJogo()
    {
        Time.timeScale = 0f; // também congela a física
        System.Reflection.Assembly nossoCodigo = typeof(MenuPrincipal).Assembly;

        foreach (MonoBehaviour mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb == this || !mb.enabled) continue;
            if (mb.GetType().Assembly != nossoCodigo) continue;

            mb.enabled = false;
            scriptsPausados.Add(mb);
        }
    }

    void LiberarJogo()
    {
        foreach (MonoBehaviour mb in scriptsPausados)
        {
            if (mb != null) mb.enabled = true;
        }
        scriptsPausados.Clear();
        Time.timeScale = 1f;
    }

    // ---------- Fim de jogo (vitória / derrota) ----------

    public void MostrarFimDeJogo(bool vitoria, string mensagem)
    {
        if (fimMostrado) return;
        fimMostrado = true;

        MenuAberto = true; // esconde o HUD
        PausarJogo();

        tituloFim.Texto = vitoria ? "VOCÊ VENCEU!" : "VOCÊ FOI PEGO";
        tituloFim.Cor = vitoria ? new Color(0.5f, 1f, 0.6f, 1f) : new Color(1f, 0.35f, 0.35f, 1f);
        mensagemFim.Texto = mensagem + $"\n\n<size=24>Pontos disponíveis na árvore: {ArvoreHabilidades.PontosDisponiveis}</size>";

        painelPrincipal.SetActive(false);
        painelArvore.SetActive(false);
        painelFim.SetActive(true);
    }

    // Recarrega a cena: tudo volta ao início e o menu principal aparece de novo.
    // (A árvore e os pontos ficam salvos.)
    void VoltarAoMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------- Ações dos botões ----------

    void AbrirMenu()
    {
        MenuAberto = true;
        PausarJogo();
        painelPrincipal.SetActive(true);
        painelArvore.SetActive(false);
        painelFim.SetActive(false);
    }

    void Jogar()
    {
        MenuAberto = false;
        painelPrincipal.SetActive(false);
        painelArvore.SetActive(false);
        LiberarJogo();
    }

    void AbrirArvore()
    {
        painelPrincipal.SetActive(false);
        painelArvore.SetActive(true);
        AtualizarArvore();
    }

    void VoltarAoMenu()
    {
        PararPreOrdem();
        LimparDestaques();
        painelArvore.SetActive(false);
        painelPrincipal.SetActive(true);
    }

    void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ClicarNo(NoHabilidade no)
    {
        PararPreOrdem();
        ArvoreHabilidades.Desbloquear(no);
        AtualizarArvore();
        MostrarDetalhe(no);
        DestacarCaminho(no);
    }

    // ---------- Visualização das operações da árvore ----------

    // Sobe do nó até a raiz seguindo os pais e devolve o caminho raiz -> nó.
    // Num nó com dois caminhos, usa o pai alternativo se só ele estiver ativo.
    List<NoHabilidade> CaminhoDaRaizAte(NoHabilidade no)
    {
        List<NoHabilidade> caminho = new List<NoHabilidade>();
        NoHabilidade atual = no;
        while (atual != null)
        {
            caminho.Add(atual);
            bool usarAlternativo = atual.paiAlternativo != null && atual.paiAlternativo.desbloqueado
                                   && (atual.pai == null || !atual.pai.desbloqueado);
            atual = usarAlternativo ? atual.paiAlternativo : atual.pai;
        }
        caminho.Reverse();
        return caminho;
    }

    void LimparDestaques()
    {
        foreach (Image moldura in molduraDosNos.Values) moldura.color = Color.clear;
        foreach (TextoUI numero in numeroDosNos.Values) numero.Texto = "";
        foreach (Image linha in linhas.Values)
        {
            linha.color = corLinha;
            linha.rectTransform.sizeDelta = new Vector2(linha.rectTransform.sizeDelta.x, 4f);
        }
    }

    void DestacarLinha(NoHabilidade de, NoHabilidade para, Color cor)
    {
        if (linhas.TryGetValue(ChaveLinha(de, para), out Image linha))
        {
            linha.color = cor;
            linha.rectTransform.sizeDelta = new Vector2(linha.rectTransform.sizeDelta.x, 8f);
        }
    }

    void DestacarCaminho(NoHabilidade no)
    {
        LimparDestaques();
        List<NoHabilidade> caminho = CaminhoDaRaizAte(no);
        List<string> nomes = new List<string>();

        for (int i = 0; i < caminho.Count; i++)
        {
            molduraDosNos[caminho[i]].color = corCaminho;
            nomes.Add(caminho[i].nome);
            if (i > 0) DestacarLinha(caminho[i - 1], caminho[i], corCaminho);
        }

        textoCaminho.Cor = corCaminho;
        textoCaminho.Texto = $"Caminho da raiz (profundidade {caminho.Count - 1}):  " + string.Join("  >  ", nomes);
    }

    void IniciarPreOrdem()
    {
        PararPreOrdem();
        animacaoPreOrdem = StartCoroutine(AnimarPreOrdem());
    }

    void PararPreOrdem()
    {
        if (animacaoPreOrdem != null) StopCoroutine(animacaoPreOrdem);
        animacaoPreOrdem = null;
    }

    // Acende os nós na ordem em que o percurso em pré-ordem os visita:
    // primeiro o nó, depois cada filho (da esquerda para a direita).
    // Usa tempo "real" porque o jogo está pausado (timeScale = 0) no menu.
    IEnumerator AnimarPreOrdem()
    {
        LimparDestaques();
        List<NoHabilidade> ordem = ArvoreHabilidades.ListarPreOrdem();

        for (int i = 0; i < ordem.Count; i++)
        {
            NoHabilidade no = ordem[i];
            if (i > 0) molduraDosNos[ordem[i - 1]].color = corPercursoVisitado;
            molduraDosNos[no].color = corPercurso;
            numeroDosNos[no].Texto = $"<b>{i + 1}</b>";
            if (no.pai != null) DestacarLinha(no.pai, no, corPercurso);

            textoCaminho.Cor = corPercurso;
            textoCaminho.Texto = $"Pré-ordem (nó, depois filhos da esquerda para a direita):  passo {i + 1}/{ordem.Count}  =  {no.nome}";
            yield return new WaitForSecondsRealtime(0.45f);
        }

        molduraDosNos[ordem[ordem.Count - 1]].color = corPercursoVisitado;
        textoCaminho.Texto = $"Pré-ordem concluída: {ordem.Count} nós visitados. Os números mostram a ordem da visita.";
        animacaoPreOrdem = null;
    }

    void ResetarArvore()
    {
        PararPreOrdem();
        LimparDestaques();
        ArvoreHabilidades.Resetar();
        AtualizarArvore();
    }

    void MostrarDetalhe(NoHabilidade no)
    {
        string req = no.paiAlternativo != null
            ? $"Requer: {no.pai.nome} OU {no.paiAlternativo.nome}"
            : (no.pai != null ? $"Requer: {no.pai.nome}" : "");
        textoDetalhe.Texto = $"<b>{no.nome}</b>  ({no.custo} pt)   {no.descricao.Replace("\n", "  ·  ")}   {req}";
    }

    // ---------- Atualização visual da árvore ----------

    void AtualizarArvore()
    {
        foreach (KeyValuePair<NoHabilidade, Image> par in fundoDosNos)
        {
            NoHabilidade no = par.Key;
            string situacao;

            if (no.desbloqueado)
            {
                par.Value.color = corDesbloqueado;
                situacao = "ATIVA";
            }
            else if (ArvoreHabilidades.PodeDesbloquear(no))
            {
                par.Value.color = corDisponivel;
                situacao = $"Comprar ({no.custo} pt)";
            }
            else
            {
                par.Value.color = no.EhEspecial() ? corEspecialBloqueado : corBloqueado;
                situacao = no.PreRequisitoAtendido() ? $"Faltam pontos ({no.custo} pt)" : "Bloqueada";
            }

            string nome = no.EhEspecial() ? $"<color=#FFD866>{no.nome}</color>" : no.nome;
            textoDosNos[no].Texto = $"<b>{nome}</b>\n<size=14>{no.descricao}</size>\n<size=13><i>{situacao}</i></size>";
        }

        StatusJogador s = ArvoreHabilidades.Status;
        textoPontos.Texto = $"Pontos disponíveis: {ArvoreHabilidades.PontosDisponiveis}   " +
                            $"<size=18>(ganhe 1 ponto por página entregue em um pilar)</size>";
        textoStatus.Texto =
            $"Velocidade {s.multVelocidade * 100f:0}%   ·   " +
            $"Brilho {s.multIntensidadeLanterna * 100f:0}%   ·   " +
            $"Alcance {s.multRaioLanterna * 100f:0}%   ·   " +
            $"Abertura {s.multAnguloLanterna * 100f:0}%   ·   " +
            $"Bateria {s.multDuracaoBateria * 100f:0}%";
    }

    // ---------- Montagem da interface ----------

    void GarantirEventSystem()
    {
        // Sem EventSystem os botões não recebem clique
        if (FindFirstObjectByType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    void MontarInterface()
    {
        Canvas canvas = UIFabrica.CriarCanvas("Canvas Menu", transform, 200, new Vector2(1600f, 900f));
        TextoUI t;

        // ----- Painel principal -----
        painelPrincipal = CriarPainelTelaCheia("Painel Principal", canvas.transform);

        t = UIFabrica.CriarTexto("Titulo", painelPrincipal.transform, "SLENDER KOWA", 96);
        Font fonteTitulo = UIFabrica.FonteTitulo();
        if (fonteTitulo != null) t.DefinirFonte(fonteTitulo); else t.Negrito = true;
        t.Cor = new Color(0.92f, 0.9f, 0.88f, 1f);
        UIFabrica.Posicionar(t.rectTransform, new Vector2(0f, 250f), new Vector2(1200f, 120f));

        t = UIFabrica.CriarTexto("Subtitulo", painelPrincipal.transform,
            "Colete as 8 páginas e entregue cada uma no seu pilar", 26);
        t.Cor = new Color(1f, 1f, 1f, 0.6f);
        UIFabrica.Posicionar(t.rectTransform, new Vector2(0f, 170f), new Vector2(1200f, 50f));

        UIFabrica.CriarBotao("Jogar", painelPrincipal.transform, new Vector2(0f, 40f), new Vector2(400f, 72f), Jogar, corBotao, 30, out _);
        UIFabrica.CriarBotao("Árvore de Habilidades", painelPrincipal.transform, new Vector2(0f, -50f), new Vector2(400f, 72f), AbrirArvore, corBotao, 30, out _);
        UIFabrica.CriarBotao("Sair", painelPrincipal.transform, new Vector2(0f, -140f), new Vector2(400f, 72f), Sair, corBotao, 30, out _);

        t = UIFabrica.CriarTexto("Dica", painelPrincipal.transform,
            "WASD: mover   ·   Mouse: lanterna (botão direito liga/desliga)   ·   E: entregar página   ·   Shift / Q / F: habilidades", 20);
        t.Cor = new Color(1f, 1f, 1f, 0.5f);
        UIFabrica.Posicionar(t.rectTransform, new Vector2(0f, -360f), new Vector2(1400f, 40f));

        // ----- Painel da árvore -----
        painelArvore = CriarPainelTelaCheia("Painel Arvore", canvas.transform);

        t = UIFabrica.CriarTexto("Titulo", painelArvore.transform, "ÁRVORE DE HABILIDADES", 46);
        if (fonteTitulo != null) t.DefinirFonte(fonteTitulo); else t.Negrito = true;
        UIFabrica.Posicionar(t.rectTransform, new Vector2(0f, 405f), new Vector2(1200f, 60f));

        textoPontos = UIFabrica.CriarTexto("Pontos", painelArvore.transform, "", 26);
        textoPontos.Cor = new Color(1f, 0.8f, 0.3f, 1f);
        UIFabrica.Posicionar(textoPontos.rectTransform, new Vector2(0f, 358f), new Vector2(1400f, 36f));

        RectTransform areaNos = UIFabrica.CriarRect("Nos", painelArvore.transform);
        UIFabrica.Posicionar(areaNos, new Vector2(0f, 30f), new Vector2(1500f, 600f));
        DesenharArvore(areaNos);

        textoDetalhe = UIFabrica.CriarTexto("Detalhe", painelArvore.transform, "Clique em uma habilidade para comprá-la.", 19);
        textoDetalhe.Cor = new Color(1f, 1f, 1f, 0.75f);
        UIFabrica.Posicionar(textoDetalhe.rectTransform, new Vector2(0f, -294f), new Vector2(1500f, 28f));

        textoCaminho = UIFabrica.CriarTexto("Caminho", painelArvore.transform, "", 19);
        UIFabrica.Posicionar(textoCaminho.rectTransform, new Vector2(0f, -324f), new Vector2(1500f, 28f));

        textoStatus = UIFabrica.CriarTexto("Status", painelArvore.transform, "", 21);
        textoStatus.Cor = new Color(0.7f, 0.95f, 0.75f, 1f);
        UIFabrica.Posicionar(textoStatus.rectTransform, new Vector2(0f, -356f), new Vector2(1500f, 30f));

        UIFabrica.CriarBotao("Resetar pontos", painelArvore.transform, new Vector2(-300f, -412f), new Vector2(270f, 52f), ResetarArvore, corBotao, 22, out _);
        UIFabrica.CriarBotao("Mostrar pré-ordem", painelArvore.transform, new Vector2(0f, -412f), new Vector2(270f, 52f), IniciarPreOrdem, corBotao, 22, out _);
        UIFabrica.CriarBotao("Voltar", painelArvore.transform, new Vector2(300f, -412f), new Vector2(270f, 52f), VoltarAoMenu, corBotao, 22, out _);

        // ----- Painel de fim de jogo -----
        painelFim = CriarPainelTelaCheia("Painel Fim", canvas.transform);
        painelFim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);

        tituloFim = UIFabrica.CriarTexto("Titulo", painelFim.transform, "", 80);
        if (fonteTitulo != null) tituloFim.DefinirFonte(fonteTitulo); else tituloFim.Negrito = true;
        UIFabrica.Posicionar(tituloFim.rectTransform, new Vector2(0f, 150f), new Vector2(1200f, 120f));

        mensagemFim = UIFabrica.CriarTexto("Mensagem", painelFim.transform, "", 30);
        UIFabrica.Posicionar(mensagemFim.rectTransform, new Vector2(0f, 10f), new Vector2(1200f, 140f));

        UIFabrica.CriarBotao("Menu principal", painelFim.transform, new Vector2(0f, -150f), new Vector2(360f, 70f), VoltarAoMenuPrincipal, corBotao, 28, out _);
    }

    // Distribui os nós: cada folha ganha uma coluna, cada pai fica centralizado
    // sobre os filhos, e a altura vem da profundidade. Nós com dois caminhos
    // ficam no meio dos dois pais.
    void DesenharArvore(RectTransform area)
    {
        const float larguraColuna = 238f;
        const float alturaNivel = 128f;
        Vector2 tamanhoNo = new Vector2(212f, 92f);

        Dictionary<NoHabilidade, Vector2> pos = new Dictionary<NoHabilidade, Vector2>();
        int colunas = 0;
        CalcularPosicoes(ArvoreHabilidades.Raiz, 0, ref colunas, pos, larguraColuna, alturaNivel);

        List<NoHabilidade> nos = ArvoreHabilidades.ListarPreOrdem();
        foreach (NoHabilidade no in nos)
        {
            if (no.paiAlternativo != null)
                pos[no] = new Vector2((pos[no.pai].x + pos[no.paiAlternativo].x) / 2f, pos[no].y);
        }

        // Centraliza na área (a raiz fica no topo)
        float deslocX = (colunas - 1) * larguraColuna / 2f;
        float deslocY = 2f * alturaNivel;
        foreach (NoHabilidade no in nos) pos[no] = pos[no] + new Vector2(-deslocX, deslocY);

        // Linhas primeiro, para ficarem atrás dos nós
        foreach (NoHabilidade no in nos)
        {
            foreach (NoHabilidade filho in no.filhos) linhas[ChaveLinha(no, filho)] = CriarLinha(area, pos[no], pos[filho]);
            if (no.paiAlternativo != null) linhas[ChaveLinha(no.paiAlternativo, no)] = CriarLinha(area, pos[no.paiAlternativo], pos[no]);
        }

        // Molduras atrás dos nós (acendem para mostrar caminho / percurso)
        foreach (NoHabilidade no in nos)
        {
            Image moldura = UIFabrica.CriarImagem("Moldura " + no.id, area, Color.clear);
            UIFabrica.Posicionar(moldura.rectTransform, pos[no], tamanhoNo + new Vector2(14f, 14f));
            moldura.raycastTarget = false;
            molduraDosNos[no] = moldura;
        }

        foreach (NoHabilidade no in nos)
        {
            NoHabilidade noAtual = no; // cópia para o clique
            Button botao = UIFabrica.CriarBotao(no.nome, area, pos[no], tamanhoNo, () => ClicarNo(noAtual),
                                                corBloqueado, 19, out TextoUI texto);
            fundoDosNos[no] = botao.GetComponent<Image>();
            textoDosNos[no] = texto;
        }

        // Número da ordem de visita (aparece no percurso em pré-ordem)
        foreach (NoHabilidade no in nos)
        {
            TextoUI numero = UIFabrica.CriarTexto("Numero " + no.id, area, "", 22);
            numero.Cor = corPercurso;
            UIFabrica.Posicionar(numero.rectTransform, pos[no] + new Vector2(-tamanhoNo.x / 2f - 4f, tamanhoNo.y / 2f + 4f), new Vector2(40f, 30f));
            numeroDosNos[no] = numero;
        }
    }

    // Percurso pós-ordem: posiciona os filhos antes do pai
    void CalcularPosicoes(NoHabilidade no, int profundidade, ref int proximaColuna,
                          Dictionary<NoHabilidade, Vector2> pos, float larguraColuna, float alturaNivel)
    {
        float y = -profundidade * alturaNivel;

        if (no.EhFolha())
        {
            pos[no] = new Vector2(proximaColuna * larguraColuna, y);
            proximaColuna++;
            return;
        }

        float somaX = 0f;
        foreach (NoHabilidade filho in no.filhos)
        {
            CalcularPosicoes(filho, profundidade + 1, ref proximaColuna, pos, larguraColuna, alturaNivel);
            somaX += pos[filho].x;
        }
        pos[no] = new Vector2(somaX / no.filhos.Count, y);
    }

    static string ChaveLinha(NoHabilidade de, NoHabilidade para)
    {
        return de.id + ">" + para.id;
    }

    Image CriarLinha(Transform pai, Vector2 de, Vector2 ate)
    {
        Image img = UIFabrica.CriarImagem("Linha", pai, corLinha);
        RectTransform rt = img.rectTransform;
        Vector2 dir = ate - de;
        UIFabrica.Posicionar(rt, (de + ate) / 2f, new Vector2(dir.magnitude, 4f));
        rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        img.raycastTarget = false;
        return img;
    }

    GameObject CriarPainelTelaCheia(string nome, Transform pai)
    {
        Image img = UIFabrica.CriarImagem(nome, pai, corFundo); // opaco: esconde o jogo e bloqueia cliques
        UIFabrica.Preencher(img.rectTransform);
        return img.gameObject;
    }
}
