using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ControleLanterna : MonoBehaviour
{
    [Header("Ajuste de Direção")]
    [Tooltip("Ajuste se a luz ficar 'de lado'. Use -90, 90 ou 0 conforme o padrão do sprite/luz.")]
    public float offsetAngulo = -90f;

    [Header("Bateria")]
    public float bateriaMaxima = 100f;
    [Tooltip("Quanto da bateria (em %) é gasto por segundo com a lanterna ligada")]
    public float consumoPorSegundo = 1.2f;
    public float nivelFraco = 20f; // abaixo disso a luz começa a piscar

    private float bateria;
    private bool ligada = true;
    private Image barraBateria;
    private TextoUI textoBateria;

    private Camera cam;

    // Luz da lanterna e seus valores originais (antes dos bônus da árvore)
    private Light2D luz;
    private float raioBase, anguloExternoBase, anguloInternoBase, intensidadeBase;

    // Efeitos temporários das habilidades especiais
    private float flashAte = -1f;
    private float expandidaAte = -1f;

    public Light2D Luz => luz;
    public bool Expandida => Time.time < expandidaAte;
    public float Bateria => bateria;
    public bool TemLuz => ligada && bateria > 0f;

    private void Start()
    {
        // Pega a câmera principal do jogo (precisa estar com a tag MainCamera)
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("ControleLanterna: nenhuma câmera com a tag MainCamera foi encontrada.");
        }

        luz = GetComponent<Light2D>();
        if (luz != null)
        {
            raioBase = luz.pointLightOuterRadius;
            anguloExternoBase = luz.pointLightOuterAngle;
            anguloInternoBase = luz.pointLightInnerAngle;
            intensidadeBase = luz.intensity;
        }
        ArvoreHabilidades.Inicializar();

        bateria = bateriaMaxima;
        MontarHudBateria();
    }

    private void Update()
    {
        // Botão direito do mouse liga/desliga a lanterna (desligada não gasta bateria)
        if (Input.GetMouseButtonDown(1)) ligada = !ligada;

        GastarBateria();
        AtualizarHudBateria();
        AplicarStatus();
        RotacionarParaOMouse();
    }

    // Habilidade FLASH: brilho muito forte por um instante
    public void Flash(float duracao)
    {
        flashAte = Time.time + duracao;
    }

    // Habilidade LANTERNA 360°: a luz envolve o jogador por um tempo
    public void Expandir(float duracao)
    {
        expandidaAte = Time.time + duracao;
    }

    // ---------- Bateria ----------

    void GastarBateria()
    {
        if (!TemLuz) return;

        // A árvore pode fazer a bateria durar mais (ex: 1.15 = dura 15% a mais)
        float consumo = consumoPorSegundo / ArvoreHabilidades.Status.multDuracaoBateria;
        if (Expandida) consumo *= 2f; // a luz 360° gasta o dobro
        bateria = Mathf.Max(0f, bateria - consumo * Time.deltaTime);
    }

    // Usado pelas habilidades: só gasta se houver carga suficiente
    public bool TentarConsumir(float quantidade)
    {
        if (!TemLuz || bateria < quantidade) return false;
        bateria -= quantidade;
        return true;
    }

    public void Recarregar(float quantidade)
    {
        bateria = Mathf.Min(bateriaMaxima, bateria + quantidade);
    }

    // Raio e ângulo atuais (já com bônus), usados para saber o que a luz atinge
    public float RaioAtual => luz != null ? luz.pointLightOuterRadius : 0f;
    public float AnguloAtual => luz != null ? luz.pointLightOuterAngle : 0f;

    // Recalcula a luz a partir dos valores base + árvore + efeitos ativos
    void AplicarStatus()
    {
        if (luz == null) return;

        StatusJogador s = ArvoreHabilidades.Status;
        float raio = raioBase * s.multRaioLanterna;
        float anguloExterno = anguloExternoBase * s.multAnguloLanterna;
        float anguloInterno = anguloInternoBase * s.multAnguloLanterna;
        float intensidade = intensidadeBase * s.multIntensidadeLanterna;

        if (Expandida)
        {
            anguloExterno = 360f;
            anguloInterno = 360f;
            raio *= 0.8f; // luz em volta, um pouco mais curta
        }

        if (Time.time < flashAte)
        {
            intensidade *= 4f;
        }

        // Sem bateria (ou desligada): apaga. Bateria fraca: a luz falha.
        if (!TemLuz)
        {
            intensidade = 0f;
        }
        else if (bateria < nivelFraco)
        {
            float ruido = Mathf.PerlinNoise(Time.time * 9f, 0f);
            float chanceFalha = 1f - bateria / nivelFraco; // quanto menos bateria, mais falha
            if (ruido < chanceFalha * 0.55f) intensidade *= 0.15f;
        }

        luz.pointLightOuterRadius = raio;
        luz.pointLightOuterAngle = Mathf.Min(anguloExterno, 360f);
        luz.pointLightInnerAngle = Mathf.Min(anguloInterno, 360f);
        luz.intensity = intensidade;
    }

    // ---------- HUD da bateria (canto superior esquerdo) ----------

    void MontarHudBateria()
    {
        Canvas canvas = UIFabrica.CriarCanvas("Canvas Bateria", null, 100, new Vector2(1600f, 900f));

        Image fundo = UIFabrica.CriarImagem("Bateria", canvas.transform, new Color(0f, 0f, 0f, 0.55f));
        RectTransform rt = fundo.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(16f, -16f);
        rt.sizeDelta = new Vector2(220f, 34f);

        barraBateria = UIFabrica.CriarImagem("Carga", rt, new Color(0.4f, 0.9f, 0.4f, 0.8f));
        RectTransform brt = barraBateria.rectTransform;
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = new Vector2(3f, 3f);
        brt.offsetMax = new Vector2(-3f, -3f);

        textoBateria = UIFabrica.CriarTexto("Texto", rt, "", 16);
        UIFabrica.Preencher(textoBateria.rectTransform);
    }

    void AtualizarHudBateria()
    {
        if (barraBateria == null) return;

        float pct = bateria / bateriaMaxima;
        RectTransform brt = barraBateria.rectTransform;
        brt.anchorMax = new Vector2(pct, 1f);
        barraBateria.color = pct > 0.5f ? new Color(0.4f, 0.9f, 0.4f, 0.8f)
                           : pct > nivelFraco / bateriaMaxima ? new Color(0.95f, 0.8f, 0.3f, 0.8f)
                           : new Color(0.95f, 0.3f, 0.3f, 0.8f);

        string estado = bateria <= 0f ? "SEM BATERIA" : (ligada ? $"{bateria:0}%" : $"DESLIGADA {bateria:0}%");
        textoBateria.Texto = $"<b>Lanterna: {estado}</b>";
    }

    void RotacionarParaOMouse()
    {
        if (cam == null) return;

        // 1. Pega a posição do mouse na tela e converte para o espaço 2D do mundo
        Vector3 posicaoMouse = cam.ScreenToWorldPoint(Input.mousePosition);

        // 2. Calcula a direção entre a lanterna e a posição do mouse
        Vector2 direcao = (Vector2)posicaoMouse - (Vector2)transform.position;
        if (direcao.sqrMagnitude < 0.0001f) return;

        // 3. Calcula o ângulo em graus com base no vetor de direção
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;

        // 4. Aplica a rotação no eixo Z do objeto
        transform.rotation = Quaternion.Euler(0f, 0f, angulo + offsetAngulo);
    }

    // Direção para onde a lanterna aponta (desfaz o offset do sprite)
    public Vector2 DirecaoDaLuz()
    {
        float anguloZ = (transform.eulerAngles.z - offsetAngulo) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(anguloZ), Mathf.Sin(anguloZ));
    }
}
