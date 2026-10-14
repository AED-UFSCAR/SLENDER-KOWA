using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    public Transform target; // O objeto que a câmera vai seguir
    public float smoothSpeed = 5f; // Suavidade da câmera
    private Vector3 offset = new Vector3(0, 0, -10f); // Mantém a câmera recuada no Z

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}