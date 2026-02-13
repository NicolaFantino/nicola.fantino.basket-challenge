using UnityEngine;

public class BackboardBonus : MonoBehaviour {
    [Header("Visuals")]
    [SerializeField] private Renderer boardRenderer;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material bonusMaterial;

    public bool IsActive { get; private set; }
    public int BonusPoints { get; private set; }

    private void Awake() {
        // Get Renderer component if not assigned
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

        //Debug.Log($"BONUS TABELLONE ATTIVO! Valore: {points} punti");
    }

    public void DeactivateBonus() {
        IsActive = false;
        BonusPoints = 0;

        if (boardRenderer != null && normalMaterial != null) {
            boardRenderer.material = normalMaterial;
        }
    }
}
