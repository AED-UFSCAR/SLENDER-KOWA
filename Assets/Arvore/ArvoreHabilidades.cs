using System.Collections.Generic;
using UnityEngine;

// Status finais do personagem, calculados percorrendo a árvore
public struct StatusJogador
{
    public float multVelocidade;
    public float multRaioLanterna;
    public float multAnguloLanterna;
    public float multIntensidadeLanterna;
    public float multDuracaoBateria;
}

// Árvore de habilidades do jogo.
// É estática para que o que foi escolhido no menu continue valendo na partida,
// e fica salva em PlayerPrefs para durar entre uma execução e outra.
// O jogador ganha 1 ponto a cada página entregue em um pilar.
public static class ArvoreHabilidades
{
    // Pontos que o jogador já começa tendo (útil para testar: coloque 16 para liberar tudo)
    public const int PONTOS_INICIAIS = 0;

    public const string DASH = "dash";
    public const string ATORDOAR = "atordoar";
    public const string EXPANDIR = "expandir";

    private const string CHAVE_DESBLOQUEADOS = "ArvoreHabilidades_Desbloqueados";
    private const string CHAVE_PONTOS = "ArvoreHabilidades_PontosGanhos";

    public static NoHabilidade Raiz { get; private set; }
    public static int PontosGanhos { get; private set; }
    public static int PontosGastos { get; private set; }
    public static int PontosDisponiveis => PONTOS_INICIAIS + PontosGanhos - PontosGastos;
    public static StatusJogador Status { get; private set; }

    private static bool inicializada = false;

    public static void Inicializar()
    {
        if (inicializada) return;
        inicializada = true;

        ConstruirArvore();
        Carregar();
        Recalcular();
    }

    //                         Sobrevivente
    //            /                 |                  \
    //     Passos Leves      Pilhas Alcalinas      Lente Ampla
    //       /      \               |                   |
    //   Fôlego   Tênis Novo    Lâmpada LED         Lente Limpa
    //     |          |             |                /       \
    // Passada L.  Adrenalina  Refletor Polido   Abertura   Lente Grande
    //       \      /            /       \           \       /
    //        DASH         Foco Conc.  FLASH          LANTERNA 360°
    //
    // DASH e LANTERNA 360° podem ser alcançados por qualquer um dos dois caminhos.
    static void ConstruirArvore()
    {
        Raiz = new NoHabilidade("raiz", "Sobrevivente", "Início da árvore", 0) { desbloqueado = true };

        // ----- Trilha da velocidade -----
        NoHabilidade passos = Raiz.AdicionarFilho(Buff("passos", "Passos Leves", "+5% velocidade", v: 0.05f));
        NoHabilidade folego = passos.AdicionarFilho(Buff("folego", "Fôlego", "+5% velocidade", v: 0.05f));
        NoHabilidade passada = folego.AdicionarFilho(Buff("passada", "Passada Longa", "+7% velocidade", v: 0.07f));
        NoHabilidade tenis = passos.AdicionarFilho(Buff("tenis", "Tênis Novo", "+5% velocidade", v: 0.05f));
        NoHabilidade adrenalina = tenis.AdicionarFilho(Buff("adrenalina", "Adrenalina", "+8% velocidade", v: 0.08f));
        NoHabilidade dash = passada.AdicionarFilho(Especial("dash", "DASH", "[Shift] Avanço rápido\nRecarga: 4s", DASH));
        dash.DefinirPaiAlternativo(adrenalina);

        // ----- Trilha da intensidade da lanterna -----
        NoHabilidade alcalinas = Raiz.AdicionarFilho(Buff("alcalinas", "Pilhas Alcalinas", "+6% brilho, +15% bateria", i: 0.06f));
        alcalinas.bonusBateria = 0.15f;
        NoHabilidade led = alcalinas.AdicionarFilho(Buff("led", "Lâmpada LED", "+8% brilho", i: 0.08f));
        NoHabilidade refletor = led.AdicionarFilho(Buff("refletor", "Refletor Polido", "+10% brilho", i: 0.10f));
        refletor.AdicionarFilho(Buff("foco", "Foco Concentrado", "+7% brilho", i: 0.07f));
        refletor.AdicionarFilho(Especial("flash", "FLASH", "[Q] Atordoa o Kowa\nRecarga: 10s", ATORDOAR));

        // ----- Trilha do raio da lanterna -----
        NoHabilidade lenteAmpla = Raiz.AdicionarFilho(Buff("lenteampla", "Lente Ampla", "+6% alcance", r: 0.06f));
        NoHabilidade lenteLimpa = lenteAmpla.AdicionarFilho(Buff("lentelimpa", "Lente Limpa", "+7% alcance", r: 0.07f));
        NoHabilidade abertura = lenteLimpa.AdicionarFilho(Buff("abertura", "Abertura", "+8% abertura", a: 0.08f));
        NoHabilidade lenteGrande = lenteLimpa.AdicionarFilho(Buff("lentegrande", "Lente Grande", "+8% alcance", r: 0.08f));
        NoHabilidade expandir = lenteGrande.AdicionarFilho(Especial("lanterna360", "LANTERNA 360°", "[F] Luz ao redor\nRecarga: 15s", EXPANDIR));
        expandir.DefinirPaiAlternativo(abertura);
    }

