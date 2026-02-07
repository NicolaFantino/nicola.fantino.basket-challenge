using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player {
    [SerializeField] private string name;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private ThrowBallDeterministic ballScript;
    [SerializeField] private bool isAI = false;

    private int score = 0;

    // Proprietà pubbliche per permettere al GameManager di leggere i dati
    public string Name => name;
    public Transform PlayerRoot => playerRoot;
    public ThrowBallDeterministic BallScript => ballScript;
    public bool IsAI => isAI;
    public int Score => score;

    public void AddScore(int points) {
        score += points;
    }

    public void ResetScore() {
        score = 0;
    }
}
