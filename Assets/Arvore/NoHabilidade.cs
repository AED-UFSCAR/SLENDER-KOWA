using System.Collections.Generic;

// Um nó da árvore de habilidades.
// Cada nó tem um pai (pré-requisito) e uma lista de filhos.
// Os nós finais de algumas trilhas têm um "pai alternativo": eles podem ser
// alcançados por dois caminhos (basta um deles estar completo).
public class NoHabilidade
{
    public string id;
    public string nome;
    public string descricao;
    public int custo;
    public bool desbloqueado;

    // Ligações da árvore
    public NoHabilidade pai;
    public NoHabilidade paiAlternativo; // segundo caminho até este nó (opcional)
    public List<NoHabilidade> filhos = new List<NoHabilidade>();

    // Pequenos bônus nos status (0.05 = +5%)
    public float bonusVelocidade;
    public float bonusRaioLanterna;
    public float bonusAnguloLanterna;
    public float bonusIntensidadeLanterna;
    public float bonusBateria;             // ex: 0.15 = bateria dura +15%

    // Habilidade ativa com cooldown liberada por este nó (null = só bônus)
    public string habilidadeEspecial;

    public NoHabilidade(string id, string nome, string descricao, int custo)
    {
        this.id = id;
        this.nome = nome;
        this.descricao = descricao;
        this.custo = custo;
    }

    // Insere um filho e já liga o pai. Retorna o filho para encadear.
    public NoHabilidade AdicionarFilho(NoHabilidade filho)
    {
        filho.pai = this;
        filhos.Add(filho);
        return filho;
    }

    // Liga um segundo caminho até este nó (sem mudar a estrutura de árvore)
    public void DefinirPaiAlternativo(NoHabilidade outro)
    {
        paiAlternativo = outro;
    }

    // Pode desbloquear se o pai OU o pai alternativo já estiver desbloqueado
    public bool PreRequisitoAtendido()
    {
        if (pai == null) return true;
        return pai.desbloqueado || (paiAlternativo != null && paiAlternativo.desbloqueado);
    }

    public bool EhEspecial()
    {
        return !string.IsNullOrEmpty(habilidadeEspecial);
    }

    public bool EhFolha()
    {
        return filhos.Count == 0;
    }
}
