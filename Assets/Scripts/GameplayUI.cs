using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour {
    public static GameplayUI Instance { get; private set; }

    [SerializeField] private GameObject gameplayUHDPanel;

    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Popups")]
    [SerializeField] private ScorePopup scorePopupPrefab;
    [SerializeField] private Transform popupSpawnPoint;

    [Header("HUD References")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Player 1 (Left)")]
    [SerializeField] private Image p1Avatar;
    [SerializeField] private TextMeshProUGUI p1ScoreText;
    [SerializeField] private TextMeshProUGUI p1NameText;

    [Header("Player 2 (Right)")]
    [SerializeField] private Image p2Avatar;
    [SerializeField] private TextMeshProUGUI p2ScoreText;

    [Header("Firebars")]
    [SerializeField] private Slider p1FireBar;
    [SerializeField] private Slider p2FireBar;
    [SerializeField] private Image p1FireBarFill;
    [SerializeField] private Image p2FireBarFill;
    [SerializeField] private TextMeshProUGUI p2NameText;

    [Header("External")]
    [SerializeField] private PowerBarUI powerBar;

    [Header("--- GAME OVER SCREEN ---")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI p1NameTextFinalScore;
    [SerializeField] private TextMeshProUGUI finalScoreP1Text;
    [SerializeField] private TextMeshProUGUI p2NameTextFinalScore;
    [SerializeField] private TextMeshProUGUI finalScoreP2Text;

    [Header("Rewards")]
    [SerializeField] private TextMeshProUGUI trophyText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GameObject trophyIcon;

    [Header("Images")]
    [SerializeField] private Image p1FinalAvatar;
    [SerializeField] private Image p2FinalAvatar;

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    public void SetupHUD(Player player1, Player player2) {
        // Setup Player 1 (Sinistra)
        if (player1 != null) {
            if (p1Avatar != null) p1Avatar.sprite = player1.ProfileImage;
            if (p1NameText != null) p1NameText.text = player1.PlayerName;
            UpdateScore(player1.PlayerName, 0, player1);
        }

        // Setup Player 2 (Destra)
        if (player2 != null) {
            if (p2Avatar != null) p2Avatar.sprite = player2.ProfileImage;
            if (p2NameText != null) p2NameText.text = player2.PlayerName;
            UpdateScore(player2.PlayerName, 0, player2);
        }
    }

    public void SpawnScorePopup(int points, bool isPerfect, bool isBonus) {
        if (scorePopupPrefab != null && popupSpawnPoint != null) {

            ScorePopup popup = Instantiate(scorePopupPrefab, popupSpawnPoint.position, Quaternion.identity, transform);
            popup.Setup(points, isPerfect, isBonus);
        }
    }

    public void UpdateTimer(float timeRemaining) {
        if (timerText != null) {
            // Converte i secondi in minuti:secondi
            float minutes = Mathf.FloorToInt(timeRemaining / 60);
            float seconds = Mathf.FloorToInt(timeRemaining % 60);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (timeRemaining <= 10 && timeRemaining > 0) {
                timerText.color = Color.red;
            } else {
                timerText.color = Color.white;
            }
        }
    }

    public void UpdateScore(string playerName, int score, Player player) {
        
        if (!player.IsAI && p1ScoreText != null) {
            p1ScoreText.text = score.ToString();
        } else if (player.IsAI && p2ScoreText != null) {
            p2ScoreText.text = score.ToString();
        }
    }

    public void UpdateCountdownText(string text) {
        if (countdownText == null) return;

        countdownText.gameObject.SetActive(true);
        countdownText.text = text;

        // Piccola animazione: resetta la scala e la fa crescere
        StartCoroutine(AnimateCountdownText());
    }

    public void HideCountdown() {
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if(timerText != null) timerText.transform.parent.gameObject.SetActive(true);
    }

    private IEnumerator AnimateCountdownText() {
        float duration = 0.5f;
        float timer = 0f;

        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * 1.2f;

        while (timer < duration) {
            timer += Time.deltaTime;
            float t = timer / duration;
            countdownText.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
    }

    public void UpdateFireBar(float fillPercentage, Player player, bool isOnFire) {
        Slider targetSlider = player.IsAI ? p2FireBar : p1FireBar;
        Image targetFill = player.IsAI ? p2FireBarFill : p1FireBarFill;

        if (targetSlider == null) return;

        targetSlider.value = fillPercentage;

        if (targetFill != null) {
            if (isOnFire) {
                targetFill.color = Color.red;
            } else {
                targetFill.color = new Color(1f, 0.5f, 0f);
            }
        }
    }

    public void ShowGameOver(bool isWin, Player p1, Player p2, int trophiesChange, int moneyEarned) {
        if (gameOverPanel == null) return;

        if (gameplayUHDPanel != null) {
            gameplayUHDPanel.SetActive(false);
        }

        gameOverPanel.SetActive(true);

        // 1. Titolo e Colori
        if (isWin) {
            resultTitleText.text = "VITTORIA!";
            resultTitleText.color = Color.green; // O Oro
        } else if (p1.Score == p2.Score) {
            resultTitleText.text = "PAREGGIO";
            resultTitleText.color = Color.white;
        } else {
            resultTitleText.text = "SCONFITTA";
            resultTitleText.color = Color.red;
        }

        // 2. Dati Giocatori
        if (p1 != null) {
            finalScoreP1Text.text = p1.Score.ToString();
            if (p1FinalAvatar != null) p1FinalAvatar.sprite = p1.ProfileImage;
            if(p1.PlayerName != null) p1NameTextFinalScore.text = p1.PlayerName;
        }
        if (p2 != null) {
            finalScoreP2Text.text = p2.Score.ToString();
            if (p2FinalAvatar != null) p2FinalAvatar.sprite = p2.ProfileImage;
            if(p2.PlayerName != null) p2NameTextFinalScore.text = p2.PlayerName;
        }

        // 3. Ricompense
        // Formattiamo il testo: se positivo metti "+", se negativo tieni il "-"
        string sign = (trophiesChange >= 0) ? "+" : "";
        trophyText.text = $"{sign}{trophiesChange} Trofei";

        // Colore trofei: Verde se guadagni, Rosso se perdi
        trophyText.color = (trophiesChange >= 0) ? Color.green : Color.red;

        moneyText.text = $"+{moneyEarned} Monete";
    }

    public void OnRestartClicked() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuitClicked() {
        //Debug.Log("Uscita dal gioco.");
        Application.Quit();
    }

    public PowerBarUI GetPowerBar() {
        return powerBar;
    }
}