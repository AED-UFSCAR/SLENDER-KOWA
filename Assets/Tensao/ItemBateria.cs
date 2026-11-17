using UnityEngine;

// Pilha jogada pelo mapa: ao passar por cima, recarrega a lanterna.
// Criada pelo Spawner com um sprite simples gerado por código.
public class ItemBateria : MonoBehaviour
{
    public float recarga = 40f; // % de bateria devolvida

    private static Sprite spriteCache;

    public static ItemBateria Criar(Vector2 posicao)
    {
        GameObject go = new GameObject("Pilha");
        go.transform.position = posicao;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ObterSprite();
        sr.sortingOrder = 5;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        return go.AddComponent<ItemBateria>();
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.GetComponentInParent<PlayerController>() == null) return;

        ControleLanterna lanterna = outro.GetComponentInParent<PlayerController>().GetComponentInChildren<ControleLanterna>();
        if (lanterna == null) lanterna = FindFirstObjectByType<ControleLanterna>();
        if (lanterna == null) return;

        lanterna.Recarregar(recarga);
        Debug.Log($"[PILHA] Lanterna recarregada (+{recarga}%). Bateria: {lanterna.Bateria:0}%");
        Destroy(gameObject);
    }

    // Pilha amarela com o polo cinza (8x14 pixels)
    static Sprite ObterSprite()
    {
        if (spriteCache != null) return spriteCache;

        const int w = 8, h = 14;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color vazio = new Color(0, 0, 0, 0);
        Color corpo = new Color(0.95f, 0.8f, 0.15f, 1f);
        Color faixa = new Color(0.15f, 0.15f, 0.15f, 1f);
        Color polo = new Color(0.7f, 0.7f, 0.7f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = vazio;
                if (x >= 1 && x <= 6 && y <= 11) c = (y >= 4 && y <= 5) ? faixa : corpo;
                if (x >= 3 && x <= 4 && y >= 12) c = polo;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        spriteCache = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
        return spriteCache;
    }
}
