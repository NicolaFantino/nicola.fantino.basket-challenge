using System.Collections;
using UnityEngine;

// Eredita dalla classe base!
public class ThrowBallPlayer : ThrowBall {

    private PlayerControls controls;
    private Coroutine swipeTimerCoroutine;
    private Vector2 swipeStartPos;
    private bool isSwiping = false;
    private float currentMaxPower = 0f;

    [Header("UI References")]
    [SerializeField] private PowerBarUI powerBarUI;

    [Header("Throw Settings")]
    [SerializeField] private float maxSwipeDuration = 0.8f;
    [Range(0.2f, 1.0f)]
    [SerializeField] private float screenRangeMaxPower = 0.5f;

    // L'Awake del figlio deve chiamare quello del padre (base.Awake) per prendere il Rigidbody
    protected override void Awake() {
        base.Awake();

        controls = new PlayerControls();
        controls.Gameplay.Click.started += ctx => StartSwipe();
        controls.Gameplay.Click.canceled += ctx => EndSwipe();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void StartSwipe() {
        if (GameManager.Instance != null && !GameManager.Instance.IsMatchActive) return;
        if (isLaunched || isSwiping) return;

        isSwiping = true;
        currentMaxPower = 0f;
        swipeStartPos = controls.Gameplay.Point.ReadValue<Vector2>();
        swipeTimerCoroutine = StartCoroutine(SwipeTimer());
    }

    private void EndSwipe() {
        if (!isSwiping) return;
        StopCoroutine(swipeTimerCoroutine);
        Launch();
    }

    private IEnumerator SwipeTimer() {
        yield return new WaitForSeconds(maxSwipeDuration);
        if (isSwiping) Launch();
    }

    private float GetCurrentPower() {
        Vector2 currentSwipePos = controls.Gameplay.Point.ReadValue<Vector2>();
        float swipeDistance = (currentSwipePos.y - swipeStartPos.y) / Screen.height;
        float calculatedPower = Mathf.Clamp01(swipeDistance / screenRangeMaxPower);
        if (calculatedPower > currentMaxPower) currentMaxPower = calculatedPower;
        return currentMaxPower;
    }

    private void Launch() {
        isSwiping = false;
        float power = GetCurrentPower();
        Debug.Log($"POWER: {power}");
        if (power < 0.1f) {
            ResetBall();
            return;
        }

        // LOGICA DELLE ZONE
        if (power >= myPlayer.MinPerfectZone && power <= myPlayer.MaxPerfectZone) {
            // TIRO PERFETTO
            finalTarget = hoopTarget.position;
            perfectShot = true;
            Debug.Log($"PERFECT SHOT!");
        } else if (power >= myPlayer.MinBankZone && power <= myPlayer.MaxBankZone) {
            // TIRO DI TABELLONE
            finalTarget = bankTarget.position;
            pendingBankAssist = true;
            Debug.Log("BANK SHOT!");
        } else {
            if (power < myPlayer.MinPerfectZone) {
                // CASO A: TIRO CORTO (Air ball o colpisce la base della rete)
                // Usiamo InverseLerp per capire quanto siamo lontani dalla soglia minima
                float shortScale = Mathf.InverseLerp(0.1f, myPlayer.MinPerfectZone, power);
                float zOffset = Mathf.Lerp(-2.5f, -0.6f, shortScale); // Da molto corto a vicino al ferro
                float yOffset = Mathf.Lerp(-1.0f, -0.2f, shortScale);

                finalTarget = hoopTarget.position + new Vector3(0, yOffset, zOffset);
                Debug.Log("MISS: Tiro troppo debole.");
            } else if (power > myPlayer.MaxBankZone) {
                // CASO B: TIRO LUNGO (Sbatte alto sul tabellone o vola oltre)
                float longScale = Mathf.InverseLerp(myPlayer.MaxBankZone, 1.0f, power);
                float zOffset = Mathf.Lerp(0.6f, 3.0f, longScale);
                float yOffset = Mathf.Lerp(0.5f, 1.5f, longScale);

                finalTarget = hoopTarget.position + new Vector3(0, yOffset, zOffset);
                Debug.Log("MISS: Tiro troppo forte.");
            } else {
                // CASO C: "THE GAP" (Tra Bank e Perfect)
                // Colpisce il ferro e rimbalza fuori. Spostiamo il target leggermente a lato.
                float sideOffset = (Random.value > 0.5f) ? 0.35f : -0.35f;
                finalTarget = hoopTarget.position + new Vector3(sideOffset, 0, -0.2f);
                Debug.Log("MISS: Quasi... colpito il ferro!");
            }
        }

        // Usa il metodo del padre per lanciare fisicamente
        ThrowTowardsTarget(finalTarget);

        if (CameraFollowSwitcher.Instance != null) {
            CameraFollowSwitcher.Instance.SwitchToBall(this.transform);
        }
    }

    // Override dell'Update per aggiungere l'aggiornamento della UI
    protected override void Update() {
        base.Update(); // Fallo cadere come fa il padre

        if (isSwiping && powerBarUI != null) {
            powerBarUI.UpdateUI(GetCurrentPower());
        }
    }

    // Override del Reset per aggiungere il reset della UI e della camera
    public override void ResetBall() {
        base.ResetBall();
        isSwiping = false;
        if (powerBarUI != null) powerBarUI.ResetUI();

        if (CameraFollowSwitcher.Instance != null) {
            CameraFollowSwitcher.Instance.ResetToPlayer();
        }
    }
}