using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private List<Player> players = new List<Player>();
    [SerializeField] private PlayerPositioner positioner;
    //[SerializeField] private PowerBarUI powerBarUI;

    [Header("Difficulty Settings")]
    [SerializeField] private float perfectZoneWidth = 0.15f;
    [SerializeField] private float bankZoneWidth = 0.1f;
   

    [Header("Match Settings")]
    [SerializeField] private float matchDuration = 60f; // Durata in secondi (es. 1 minuto)
    private float currentTime;
    private bool isMatchActive = false;

    [Header("Bonus Event Settings")]
    [SerializeField] private BackboardBonus backboardScript; // Trascina qui il tabellone
    [SerializeField] private float bonusDuration = 10f; // Quanto dura la luce accesa
    private bool hasBonusEventHappened = false;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        StartCoroutine(StartMatchCountdown());
        //StartMatch();
        //StartCoroutine(BonusEventRoutine());

        // Posizioniamo tutti i giocatori all'inizio
        foreach (var p in players) {
            SetupTurnForPlayer(p);
        }

        // --- NUOVO: Setup della grafica (Avatar e Nomi) ---
        if (GameplayUI.Instance != null && players.Count >= 2) {
            // Assumiamo players[0] = Tu, players[1] = Avversario
            GameplayUI.Instance.SetupHUD(players[0], players[1]);
        }
    }

    // --- NUOVA LOGICA DI START ---
    private IEnumerator StartMatchCountdown() {
        isMatchActive = false; // Blocchiamo tutto all'inizio
        Debug.Log("Countdown Iniziato...");

        if (GameplayUI.Instance != null) {
            // 3
            GameplayUI.Instance.UpdateCountdownText("3");
            yield return new WaitForSeconds(1f);

            // 2
            GameplayUI.Instance.UpdateCountdownText("2");
            yield return new WaitForSeconds(1f);

            // 1
            GameplayUI.Instance.UpdateCountdownText("1");
            yield return new WaitForSeconds(1f);

            // GO!
            GameplayUI.Instance.UpdateCountdownText("GO!");
            yield return new WaitForSeconds(0.5f); // Il GO dura meno

            GameplayUI.Instance.HideCountdown();
        } else {
            // Fallback se non c'è UI, aspettiamo comunque
            yield return new WaitForSeconds(3f);
        }

        // ORA inizia davvero la partita
        StartMatch();
    }

    // --- NUOVO METODO CENTRALE PER GESTIRE IL TURNO ---
    private void SetupTurnForPlayer(Player player) {
        if (positioner == null || player == null) return;

        // 1. Spostiamo il giocatore e otteniamo la distanza
        float distance = positioner.MovePlayerToRandomPosition(player.transform);

        // 2. Calcoliamo la potenza ideale in base alla distanza (Mapping)
        // Vicino (4m) -> Slider basso (0.25). Lontano (15m) -> Slider alto (0.85)
        float normalizedDist = Mathf.InverseLerp(positioner.MinDistance, positioner.MaxDistance, distance);
        float idealPower = Mathf.Lerp(0.25f, 0.85f, normalizedDist);

        // 3. Calcoliamo i range (Min e Max) per le zone
        float minPerfectZone = Mathf.Clamp(idealPower - (perfectZoneWidth / 2f), 0.1f, 0.9f);
        float maxPerfectZone = Mathf.Clamp(idealPower + (perfectZoneWidth / 2f), 0.1f, 0.9f);

        float minBankZone = Mathf.Clamp(maxPerfectZone + 0.05f, 0.1f, 0.95f); // Il Bank shot è un po' più forte
        float maxBankZone = Mathf.Clamp(minBankZone + bankZoneWidth, 0.1f, 1.0f);

        // Aggiorniamo i valori delle zone di tiro nel giocatore
            player.SetThrowZones(minPerfectZone, maxPerfectZone, minBankZone, maxBankZone);

        //Gestione UMANO vs AI
        if (!player.IsAI) {
            // UMANO: Aggiorna la UI (La palla aspetterà l'input del dito)
            if (GameplayUI.Instance.GetPowerBar() != null) {
                GameplayUI.Instance.GetPowerBar().SetupZones(minPerfectZone, maxPerfectZone, minBankZone, maxBankZone);
            }
        } else {
            //AI: Fai partire il timer per il prossimo tiro ---
            // Solo se la partita è attiva (evita che tiri mentre c'è Game Over o Countdown)
            if (isMatchActive) {
                ThrowBallAI aiBall = player.GetComponentInChildren<ThrowBallAI>();
                if (aiBall != null) aiBall.TakeTurn();
            }
        }
    }

    private void Update() {
        if (!isMatchActive) return;

        // Gestione del countdown
        if (currentTime > 0) {
            currentTime -= Time.deltaTime;

            if (GameplayUI.Instance != null) {
                GameplayUI.Instance.UpdateTimer(currentTime);
            }

        } else {
            currentTime = 0;
            if (GameplayUI.Instance != null) {
                GameplayUI.Instance.UpdateTimer(0);
            }
            EndMatch();
        }

        if (GameplayUI.Instance != null) {
            foreach (Player p in players) {
                if (p.IsOnFire) {
                   
                    // Calcola quanto manca (es. 5s rimasti su 10s totali = 0.5f)
                    float percentage = p.FireTimer / p.fireDuration;

                    GameplayUI.Instance.UpdateFireBar(percentage, p, true);
                }
            }
        }
    }

    private IEnumerator BonusEventRoutine() {
        // Aspetta un tempo casuale tra 10s e (DurataPartita - 10s)
        float randomWait = Random.Range(10f, matchDuration - 15f);
        yield return new WaitForSeconds(randomWait);

        if (isMatchActive && !hasBonusEventHappened) {
            TriggerBonusEvent();
        }
    }

    private void TriggerBonusEvent() {
        hasBonusEventHappened = true;

        // Scegliamo a caso tra 4, 6 o 8
        int[] options = { 4, 6, 8 };
        int randomPoints = options[Random.Range(0, options.Length)];

        if (backboardScript != null) {
            backboardScript.ActivateBonus(randomPoints);
            // Spegni dopo tot secondi
            StartCoroutine(DeactivateBonusAfterTime());
        }
    }

    private IEnumerator DeactivateBonusAfterTime() {
        yield return new WaitForSeconds(bonusDuration);
        if (backboardScript != null) backboardScript.DeactivateBonus();
    }

    private void StartMatch() {
        currentTime = matchDuration;
        isMatchActive = true; // Sblocca i controlli
        hasBonusEventHappened = false;

        //FAI PARTIRE L'AI ORA!
        foreach (var p in players) {
            if (p.IsAI) {
                ThrowBallAI aiBall = p.GetComponentInChildren<ThrowBallAI>();
                if (aiBall != null) aiBall.TakeTurn();
            }
        }

        StartCoroutine(BonusEventRoutine()); // Avvia il timer del bonus solo ora
        Debug.Log("Partita Iniziata!");
    }

    private void EndMatch() {
        isMatchActive = false;
        Debug.Log("PARTITA FINITA!");

        // 1. Recupera i giocatori (assumiamo P1 = Umano, P2 = AI)
        Player p1 = players[0];
        Player p2 = players[1];

        // 2. Determina Vincitore
        bool isWin = p1.Score > p2.Score;
        bool isDraw = p1.Score == p2.Score;

        // 3. Calcola Ricompense (Logica simulata)
        int trophiesChange = 0;
        int moneyEarned = 0;

        if (isWin) {
            trophiesChange = 25;  // Hai vinto 25 coppe
            moneyEarned = 100;    // Hai guadagnato 100 monete
        } else if (isDraw) {
            trophiesChange = 0;
            moneyEarned = 20;
        } else {
            trophiesChange = -20; // Hai perso 20 coppe
            moneyEarned = 10;     // Consolazione
        }

        // 4. Chiama la UI
        if (GameplayUI.Instance != null) {
            GameplayUI.Instance.ShowGameOver(isWin, p1, p2, trophiesChange, moneyEarned);
        }
    }

    // 1. Metodo per assegnare SOLO i punti (chiamato dal sensore)
    public void AwardPoints(Player shooter, ThrowBall ball) {
        if (!isMatchActive) return;

        shooter.ScoredThisTurn = true;

        int points = 0;
        bool isBonus = ball.DidHitBonusBoard();
        bool isPerfect = ball.getIsShotPerfect(); // Assicurati che questo metodo sia public in ThrowBall

        if (isBonus) {
            points = ball.GetBonusPointsValue();
        } else {
            points = isPerfect ? 3 : 2;
        }

        if (shooter.IsOnFire) {
            points *= 2;
        }

        shooter.AddScore(points);
        shooter.AddStreak();
        Debug.Log($"{shooter.PlayerName} ha segnato! Totale: {shooter.Score}");

        //AGGIORNAMENTO UI
        if (GameplayUI.Instance != null) {
            // 1. Mostra il Popup e aggiorniamo la firebar (Solo se è il giocatore umano a segnare, opzionale)
            if (!shooter.IsAI) {
                GameplayUI.Instance.SpawnScorePopup(points, isPerfect, isBonus);
            }

            // 2. Aggiorna il punteggio fisso in alto
            if (!shooter.IsOnFire) {
                float percentage = (float)shooter.CurrentStreak / shooter.streakToFire;
                GameplayUI.Instance.UpdateFireBar(percentage, shooter, false);
            }
            GameplayUI.Instance.UpdateScore(shooter.PlayerName, shooter.Score, shooter);
        }
    }

    // Chiamato da ThrowBall.cs
    public void OnShotFinished(Player player) {
        // Se la partita è finita, non cambiamo più le posizioni
        if (!isMatchActive) return;

        if (player != null) {
            // --- MECCANICA FIREBALL: CONTROLLO MISS ---
            // Se la palla è caduta ma il flag è ancora falso... significa che ha padellato!
            if (!player.ScoredThisTurn) {
                player.ResetStreak();

                // Aggiorna la UI per svuotare la barra
                if (GameplayUI.Instance != null) {
                    GameplayUI.Instance.UpdateFireBar(0,player, false);
                }
            }
            player.ScoredThisTurn = false;
            SetupTurnForPlayer(player);
        }
    }

    public bool IsMatchActive => isMatchActive;
}
