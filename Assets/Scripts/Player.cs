using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player: MonoBehaviour {

    [SerializeField] private string playerName;
    [SerializeField] private Sprite profileImage;
    [SerializeField] private bool isAI = false;

    [Header("Fireball Mechanic")]
    public int streakToFire = 3; // Quanti canestri di fila servono
    public float fireDuration = 10f;

    private int score = 0;
    private float minPerfectZone;
    private float maxPerfectZone;
    private float minBankZone;
    private float maxBankZone;

    // Proprietà pubbliche per permettere al GameManager di leggere i dati
    public string PlayerName => playerName;
    public bool IsAI => isAI;
    public int Score => score;
    public Sprite ProfileImage => profileImage;

    private void Update() {
        // Se è infuocato, il timer scende
        if (IsOnFire) {
            FireTimer -= Time.deltaTime;

            if (FireTimer <= 0) {
                ResetStreak(); // Tempo scaduto, si spegne!
            }
        }
    }

    public void AddScore(int points) {
        score += points;
    }

    public void ResetScore() {
        score = 0;
    }

    public void AddStreak() {
        if (IsOnFire) return;

        CurrentStreak++;
        if (CurrentStreak >= streakToFire) {
            IsOnFire = true;
            FireTimer = fireDuration;
            Debug.Log($"{PlayerName} IS ON FIRE!!!");
        }
    }

    public void ResetStreak() {
        CurrentStreak = 0;
        IsOnFire = false;
        FireTimer = 0f;
        if (GameplayUI.Instance != null && GameManager.Instance != null) {
            GameplayUI.Instance.UpdateFireBar(0f, this, false);
        }
        Debug.Log($"{PlayerName} ha perso la palla di fuoco!");
    }

    public void SetThrowZones(float minPerfect, float maxPerfect, float minBank, float maxBank) {
        minPerfectZone = minPerfect;
        maxPerfectZone = maxPerfect;
        minBankZone = minBank;
        maxBankZone = maxBank;
    }

    public int CurrentStreak { get; private set; }
    public bool IsOnFire { get; private set; }

    public float FireTimer { get; private set; }

    public bool ScoredThisTurn { get; set; } = false;

    public float MinPerfectZone => minPerfectZone;
    public float MaxPerfectZone => maxPerfectZone;
    public float MinBankZone => minBankZone;
    public float MaxBankZone => maxBankZone;

}
