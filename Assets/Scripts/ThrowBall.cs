using UnityEngine;

public abstract class ThrowBall : MonoBehaviour {

    protected Rigidbody ballRb;
    protected Player myPlayer;
    protected Vector3 initialBallLocalPosition;
    protected Quaternion initialBallRotation;

    [Header("Target References")]
    [SerializeField] protected Transform hoopTarget;
    [SerializeField] protected Transform bankTarget;
    [SerializeField] protected float timeOfFlight = 1.2f;

    [Header("Reset Settings")]
    [SerializeField] protected float yResetThreshold = -1f;

    [Header("Debug Settings")]
    [SerializeField] protected bool showDebugArcs = true;
    [SerializeField] protected int arcResolution = 20;

    protected bool isLaunched = false;
    protected bool pendingBankAssist = false;
    protected bool perfectShot = false;
    protected bool hitBonusBackboard = false;
    protected int potentialBonusPoints = 0;
    protected Vector3 finalTarget;
    protected bool passedTopTrigger = false;

    protected virtual void Awake() {
        myPlayer = GetComponentInParent<Player>();
        ballRb = GetComponent<Rigidbody>();
        ballRb.isKinematic = true;

        initialBallLocalPosition = transform.localPosition;
        initialBallRotation = transform.rotation;
    }

    protected void ThrowTowardsTarget(Vector3 target) {
        ballRb.isKinematic = false;
        Vector3 velocity = CalculateVelocity(target, transform.position, timeOfFlight);
        ballRb.velocity = velocity;
        ballRb.AddTorque(transform.right * 5f, ForceMode.Impulse);
        isLaunched = true;
    }

    protected Vector3 CalculateVelocity(Vector3 target, Vector3 origin, float time) {
        Vector3 distanceXZ = target - origin;
        distanceXZ.y = 0;
        float sY = target.y - origin.y;
        float sXZ = distanceXZ.magnitude;

        float Vxz = sXZ / time;
        float Vy = (sY / time) + (0.5f * Mathf.Abs(Physics.gravity.y) * time);

        Vector3 result = distanceXZ.normalized * Vxz;
        result.y = Vy;
        return result;
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Backboard")) {
            BackboardBonus bonusScript = collision.gameObject.GetComponent<BackboardBonus>();
            if (bonusScript != null && bonusScript.IsActive) {
                hitBonusBackboard = true;
                potentialBonusPoints = bonusScript.BonusPoints;
            }

            if (pendingBankAssist) {
                pendingBankAssist = false;
                ballRb.velocity = Vector3.zero;
                ballRb.angularVelocity = Vector3.zero;

                Vector3 assistVelocity = CalculateVelocity(hoopTarget.position, transform.position, 0.4f);
                ballRb.velocity = assistVelocity;
                ballRb.AddTorque(transform.right * 2f, ForceMode.Impulse);
            }
        }
    }

    // Controllo caduta della palla
    protected virtual void Update() {
        if (isLaunched && transform.position.y <= yResetThreshold) {
            ResetBall();
            GameManager.Instance.OnShotFinished(myPlayer);
        }
    }

    public virtual void ResetBall() {
        isLaunched = false;
        perfectShot = false;
        hitBonusBackboard = false;
        passedTopTrigger = false;
        potentialBonusPoints = 0;

        
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.isKinematic = true;

        transform.localPosition = initialBallLocalPosition;
        transform.rotation = initialBallRotation;
    }

    public void setPassedTopTrigger(bool passedTopTrigger) {
        this.passedTopTrigger = passedTopTrigger;
    }

    public bool getPassedTopTrigger() {
        return passedTopTrigger;
    }

    // Getters comuni
    public bool getIsShotPerfect() => perfectShot;
    public bool DidHitBonusBoard() => hitBonusBackboard;
    public int GetBonusPointsValue() => potentialBonusPoints;

    private void OnDrawGizmos() {
        if (!showDebugArcs || hoopTarget == null || bankTarget == null) return;

        // Disegniamo l'arco per il tiro perfetto (Verde)
        //DrawTrajectoryArc(hoopTarget.position, Color.green);

        // Disegniamo l'arco per il tabellone (Giallo)
        //DrawTrajectoryArc(bankTarget.position, Color.yellow);

        // 2. Arco del TIRO ATTUALE (Il finalTarget calcolato)
        // Lo disegniamo solo se � stato calcolato almeno una volta
        if (finalTarget != Vector3.zero) {
            Gizmos.color = Color.red;

            // Disegna l'arco effettivo
            DrawTrajectoryArc(finalTarget, Color.red);
        }
    }

    private void DrawTrajectoryArc(Vector3 target, Color color) {
        Gizmos.color = color;
        Vector3 lastPoint = transform.position;

        // Calcoliamo la velocit� iniziale che userebbe il sistema
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
