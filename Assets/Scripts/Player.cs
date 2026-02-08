using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player: MonoBehaviour {

    [SerializeField] private string playerName;
    [SerializeField] private bool isAI = false;

    private int score = 0;
    private float minPerfectZone;
    private float maxPerfectZone;
    private float minBankZone;
    private float maxBankZone;

    // Proprietà pubbliche per permettere al GameManager di leggere i dati
    public string Name => name;
    public bool IsAI => isAI;
    public int Score => score;

    public void AddScore(int points) {
        score += points;
    }

    public void ResetScore() {
        score = 0;
    }

    public void SetThrowZones(float minPerfect, float maxPerfect, float minBank, float maxBank) {
        minPerfectZone = minPerfect;
        maxPerfectZone = maxPerfect;
        minBankZone = minBank;
        maxBankZone = maxBank;
    }

    public float MinPerfectZone => minPerfectZone;
    public float MaxPerfectZone => maxPerfectZone;
    public float MinBankZone => minBankZone;
    public float MaxBankZone => maxBankZone;

}
