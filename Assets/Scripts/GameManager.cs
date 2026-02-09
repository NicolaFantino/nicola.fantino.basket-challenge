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
        StartMatch();
        StartCoroutine(BonusEventRoutine());

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

        // 5. Se è il giocatore UMANO, aggiorniamo anche la PowerBar a schermo
        if (!player.IsAI && GameplayUI.Instance.GetPowerBar() != null) {
            GameplayUI.Instance.GetPowerBar().SetupZones(minPerfectZone, maxPerfectZone, minBankZone, maxBankZone);

            // CAMBIO CAMERA
            /*if (CameraManager.Instance != null) {
                CameraManager.Instance.SwitchToPlayerFocus(player.transform);
            }*/
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
        isMatchActive = true;
        hasBonusEventHappened = false;
        Debug.Log("Partita Iniziata!");
    }

    private void EndMatch() {
        isMatchActive = false;
        Debug.Log("TEMPO SCADUTO! La partita è terminata.");

        // Qui potresti disabilitare gli input di tutti i giocatori
        foreach (var player in players) {
            // Esempio: player.BallScript.enabled = false;
        }

        // Mostra il punteggio finale in console
        ShowFinalScores();
    }

    private void ShowFinalScores() {
        Debug.Log("--- CLASSIFICA FINALE ---");
        foreach (var p in players) {
            Debug.Log($"{p.Name}: {p.Score} punti");
        }
    }

    // 1. Metodo per assegnare SOLO i punti (chiamato dal sensore)
    public void AwardPoints(Player shooter, ThrowBallDeterministic ball) {
        if (!isMatchActive) return;

        int points = 0;
        bool isBonus = ball.DidHitBonusBoard();
        bool isPerfect = ball.getIsShotPerfect(); // Assicurati che questo metodo sia public in ThrowBall

        if (isBonus) {
            points = ball.GetBonusPointsValue();
        } else {
            points = isPerfect ? 3 : 2;
        }

        shooter.AddScore(points);
        Debug.Log($"{shooter.Name} ha segnato! Totale: {shooter.Score}");

        //AGGIORNAMENTO UI
        if (GameplayUI.Instance != null) {
            // 1. Mostra il Popup (Solo se è il giocatore umano a segnare, opzionale)
            if (!shooter.IsAI) {
                GameplayUI.Instance.SpawnScorePopup(points, isPerfect, isBonus);
            }

            // 2. Aggiorna il punteggio fisso in alto
            // Assumiamo che il primo della lista sia il P1 (Umano)
            bool isP1 = (players.IndexOf(shooter) == 0);
            GameplayUI.Instance.UpdateScore(shooter.Name, shooter.Score, isP1);
        }
    }

    // Chiamato da ThrowBallDeterministic.cs
    public void OnShotFinished(Player player) {
        // Se la partita è finita, non cambiamo più le posizioni
        if (!isMatchActive) return;

        // Cerchiamo il giocatore che possiede questa palla
       //Player shooter = players.Find(p => p.BallScript == ball);

        if (player != null) {
            // 1. Spostiamo solo il giocatore interessato
            /*if (positioner != null) {
                positioner.MovePlayerToRandomPosition(player.transform);
            }*/
            SetupTurnForPlayer(player);

            // 2. Resettiamo la palla
            //shooter.BallScript.ResetBall();

            //Debug.Log($"Timer: {Mathf.CeilToInt(currentTime)}s | {ballPlayer.Name} ha resettato. Score: {shooter.Score}");
        }
    }
}
