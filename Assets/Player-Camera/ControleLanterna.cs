using UnityEngine;

public class ControleLanterna : MonoBehaviour
{
    [Header("Ajuste de Direção")]
    [Tooltip("Ajuste se a luz ficar 'de lado'. Use -90, 90 ou 0 conforme o padrão do sprite/luz.")]
    public float offsetAngulo = -90f;

    private Camera cam;

    private void Start()
    {
        // Pega a câmera principal do jogo
        cam = Camera.main;
    }

    private void Update()
    {
        RotacionarParaOJogador();
    }

    void RotacionarParaOJogador()
    {
        // 1. Pega a posição do mouse na tela e converte para o espaço 2D do mundo
        Vector3 posicaoMouse = cam.ScreenToWorldPoint(Input.mousePosition);

        // 2. Calcula a direção entre a lanterna e a posição do mouse
        Vector2 direcao = (Vector2)posicaoMouse - (Vector2)transform.position;

        // 3. Calcula o ângulo em graus com base no vetor de direção
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;

        // 4. Aplica a rotação no eixo Z do objeto
        transform.rotation = Quaternion.Euler(0f, 0f, angulo + offsetAngulo);
    }
}