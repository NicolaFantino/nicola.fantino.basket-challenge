using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private List<Player> players = new List<Player>();
    [SerializeField] private PlayerPositioner positioner;

    [Header("Difficulty Settings")]
    [SerializeField] private float perfectZoneWidth = 0.15f;
    [SerializeField] private float bankZoneWidth = 0.1f;
   

    [Header("Match Settings")]
    [SerializeField] private float matchDuration = 60f;
    private float currentTime;
    private bool isMatchActive = false;

    [Header("Bonus Event Settings")]
    [SerializeField] private BackboardBonus backboardScript;
    [SerializeField] private float bonusDuration = 10f;
    private bool hasBonusEventHappened = false;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        StartCoroutine(StartMatchCountdown());

        // Place players in their initial positions
        foreach (var p in players) {
            SetupTurnForPlayer(p);
        }

        //Setup gameplay UI
        if (GameplayUI.Instance != null && players.Count >= 2) {
            GameplayUI.Instance.SetupHUD(players[0], players[1]);
        }
    }

    private IEnumerator StartMatchCountdown() {
        isMatchActive = false;
        //Debug.Log("Countdown Iniziato...");

        if (GameplayUI.Instance != null) {
           
            GameplayUI.Instance.UpdateCountdownText("3");
            yield return new WaitForSeconds(1f);

            GameplayUI.Instance.UpdateCountdownText("2");
            yield return new WaitForSeconds(1f);

            GameplayUI.Instance.UpdateCountdownText("1");
            yield return new WaitForSeconds(1f);

            GameplayUI.Instance.UpdateCountdownText("GO!");
            yield return new WaitForSeconds(0.5f);

            GameplayUI.Instance.HideCountdown();
        }
        StartMatch();
    }

    private void StartMatch() {
        currentTime = matchDuration;
        isMatchActive = true;
        hasBonusEventHappened = false;

        foreach (var p in players) {
            if (p.IsAI) {
                ThrowBallAI aiBall = p.GetComponentInChildren<ThrowBallAI>();
                if (aiBall != null) aiBall.TakeTurn();
            }
        }

        StartCoroutine(BonusEventRoutine());
        //Debug.Log("Partita Iniziata!");
    }

    private void EndMatch() {
        isMatchActive = false;
        Debug.Log("PARTITA FINITA!");

        Player p1 = players[0];
        Player p2 = players[1];

        bool isWin = p1.Score > p2.Score;
        bool isDraw = p1.Score == p2.Score;

        int trophiesChange = 0;
        int moneyEarned = 0;

        if (isWin) {
            trophiesChange = 25;
            moneyEarned = 100;  
        } else if (isDraw) {
            trophiesChange = 0;
            moneyEarned = 20;
        } else {
            trophiesChange = -20;
            moneyEarned = 10;
        }

        if (GameplayUI.Instance != null) {
            GameplayUI.Instance.ShowGameOver(isWin, p1, p2, trophiesChange, moneyEarned);
        }
    }

    private void SetupTurnForPlayer(Player player) {
        if (positioner == null || player == null) return;

        float distance = positioner.MovePlayerToRandomPosition(player.transform);

        // Based on the distance, we calculate the ideal power for a perfect shot
        float normalizedDist = Mathf.InverseLerp(positioner.MinDistance, positioner.MaxDistance, distance);

        //Definiamo i margini di sicurezza per evitare che le zone escano dallo slider
        float halfPerfectWidth = perfectZoneWidth / 2f;
        float safeMinPower = 0.1f + halfPerfectWidth;
        float safeMaxPower = 0.95f - halfPerfectWidth - bankZoneWidth - 0.05f;

        //Calcola e clampa l'idealPower prima di creare le zone
        float rawIdealPower = Mathf.Lerp(0.35f, 0.85f, normalizedDist);
        float idealPower = Mathf.Clamp(rawIdealPower, safeMinPower, safeMaxPower);

        float minPerfectZone = idealPower - halfPerfectWidth;
        float maxPerfectZone = idealPower + halfPerfectWidth;

        // Assicuriamoci che la bank zone inizi al massimo a 0.90, così c'è spazio per disegnarla
        float minBankZone = Mathf.Clamp(maxPerfectZone + 0.1f, 0.1f, 0.90f);

        float maxBankZone = Mathf.Clamp(minBankZone + bankZoneWidth, 0.1f, 1.0f);

        //Debug.Log($"Perfect Zone: [{minPerfectZone:F2}, {maxPerfectZone:F2}] Bank Zone: [{minBankZone:F2}, {maxBankZone:F2}]");

        //Update player throw zones values
        player.SetThrowZones(minPerfectZone, maxPerfectZone, minBankZone, maxBankZone);

        if (!player.IsAI) {
            if (GameplayUI.Instance.GetPowerBar() != null) {
                GameplayUI.Instance.GetPowerBar().SetupZones(minPerfectZone, maxPerfectZone, minBankZone, maxBankZone);
            }
        } else {
            if (isMatchActive) {
                ThrowBallAI aiBall = player.GetComponentInChildren<ThrowBallAI>();
                if (aiBall != null) aiBall.TakeTurn();
            }
        }
    }

    private void Update() {
        if (!isMatchActive) return;

        //Match conuntdown
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

        //Fireball Timer Update
        if (GameplayUI.Instance != null) {
            foreach (Player p in players) {
                if (p.IsOnFire) {
                   
                    float percentage = p.FireTimer / p.fireDuration;

                    GameplayUI.Instance.UpdateFireBar(percentage, p, true);
                }
            }
        }
    }

    private IEnumerator BonusEventRoutine() {
        //Wait a random time to trigger the bonus event
        float randomWait = Random.Range(10f, matchDuration - 15f);
        yield return new WaitForSeconds(randomWait);

        if (isMatchActive && !hasBonusEventHappened) {
            TriggerBonusEvent();
        }
    }

    private void TriggerBonusEvent() {
        hasBonusEventHappened = true;

        int[] options = { 4, 6, 8 };
        int randomPoints = options[Random.Range(0, options.Length)];

        if (backboardScript != null) {
            backboardScript.ActivateBonus(randomPoints);
            StartCoroutine(DeactivateBonusAfterTime());
        }
    }

    private IEnumerator DeactivateBonusAfterTime() {
        yield return new WaitForSeconds(bonusDuration);
        if (backboardScript != null) backboardScript.DeactivateBonus();
    }

    public void AwardPoints(Player shooter, ThrowBall ball) {
        if (!isMatchActive) return;

        shooter.ScoredThisTurn = true;

        int points = 0;
        bool isBonus = ball.DidHitBonusBoard();
        bool isPerfect = ball.getIsShotPerfect();

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
        //Debug.Log($"{shooter.PlayerName} ha segnato! Totale: {shooter.Score}");

        if (GameplayUI.Instance != null) {

            if (!shooter.IsAI) {
                GameplayUI.Instance.SpawnScorePopup(points, isPerfect, isBonus);
            }

            if (!shooter.IsOnFire) {
                float percentage = (float)shooter.CurrentStreak / shooter.streakToFire;
                GameplayUI.Instance.UpdateFireBar(percentage, shooter, false);
            }
            GameplayUI.Instance.UpdateScore(shooter.PlayerName, shooter.Score, shooter);
        }
    }

    //Method called by ThrowBall
    public void OnShotFinished(Player player) {
        if (!isMatchActive) return;

        if (player != null) {
            if (!player.ScoredThisTurn) {
                player.ResetStreak();

                //Empty the fire bar if the player missed
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
