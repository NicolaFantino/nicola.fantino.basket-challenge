using UnityEngine;
using System.Collections;

// Eredita dalla classe base!
public class ThrowBallAI : ThrowBall {

    [Header("AI Settings")]
    [SerializeField] private float minThinkTime = 1.0f;
    [SerializeField] private float maxThinkTime = 2.5f;

    [Header("AI Skill (Probabilità 0-1)")]
    [SerializeField] private float perfectChance = 0.4f;
    [SerializeField] private float bankChance = 0.2f;

    // Non serve un Awake personalizzato, usa quello del padre in automatico
    // Non serve un Update personalizzato, usa quello del padre per la caduta

    public void TakeTurn() {
        if (!GameManager.Instance.IsMatchActive) return;
        StartCoroutine(AiShotRoutine());
    }

    private IEnumerator AiShotRoutine() {
        float thinkTime = Random.Range(minThinkTime, maxThinkTime);
        yield return new WaitForSeconds(thinkTime);

        DecideShotOutcome();

        // Usa il metodo del padre!
        ThrowTowardsTarget(finalTarget);
    }

    private void DecideShotOutcome() {
        float roll = Random.value;

        if (roll <= perfectChance) {
            finalTarget = hoopTarget.position;
            perfectShot = true;
        } else if (roll <= perfectChance + bankChance) {
            finalTarget = bankTarget.position;
            pendingBankAssist = true;
        } else {
            float xOffset = Random.Range(-0.5f, 0.5f);
            float zOffset = Random.Range(-1.5f, 1.5f);
            float yOffset = Random.Range(-0.5f, 0.5f);
            finalTarget = hoopTarget.position + new Vector3(xOffset, yOffset, zOffset);
        }
    }
}
