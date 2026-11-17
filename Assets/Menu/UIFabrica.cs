using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Texto de interface que usa TextMeshPro (nítido em qualquer resolução) quando
// os recursos do TMP estão importados no projeto, e o Text antigo da Unity caso contrário.
public class TextoUI
{
    public readonly Graphic grafico;
    private readonly Text legado;
    private readonly TextMeshProUGUI tmp;

    public TextoUI(Text legado) { this.legado = legado; grafico = legado; }
    public TextoUI(TextMeshProUGUI tmp) { this.tmp = tmp; grafico = tmp; }

    public RectTransform rectTransform => grafico.rectTransform;

    public string Texto
    {
        get => tmp != null ? tmp.text : legado.text;
        set { if (tmp != null) tmp.text = value; else legado.text = value; }
    }

    public Color Cor
    {
        get => grafico.color;
        set => grafico.color = value;
    }

    // Troca a fonte do texto (o TMP precisa de um "Font Asset", criado na hora a partir do .ttf)
    public void DefinirFonte(Font fonte)
    {
        if (fonte == null) return;
        if (tmp != null)
        {
            TMP_FontAsset asset = UIFabrica.ObterFontAsset(fonte);
            if (asset != null) tmp.font = asset;
        }
        else
        {
            legado.font = fonte;
        }
    }

    public bool Negrito
    {
        set
        {
            if (tmp != null) tmp.fontStyle = value ? FontStyles.Bold : FontStyles.Normal;
            else legado.fontStyle = value ? FontStyle.Bold : FontStyle.Normal;
        }
    }
}

// Funções para montar a interface por código (menu, HUDs)
public static class UIFabrica
{
    private static Font fonteLegada;
    private static bool? tmpDisponivel;
    private static readonly System.Collections.Generic.Dictionary<Font, TMP_FontAsset> fontAssets =
        new System.Collections.Generic.Dictionary<Font, TMP_FontAsset>();

    // Fonte do título: coloque um arquivo .ttf/.otf em Assets/Resources/Fontes/ com o nome "Titulo"
    // (ex: Assets/Resources/Fontes/Titulo.ttf). Se não existir, usa a fonte padrão.
    public static Font FonteTitulo()
    {
        return Resources.Load<Font>("Fontes/Titulo");
    }

    public static TMP_FontAsset ObterFontAsset(Font fonte)
    {
        if (fonteAssetsContem(fonte, out TMP_FontAsset asset)) return asset;
        asset = TMP_FontAsset.CreateFontAsset(fonte); // asset dinâmico, gerado em tempo de execução
        fontAssets[fonte] = asset;
        return asset;
    }

    static bool fonteAssetsContem(Font fonte, out TMP_FontAsset asset)
    {
        return fontAssets.TryGetValue(fonte, out asset) && asset != null;
    }

    // O TMP só funciona se "TMP Essential Resources" tiver sido importado
    // (Window > TextMeshPro > Import TMP Essential Resources).
    public static bool TmpDisponivel
    {
        get
        {
            if (tmpDisponivel == null)
            {
                tmpDisponivel = Resources.Load<TMP_Settings>("TMP Settings") != null
                                && TMP_Settings.defaultFontAsset != null;
                if (tmpDisponivel == false)
                {
                    Debug.Log("UIFabrica: TextMeshPro não importado, usando texto antigo (menos nítido). " +
                              "Para texto nítido: Window > TextMeshPro > Import TMP Essential Resources.");
                }
            }
            return tmpDisponivel.Value;
        }
    }

    public static Canvas CriarCanvas(string nome, Transform pai, int ordem, Vector2 resolucaoReferencia)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = ordem;
        canvas.pixelPerfect = false;
        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = resolucaoReferencia;
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static RectTransform CriarRect(string nome, Transform pai)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        return go.GetComponent<RectTransform>();
    }

    // Posiciona pelo centro do pai
    public static void Posicionar(RectTransform rt, Vector2 posicao, Vector2 tamanho)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;
    }

    // Ocupa o pai inteiro, com margem
    public static void Preencher(RectTransform rt, float margem = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(margem, margem);
        rt.offsetMax = new Vector2(-margem, -margem);
    }

    public static Image CriarImagem(string nome, Transform pai, Color cor)
    {
        Image img = CriarRect(nome, pai).gameObject.AddComponent<Image>();
        img.color = cor;
        return img;
    }

    public static TextoUI CriarTexto(string nome, Transform pai, string conteudo, int tamanho,
                                     TextAnchor alinhamento = TextAnchor.MiddleCenter)
    {
        RectTransform rt = CriarRect(nome, pai);
        TextoUI texto;

        if (TmpDisponivel)
        {
            TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.fontSize = tamanho;
            t.richText = true;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Overflow;
            t.alignment = alinhamento == TextAnchor.MiddleLeft ? TextAlignmentOptions.MidlineLeft
                        : alinhamento == TextAnchor.MiddleRight ? TextAlignmentOptions.MidlineRight
                        : TextAlignmentOptions.Center;
            t.raycastTarget = false;
            texto = new TextoUI(t);
        }
        else
        {
            if (fonteLegada == null) fonteLegada = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = fonteLegada;
            t.fontSize = tamanho;
            t.alignment = alinhamento;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            texto = new TextoUI(t);
        }

        texto.Cor = Color.white;
        texto.Texto = conteudo;
        return texto;
    }

    public static Button CriarBotao(string rotulo, Transform pai, Vector2 posicao, Vector2 tamanho,
                                    UnityEngine.Events.UnityAction acao, Color cor, int tamanhoFonte,
                                    out TextoUI texto)
    {
        RectTransform rt = CriarRect("Botao " + rotulo, pai);
        Posicionar(rt, posicao, tamanho);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = cor;
        Button b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(acao);

        texto = CriarTexto("Texto", rt, rotulo, tamanhoFonte);
        Preencher(texto.rectTransform, 6f);
        return b;
    }
}
