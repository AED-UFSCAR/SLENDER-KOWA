using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class CircularBoundary : MonoBehaviour
{
    [Header("Configurações da Borda")]
    public float radius = 16f;       // Raio do mapa
    public int pointsCount = 64;     // Suavidade da curva

    void Awake()
    {
        GenerateCircleCollider();
    }

    // Essa função atualiza a borda na mesma hora em que você muda o valor no Inspector!
    void OnValidate()
    {
        GenerateCircleCollider();
    }

    void GenerateCircleCollider()
    {
        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();

        // Proteção para o editor
        if (edgeCollider == null) return;

        Vector2[] points = new Vector2[pointsCount + 1];

        for (int i = 0; i < pointsCount; i++)
        {
            float angle = i * 2 * Mathf.PI / pointsCount;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            points[i] = new Vector2(x, y);
        }

        points[pointsCount] = points[0];
        edgeCollider.points = points;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}