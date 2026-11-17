using UnityEngine;

// Pilha (LIFO) implementada com vetor, usada para guardar a ordem das páginas coletadas.
public class PilhaPaginas
{
    private int[] elementos;
    private int topo;
    private int capacidade;

    // Construtor
    public PilhaPaginas(int tamanhoMaximo = 8)
    {
        capacidade = tamanhoMaximo;
        elementos = new int[capacidade];
        topo = -1;
    }

    // Push: retorna true se conseguiu empilhar
    public bool Push(int item)
    {
        if (EstaCheia())
        {
            Debug.LogWarning("Pilha cheia! Não é possível empilhar mais elementos.");
            return false;
        }

        topo++;
        elementos[topo] = item;
        return true;
    }

    // Pop: remove e retorna o topo (-1 se a pilha estiver vazia)
    public int Pop()
    {
        if (EstaVazia())
        {
            Debug.LogWarning("Pilha vazia!");
            return -1;
        }

        int itemRemovido = elementos[topo];
        topo--;
        return itemRemovido;
    }

    // Peek: olha o topo sem remover (-1 se a pilha estiver vazia)
    public int Peek()
    {
        if (EstaVazia()) return -1;
        return elementos[topo];
    }

    public bool EstaVazia()
    {
        return topo < 0;
    }

    public bool EstaCheia()
    {
        return topo >= capacidade - 1;
    }

    // Count
    public int Count
    {
        get { return topo + 1; }
    }

    // Cópia dos elementos do topo para a base (índice 0 = topo). Usado pelo HUD.
    public int[] ToArray()
    {
        int[] copia = new int[Count];
        for (int i = 0; i < Count; i++)
        {
            copia[i] = elementos[topo - i];
        }
        return copia;
    }

    // Texto da pilha do topo para a base, útil para debug/HUD. Ex: "[5, 2, 7]"
    public override string ToString()
    {
        if (EstaVazia()) return "[]";

        System.Text.StringBuilder sb = new System.Text.StringBuilder("[");
        for (int i = topo; i >= 0; i--)
        {
            sb.Append(elementos[i]);
            if (i > 0) sb.Append(", ");
        }
        sb.Append("]");
        return sb.ToString();
    }
}
