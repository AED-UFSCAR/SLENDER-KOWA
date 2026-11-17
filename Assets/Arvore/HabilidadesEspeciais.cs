using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Controla as habilidades especiais liberadas no fim de cada trilha da árvore,
// cada uma com seu tempo de recarga (cooldown), e mostra a recarga na tela.
// É adicionado automaticamente ao Player pelo PlayerController.
public class HabilidadesEspeciais : MonoBehaviour
{
    [Header("Dash  [Shift]")]
    public float dashRecarga = 4f;
    public float dashDuracao = 0.18f;
    public float dashMultiplicador = 4f;

    [Header("Flash atordoante  [Q]")]
    public float flashRecarga = 10f;
    public float flashDuracao = 0.25f;
    public float tempoAtordoado = 2.5f;

    [Header("Lanterna 360°  [F]")]
    public float expandirRecarga = 15f;
    public float expandirDuracao = 3f;

    // Classe interna simples para cada habilidade
    class Habilidade
    {
        public string id, nome;
        public KeyCode tecla;
        public float recarga;
        public float prontaEm; // Time.time em que pode ser usada de novo
        public System.Func<bool> usar; // retorna false se não deu para usar (ex: sem bateria)
        public Image barra;
        public TextoUI texto;
    }

    private readonly List<Habilidade> habilidades = new List<Habilidade>();
    private PlayerController jogador;
    private ControleLanterna lanterna;
    private GameObject canvasGO;

    void Start()
    {
        jogador = GetComponent<PlayerController>();
        lanterna = GetComponentInChildren<ControleLanterna>();
        if (lanterna == null) lanterna = FindFirstObjectByType<ControleLanterna>();

        Registrar(ArvoreHabilidades.DASH, "Dash", KeyCode.LeftShift, dashRecarga, UsarDash);
        Registrar(ArvoreHabilidades.ATORDOAR, "Flash", KeyCode.Q, flashRecarga, UsarFlash);
        Registrar(ArvoreHabilidades.EXPANDIR, "Lanterna 360°", KeyCode.F, expandirRecarga, UsarExpandir);

        MontarHud();
    }

    // Só registra as habilidades que o jogador desbloqueou na árvore
    void Registrar(string id, string nome, KeyCode tecla, float recarga, System.Func<bool> usar)
    {
        if (!ArvoreHabilidades.PossuiHabilidade(id)) return;
        habilidades.Add(new Habilidade { id = id, nome = nome, tecla = tecla, recarga = recarga, usar = usar, prontaEm = 0f });
    }

    void Update()
    {
        foreach (Habilidade h in habilidades)
        {
            if (Input.GetKeyDown(h.tecla) && Time.time >= h.prontaEm && h.usar())
            {
                h.prontaEm = Time.time + h.recarga;
            }
            AtualizarHud(h);
        }
    }

    // ---------- Efeitos ----------

    bool UsarDash()
    {
        if (jogador == null) return false;
        jogador.IniciarDash(dashMultiplicador, dashDuracao);
        return true;
    }

    bool UsarFlash()
    {
        // O flash gasta 10% da bateria e precisa da lanterna ligada
        if (lanterna == null || !lanterna.TentarConsumir(10f)) return false;
        lanterna.Flash(flashDuracao);

        // Atordoa tudo que estiver dentro do cone de luz
        Vector2 origem = lanterna.transform.position;
        Vector2 direcao = lanterna.DirecaoDaLuz();
        float meioAngulo = lanterna.AnguloAtual / 2f;

        foreach (Collider2D c in Physics2D.OverlapCircleAll(origem, lanterna.RaioAtual))
        {
            IAtordoavel alvo = c.GetComponentInParent<IAtordoavel>();
            if (alvo == null) continue;

            Vector2 paraAlvo = (Vector2)c.transform.position - origem;
            if (meioAngulo >= 180f || Vector2.Angle(direcao, paraAlvo) <= meioAngulo)
            {
                alvo.Atordoar(tempoAtordoado);
            }
        }
        return true;
    }

    bool UsarExpandir()
    {
        if (lanterna == null || !lanterna.TemLuz) return false;
        lanterna.Expandir(expandirDuracao); // enquanto ativa, gasta bateria em dobro
        return true;
    }

    // ---------- HUD de recarga (canto inferior direito) ----------

    void MontarHud()
    {
        if (habilidades.Count == 0) return;

        Canvas canvas = UIFabrica.CriarCanvas("Canvas Habilidades", transform, 100, new Vector2(1600f, 900f));
        canvasGO = canvas.gameObject;
        Vector2 tamanho = new Vector2(250f, 44f);

        for (int i = 0; i < habilidades.Count; i++)
        {
            Habilidade h = habilidades[i];

            Image fundo = UIFabrica.CriarImagem("Habilidade " + h.nome, canvas.transform, new Color(0f, 0f, 0f, 0.6f));
            RectTransform rt = fundo.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0f);
            rt.sizeDelta = tamanho;
            rt.anchoredPosition = new Vector2(-20f, 20f + i * (tamanho.y + 8f));

            // Barra que enche conforme a recarga termina
            h.barra = UIFabrica.CriarImagem("Barra", rt, new Color(1f, 0.85f, 0.3f, 0.35f));
            RectTransform brt = h.barra.rectTransform;
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = brt.offsetMax = Vector2.zero;

            h.texto = UIFabrica.CriarTexto("Texto", rt, "", 20, TextAnchor.MiddleLeft);
            UIFabrica.Preencher(h.texto.rectTransform, 10f);
        }
    }

    void AtualizarHud(Habilidade h)
    {
        if (h.barra == null) return;

        float falta = Mathf.Max(0f, h.prontaEm - Time.time);
        float progresso = 1f - falta / h.recarga;
        h.barra.rectTransform.anchorMax = new Vector2(progresso, 1f);

        string tecla = h.tecla == KeyCode.LeftShift ? "Shift" : h.tecla.ToString();
        h.texto.Texto = falta > 0f
            ? $"[{tecla}] {h.nome}   <color=#AAAAAA>{falta:0.0}s</color>"
            : $"[{tecla}] {h.nome}   <color=#7CFF8A>PRONTA</color>";
    }
}
