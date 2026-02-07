using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player: MonoBehaviour {

    [SerializeField] private string playerName;
    [SerializeField] private bool isAI = false;

    private int score = 0;

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
}
