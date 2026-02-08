using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private List<Player> players = new List<Player>();
    [SerializeField] private PlayerPositioner positioner;

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
    }

    private void Update() {
        if (!isMatchActive) return;

        // Gestione del countdown
        if (currentTime > 0) {
            currentTime -= Time.deltaTime;
        } else {
            currentTime = 0;
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

        // 1. Controllo Bonus Tabellone
        if (ball.DidHitBonusBoard()) {
            points = ball.GetBonusPointsValue();
            Debug.Log($"PUNTI BONUS TABELLONE! +{points}");
        }
        // 2. Altrimenti calcolo normale
        else {
            points = ball.getIsShotPerfect() ? 3 : 2;
        }

        shooter.AddScore(points);
        Debug.Log($"{shooter.Name} ha segnato! Totale: {shooter.Score}");
    }

    // Chiamato da ThrowBallDeterministic.cs
    public void OnShotFinished(Player player) {
        // Se la partita è finita, non cambiamo più le posizioni
        if (!isMatchActive) return;

        // Cerchiamo il giocatore che possiede questa palla
       //Player shooter = players.Find(p => p.BallScript == ball);

        if (player != null) {
            // 1. Spostiamo solo il giocatore interessato
            if (positioner != null) {
                positioner.MovePlayerToRandomPosition(player.transform);
            }

            // 2. Resettiamo la palla
            //shooter.BallScript.ResetBall();

            //Debug.Log($"Timer: {Mathf.CeilToInt(currentTime)}s | {ballPlayer.Name} ha resettato. Score: {shooter.Score}");
        }
    }
}
