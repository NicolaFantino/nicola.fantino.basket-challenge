using UnityEngine;

public class BackboardBonus : MonoBehaviour {
    [Header("Visuals")]
    [SerializeField] private Renderer boardRenderer;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material bonusMaterial; // Un materiale emissivo/luminoso

    // Stato interno
    public bool IsActive { get; private set; }
    public int BonusPoints { get; private set; }

    private void Awake() {
        // Se non assegnato manualmente, prova a prenderlo da solo
        if (boardRenderer == null) boardRenderer = GetComponent<Renderer>();
        DeactivateBonus(); // Parte spento
    }

    public void ActivateBonus(int points) {
        IsActive = true;
        BonusPoints = points;

        // Cambio Visuale
        if (boardRenderer != null && bonusMaterial != null) {
            boardRenderer.material = bonusMaterial;
        }

        Debug.Log($"BONUS TABELLONE ATTIVO! Valore: {points} punti");
    }

    public void DeactivateBonus() {
        IsActive = false;
        BonusPoints = 0;

        // Ripristino Visuale
        if (boardRenderer != null && normalMaterial != null) {
            boardRenderer.material = normalMaterial;
        }
    }
}
