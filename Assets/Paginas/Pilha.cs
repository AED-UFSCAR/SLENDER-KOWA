using UnityEngine;

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

    // Push
    public void Push(int item)
    {
        if (topo < capacidade - 1)
        {
            topo++;
            elementos[topo] = item;
        }
        else
        {
            Debug.LogWarning("Pilha cheia! Não é possível empilhar mais elementos.");
        }
    }

    // Pop
    public int Pop()
    {
        if (topo >= 0)
        {
            int itemRemovido = elementos[topo];
            topo--;
            return itemRemovido;
        }

        Debug.LogWarning("Pilha vazia!");
        return -1;
    }

    // Peek
    public int Peek()
    {
        if (topo >= 0)
        {
            return elementos[topo];
        }

        return -1;
    }

    // Count
    public int Count
    {
        get { return topo + 1; }
    }
}