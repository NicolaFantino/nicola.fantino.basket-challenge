using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerPositioner : MonoBehaviour {
    [Header("Area Settings")]
    [Range(2f, 15f)]
    [SerializeField] private float minDistance = 4f;
    [Range(5f, 25f)]
    [SerializeField] private float maxDistance = 9f;
    [Range(10f, 90f)]
    [SerializeField] private float maxAngle = 45f; // Metà ampiezza del ventaglio

    [Header("Gizmos Settings")]
    [SerializeField] private Color areaColor = new Color(0, 1, 1, 0.3f);
    [SerializeField] private Color boundaryColor = new Color(0, 1, 1, 1f);

    /// <summary>
    /// Sposta il player in una posizione casuale all'interno del ventaglio definito da questo oggetto.
    /// </summary>
    public void MovePlayerToRandomPosition(Transform playerTransform) {
        // 1. Generiamo valori casuali per angolo e distanza
        float randomAngle = Random.Range(-maxAngle, maxAngle);
        float randomDistance = Random.Range(minDistance, maxDistance);

        // 2. Creiamo la rotazione basata sull'angolo (attorno all'asse Y)
        Quaternion rotation = Quaternion.Euler(0, randomAngle, 0);

        // 3. Calcoliamo la direzione finale ruotando il FORWARD locale di questo oggetto
        // In questo modo, il ventaglio segue la freccia blu del manager
        Vector3 finalDirection = rotation * transform.forward;

        // 4. Calcoliamo la posizione finale
        Vector3 newPos = transform.position + (finalDirection * randomDistance);

        // 5. Applichiamo la posizione (mantenendo l'altezza Y del player)
        playerTransform.position = new Vector3(newPos.x, playerTransform.position.y, newPos.z);

        // 6. Facciamo guardare il player verso il centro del manager (il canestro)
        Vector3 lookTarget = new Vector3(transform.position.x, playerTransform.position.y, transform.position.z);
        playerTransform.LookAt(lookTarget);

        Debug.Log($"Player spostato a {randomDistance:F2}m con angolo di {randomAngle:F2}°");
    }

    // =================== LOGICA DEI GIZMOS ===================

    private void OnDrawGizmos() {
        Vector3 center = transform.position;
        Vector3 forward = transform.forward;

        // Disegniamo i due archi (distanza minima e massima)
        DrawGizmoArc(center, forward, minDistance, maxAngle, boundaryColor);
        DrawGizmoArc(center, forward, maxDistance, maxAngle, boundaryColor);

        // Disegniamo i due lati dritti che chiudono il ventaglio
        Vector3 leftBoundary = Quaternion.Euler(0, -maxAngle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, maxAngle, 0) * forward;

        Gizmos.color = boundaryColor;
        Gizmos.DrawLine(center + leftBoundary * minDistance, center + leftBoundary * maxDistance);
        Gizmos.DrawLine(center + rightBoundary * minDistance, center + rightBoundary * maxDistance);

        // Visualizzazione solida opzionale (solo nell'Editor)
#if UNITY_EDITOR
        Handles.color = areaColor;
        Handles.DrawSolidArc(center, Vector3.up, leftBoundary, maxAngle * 2, maxDistance);

        // Sottraiamo l'area interna per chiarezza visiva
        Handles.color = new Color(0, 0, 0, 0.2f);
        Handles.DrawSolidArc(center, Vector3.up, leftBoundary, maxAngle * 2, minDistance);
#endif
    }

    private void DrawGizmoArc(Vector3 center, Vector3 forward, float radius, float angle, Color color) {
        Gizmos.color = color;
        int segments = 20;
        float startAngle = -angle;
        float endAngle = angle;
        float step = (endAngle - startAngle) / segments;

        Vector3 prevPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++) {
            float currentAngle = startAngle + (step * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 currentPoint = center + (direction * radius);

            if (i > 0) Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
}
