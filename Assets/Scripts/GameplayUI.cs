using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour {
    public static GameplayUI Instance { get; private set; }

    [Header("Popups")]
    [SerializeField] private ScorePopup scorePopupPrefab;
    [SerializeField] private Transform popupSpawnPoint; // Dove appaiono i punti (es. centro-alto)

    [Header("HUD References")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Player 1 (Left)")]
    [SerializeField] private Image p1Avatar;
    [SerializeField] private TextMeshProUGUI p1ScoreText;
    [SerializeField] private TextMeshProUGUI p1NameText;

    [Header("Player 2 (Right)")]
    [SerializeField] private Image p2Avatar;
    [SerializeField] private TextMeshProUGUI p2ScoreText;
    [SerializeField] private TextMeshProUGUI p2NameText;

    [Header("External")]
    [SerializeField] private PowerBarUI powerBar; // Riferimento alla tua barra esistente

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    public void SetupHUD(Player player1, Player player2) {
        // Setup Player 1 (Sinistra)
        if (player1 != null) {
            if (p1Avatar != null) p1Avatar.sprite = player1.ProfileImage;
            if (p1NameText != null) p1NameText.text = player1.Name;
            UpdateScore(player1.Name, 0, true);
        }

        // Setup Player 2 (Destra)
        if (player2 != null) {
            if (p2Avatar != null) p2Avatar.sprite = player2.ProfileImage;
            if (p2NameText != null) p2NameText.text = player2.Name;
            UpdateScore(player2.Name, 0, false);
        }
    }

    // --- METODI PER I POPUP ---
    public void SpawnScorePopup(int points, bool isPerfect, bool isBonus) {
        if (scorePopupPrefab != null && popupSpawnPoint != null) {
            // Istanziamo il prefab dentro il Canvas
            ScorePopup popup = Instantiate(scorePopupPrefab, popupSpawnPoint.position, Quaternion.identity, transform);
            popup.Setup(points, isPerfect, isBonus);
        }
    }

    // --- METODI PER L'HUD ---
    public void UpdateTimer(float timeRemaining) {
        if (timerText != null) {
            // Converte i secondi in minuti:secondi
            float minutes = Mathf.FloorToInt(timeRemaining / 60);
            float seconds = Mathf.FloorToInt(timeRemaining % 60);

            // Formatta la stringa (es. "01:00" o "00:09")
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Colore rosso se mancano meno di 10 secondi
            if (timeRemaining <= 10 && timeRemaining > 0) {
                timerText.color = Color.red;
            } else {
                timerText.color = Color.white;
            }
        }
    }

    public void UpdateScore(string playerName, int score, bool isPlayer1) {
        // Esempio semplice: se è il Player 1 aggiorna il testo 1
        if (isPlayer1 && p1ScoreText != null) {
            p1ScoreText.text = score.ToString();
        } else if (!isPlayer1 && p2ScoreText != null) {
            p2ScoreText.text = score.ToString();
        }
    }

    // Wrapper per la PowerBar (così il GameManager parla solo con GameplayUI)
    public PowerBarUI GetPowerBar() {
        return powerBar;
    }
}