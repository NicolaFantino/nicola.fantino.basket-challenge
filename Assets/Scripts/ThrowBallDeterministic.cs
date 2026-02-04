using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBallDeterministic : MonoBehaviour
{
    private PlayerControls controls;
    private Rigidbody rb;
    private Coroutine swipeTimerCoroutine;
    private Vector3 initialBallLocalPosition;
    private Quaternion initialBallRotation;

    private Vector2 swipeStartPos;
    private bool isSwiping = false;

    [Header("Throw Settings")]
    [SerializeField] private float maxSwipeDuration = 0.8f;

    [Tooltip("Percentuale di schermo da percorrere per il 100% di potenza (0.5 = metà schermo)")]
    [Range(0.2f, 1.0f)]
    [SerializeField] private float screenRangeMaxPower = 0.5f;

    [Header("Target References")]
    [SerializeField] private Transform hoopTarget; // Centro canestro
    [SerializeField] private Transform bankTarget; // Punto sul tabellone
    [SerializeField] private float timeOfFlight = 1.2f; // Quanto dura il volo della palla, TODO: da regolare in base alla distanza dal canestro

    [Header("Logic Zones (0 to 1)")]
    [SerializeField] private float perfectThresholdMin = 0.40f;
    [SerializeField] private float perfectThresholdMax = 0.60f;
    [SerializeField] private float bankThresholdMin = 0.75f;
    [SerializeField] private float bankThresholdMax = 0.85f;

    [Header("Reset Settings")]
    [SerializeField] private float yResetThreshold = -1f;
    [SerializeField] private float maxLifeTime = 5f;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugArcs = true;
    [SerializeField] private int arcResolution = 20; // Quanti segmenti compongono la linea

    private bool isLaunched = false;
    private bool pendingBankAssist = false;
    private Coroutine autoResetCoroutine;
    private Vector3 finalTarget;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        controls = new PlayerControls();

        controls.Gameplay.Click.started += ctx => StartSwipe();
        controls.Gameplay.Click.canceled += ctx => EndSwipe();

        initialBallLocalPosition = transform.localPosition;
        initialBallRotation = transform.rotation;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void StartSwipe() {
        if (isLaunched || isSwiping) return;
        isSwiping = true;
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

        if (isSwiping) {
            //Time's up, automatic launch
            Launch();
        }
    }

    private void Launch() {
        isSwiping = false;

        Vector2 currentSwipePos = controls.Gameplay.Point.ReadValue<Vector2>();
        float swipeDistance = (currentSwipePos.y - swipeStartPos.y) / Screen.height;

        // Limita il valore tra 0 e 1 (rappresenta il riempimento della barra)
        float power = Mathf.Clamp01(swipeDistance / screenRangeMaxPower);
        Debug.Log($"Swipe Power: {power}");

        if (power < 0.1f) {
            ResetBallState();
            return;
        }

        // LOGICA DELLE ZONE
        if (power >= perfectThresholdMin && power <= perfectThresholdMax) {
            // TIRO PERFETTO
            finalTarget = hoopTarget.position;
            Debug.Log("PERFECT SHOT!");
        } else if (power >= bankThresholdMin && power <= bankThresholdMax) {
            // TIRO DI TABELLONE
            finalTarget = bankTarget.position;
            pendingBankAssist = true;
            Debug.Log("BANK SHOT!");
        } else {
            if (power < perfectThresholdMin) {
                // CASO A: TIRO CORTO (Air ball o colpisce la base della rete)
                // Usiamo InverseLerp per capire quanto siamo lontani dalla soglia minima
                float shortScale = Mathf.InverseLerp(0.1f, perfectThresholdMin, power);
                float zOffset = Mathf.Lerp(-2.5f, -0.6f, shortScale); // Da molto corto a vicino al ferro
                float yOffset = Mathf.Lerp(-1.0f, -0.2f, shortScale);

                finalTarget = hoopTarget.position + new Vector3(0, yOffset, zOffset);
                Debug.Log("MISS: Tiro troppo debole.");
            } else if (power > bankThresholdMax) {
                // CASO B: TIRO LUNGO (Sbatte alto sul tabellone o vola oltre)
                float longScale = Mathf.InverseLerp(bankThresholdMax, 1.0f, power);
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

        ThrowTowardsTarget(finalTarget);
    }

    private void ThrowTowardsTarget(Vector3 target) {
        rb.isKinematic = false;

        // Calcolo della velocità iniziale necessaria per colpire il target in 'timeOfFlight' secondi
        Vector3 velocity = CalculateVelocity(target, transform.position, timeOfFlight);

        rb.velocity = velocity;

        // Rotazione palla (estetica)
        rb.AddTorque(transform.right * 5f, ForceMode.Impulse);

        OnBallLaunched();
    }

    private Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time) {
        // Distanza sui piani X e Z
        Vector3 distance = target - origin;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0;

        float sY = distance.y;
        float sXZ = distanceXZ.magnitude;

        // Velocità orizzontale costante
        float Vxz = sXZ / time;
        // Velocità verticale con gravità: Vy = (sY - 0.5 * g * t^2) / t
        float Vy = (sY / time) + (0.5f * Mathf.Abs(Physics.gravity.y) * time);

        Vector3 result = distanceXZ.normalized * Vxz;
        result.y = Vy;

        return result;
    }

    private void OnCollisionEnter(Collision collision) {
        // Se il tiro è un Bank Shot e abbiamo colpito il Tabellone
        if (pendingBankAssist && collision.gameObject.CompareTag("Backboard")) {
            pendingBankAssist = false; // Assist usato!

            // 1. Reset totale della palla
            // Fermiamo sia il movimento lineare che la rotazione caotica dell'impatto
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Calcoliamo una nuova velocità per andare dal punto di impatto al canestro
            // Usiamo un tempo di volo molto breve per il rimbalzo (es. 0.4 secondi)
            float bounceTime = 0.4f;
            Vector3 assistVelocity = CalculateVelocity(hoopTarget.position, transform.position, bounceTime);

            // Applichiamo la nuova velocità "corretta"
            rb.velocity = assistVelocity;

            // 4. (Opzionale) Aggiungiamo un leggero backspin estetico
            // Questo rende il canestro visivamente più gratificante
            rb.AddTorque(transform.right * 2f, ForceMode.Impulse);

            Debug.Log("Bank Assist Attivato: Palla diretta al canestro!");
        }
    }

    // --- LOGICA DI RESET (Invariata o quasi) ---

    private void Update() {
        if (isLaunched && transform.position.y <= yResetThreshold) {
            ResetBall();
        }
    }

    private void ResetBallState() {
        rb.isKinematic = true;
        isSwiping = false;
    }

    public void OnBallLaunched() {
        isLaunched = true;
        if (autoResetCoroutine != null) StopCoroutine(autoResetCoroutine);
        autoResetCoroutine = StartCoroutine(ResetTimer());
    }

    private IEnumerator ResetTimer() {
        yield return new WaitForSeconds(maxLifeTime);
        if (isLaunched) ResetBall();
    }

    public void ResetBall() {
        if (autoResetCoroutine != null) StopCoroutine(autoResetCoroutine);
        isLaunched = false;
        isSwiping = false;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = initialBallLocalPosition;
        transform.rotation = initialBallRotation;
    }

    private void OnDrawGizmos() {
        if (!showDebugArcs || hoopTarget == null || bankTarget == null) return;

        // Disegniamo l'arco per il tiro perfetto (Verde)
        //DrawTrajectoryArc(hoopTarget.position, Color.green);

        // Disegniamo l'arco per il tabellone (Giallo)
        //DrawTrajectoryArc(bankTarget.position, Color.yellow);

        // 2. Arco del TIRO ATTUALE (Il finalTarget calcolato)
        // Lo disegniamo solo se è stato calcolato almeno una volta
        if (finalTarget != Vector3.zero) {
            Gizmos.color = Color.red;

            // Disegna l'arco effettivo
            DrawTrajectoryArc(finalTarget, Color.red);
        }
    }

    private void DrawTrajectoryArc(Vector3 target, Color color) {
        Gizmos.color = color;
        Vector3 lastPoint = transform.position;

        // Calcoliamo la velocità iniziale che userebbe il sistema
        Vector3 velocity = CalculateVelocity(target, transform.position, timeOfFlight);

        for (int i = 1; i <= arcResolution; i++) {
            // Calcola il tempo trascorso per questo segmento
            float t = (i / (float)arcResolution) * timeOfFlight;

            // Formula del moto uniformemente accelerato: P = P0 + V0*t + 0.5*g*t^2
            Vector3 nextPoint = transform.position + velocity * t + 0.5f * Physics.gravity * Mathf.Pow(t, 2);

            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }

        // Disegna una piccola sfera sul target finale
        Gizmos.DrawWireSphere(target, 0.1f);
    }
}