    static NoHabilidade Buff(string id, string nome, string desc,
                             float v = 0f, float r = 0f, float a = 0f, float i = 0f)
    {
        return new NoHabilidade(id, nome, desc, 1)
        {
            bonusVelocidade = v,
            bonusRaioLanterna = r,
            bonusAnguloLanterna = a,
            bonusIntensidadeLanterna = i
        };
    }

    static NoHabilidade Especial(string id, string nome, string desc, string habilidade)
    {
        return new NoHabilidade(id, nome, desc, 2) { habilidadeEspecial = habilidade };
    }

    // ---------- Operações da árvore ----------

    // Busca em profundidade (recursiva) por id
    public static NoHabilidade Buscar(string id)
    {
        Inicializar();
        return Buscar(Raiz, id);
    }

    static NoHabilidade Buscar(NoHabilidade no, string id)
    {
        if (no == null) return null;
        if (no.id == id) return no;

        foreach (NoHabilidade filho in no.filhos)
        {
            NoHabilidade achado = Buscar(filho, id);
            if (achado != null) return achado;
        }
        return null;
    }

    // Lista todos os nós em pré-ordem (raiz, depois filhos da esquerda para a direita)
    public static List<NoHabilidade> ListarPreOrdem()
    {
        Inicializar();
        List<NoHabilidade> lista = new List<NoHabilidade>();
        PreOrdem(Raiz, lista);
        return lista;
    }

    static void PreOrdem(NoHabilidade no, List<NoHabilidade> lista)
    {
        lista.Add(no);
        foreach (NoHabilidade filho in no.filhos) PreOrdem(filho, lista);
    }

    // O jogador tem a habilidade especial se algum nó desbloqueado a libera
    public static bool PossuiHabilidade(string habilidade)
    {
        foreach (NoHabilidade no in ListarPreOrdem())
        {
            if (no.desbloqueado && no.habilidadeEspecial == habilidade) return true;
        }
        return false;
    }

    public static bool PodeDesbloquear(NoHabilidade no)
    {
        return no != null && !no.desbloqueado && no.PreRequisitoAtendido() && PontosDisponiveis >= no.custo;
    }

    public static bool Desbloquear(NoHabilidade no)
    {
        Inicializar();
        if (!PodeDesbloquear(no)) return false;

        no.desbloqueado = true;
        Salvar();
        Recalcular();
        return true;
    }

    // Devolve todos os pontos gastos (mantém só a raiz)
    public static void Resetar()
    {
        Inicializar();
        foreach (NoHabilidade no in ListarPreOrdem())
        {
            no.desbloqueado = (no == Raiz);
        }
        Salvar();
        Recalcular();
    }

    // Chamado pelo pilar quando uma página é entregue
    public static void GanharPonto(int quantidade = 1)
    {
        Inicializar();
        PontosGanhos += quantidade;
        Salvar();
    }

    // Soma os bônus de todos os nós desbloqueados (percurso recursivo em pré-ordem)
    static void Recalcular()
    {
        StatusJogador s = new StatusJogador
        {
            multVelocidade = 1f,
            multRaioLanterna = 1f,
            multAnguloLanterna = 1f,
            multIntensidadeLanterna = 1f,
            multDuracaoBateria = 1f
        };
        int gastos = 0;
        SomarBonus(Raiz, ref s, ref gastos);

        Status = s;
        PontosGastos = gastos;
    }

    static void SomarBonus(NoHabilidade no, ref StatusJogador s, ref int gastos)
    {
        if (no.desbloqueado)
        {
            s.multVelocidade += no.bonusVelocidade;
            s.multRaioLanterna += no.bonusRaioLanterna;
            s.multAnguloLanterna += no.bonusAnguloLanterna;
            s.multIntensidadeLanterna += no.bonusIntensidadeLanterna;
            s.multDuracaoBateria += no.bonusBateria;
            gastos += no.custo;
        }

        // Visita os filhos mesmo se este estiver bloqueado, porque um nó
        // com pai alternativo pode ter sido liberado pelo outro caminho
        foreach (NoHabilidade filho in no.filhos) SomarBonus(filho, ref s, ref gastos);
    }

    // ---------- Salvar / carregar ----------

    static void Salvar()
    {
        List<string> ids = new List<string>();
        foreach (NoHabilidade no in ListarPreOrdem())
        {
            if (no.desbloqueado && no != Raiz) ids.Add(no.id);
        }
        PlayerPrefs.SetString(CHAVE_DESBLOQUEADOS, string.Join(",", ids));
        PlayerPrefs.SetInt(CHAVE_PONTOS, PontosGanhos);
        PlayerPrefs.Save();
    }

    static void Carregar()
    {
        PontosGanhos = PlayerPrefs.GetInt(CHAVE_PONTOS, 0);

        string salvo = PlayerPrefs.GetString(CHAVE_DESBLOQUEADOS, "");
        if (string.IsNullOrEmpty(salvo)) return;

        foreach (string id in salvo.Split(','))
        {
            NoHabilidade no = Buscar(Raiz, id);
            if (no != null) no.desbloqueado = true;
        }
    }
}
