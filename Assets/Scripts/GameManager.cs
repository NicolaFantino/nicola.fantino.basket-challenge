using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("Competition Settings")]
    [SerializeField] private List<Player> players = new List<Player>();
    [SerializeField] private PlayerPositioner positioner;

    [Header("Match Settings")]
    [SerializeField] private float matchDuration = 60f; // Durata in secondi (es. 1 minuto)
    private float currentTime;
    private bool isMatchActive = false;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        StartMatch();
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

    private void StartMatch() {
        currentTime = matchDuration;
        isMatchActive = true;
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
    public void AwardPoints(Player shooter, bool perfectShot) {
        if (!isMatchActive) return;

        int points = perfectShot ? 3 : 2;
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
