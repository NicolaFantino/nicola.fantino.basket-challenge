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

        ballRb.isKinematic = true;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

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
}
