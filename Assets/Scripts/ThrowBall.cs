using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class ThrowBall : MonoBehaviour
{
    private PlayerControls controls;
    private Rigidbody rb;
    private Coroutine swipeTimerCoroutine;
    private Vector3 initialBallPosition;
    private Quaternion initialBallRotation;

    private Vector2 startPos;
    private bool isSwiping = false;

    [Header("Throw Settings")]
    [SerializeField] private float maxForce = 25f;
    [SerializeField] private float maxSwipeDuration = 0.8f;

    [Header("Reset Settings")]
    [SerializeField] private float yResetThreshold = 0f;
    [SerializeField] private float maxLifeTime = 5f;

    [Header("Improved Sensivity")]
    [SerializeField] private float verticalSensitivity = 25f;
    [SerializeField] private float depthSensitivity = 30f;
    [SerializeField] private float minSwipeDistancePercent = 0.05f; // Default is 5% of the screen

    private bool isLaunched = false;
    private Coroutine autoResetCoroutine;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        controls = new PlayerControls();

        //Subscrive to input events
        controls.Gameplay.Click.started += ctx => StartSwipe();
        controls.Gameplay.Click.canceled += ctx => EndSwipe();

            
        initialBallPosition = transform.position;
        initialBallRotation = transform.rotation;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void StartSwipe() {
        //Avoid douple input
        if (isSwiping) return;

        isSwiping = true;

        startPos = controls.Gameplay.Point.ReadValue<Vector2>();
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
    void Update() {
        // 1. Controllo Altezza: se la palla è stata lanciata e scende sotto la soglia
        if (isLaunched && transform.position.y <= yResetThreshold) {
            ResetBall();
        }
    }
    private void Launch() {
        isSwiping = false;

        Vector2 currentPos = controls.Gameplay.Point.ReadValue<Vector2>();
        Vector2 swipeDelta = currentPos - startPos;

        // 1. NORMALIZZAZIONE VERTICALE (Percentuale dell'altezza schermo)
        // Questo garantisce che la forza sia la stessa su ogni risoluzione
        float normY = swipeDelta.y / Screen.height;

        // 2. DEADZONE E FILTRO DIREZIONE
        // Se lo swipe è verso il basso (negativo) o troppo corto (< 5% dello schermo)
        if (normY < minSwipeDistancePercent) {
            ResetBallState();
            return;
        }

        // 3. CALCOLO FORZA DRITTA
        // X è sempre 0 per evitare curve
        float forceX = 0f;
        float forceY = normY * verticalSensitivity; // Altezza della parabola
        float forceZ = normY * depthSensitivity;    // Spinta verso il canestro

        Vector3 finalForce = new Vector3(forceX, forceY, forceZ);

        // Clamp per evitare che swipe estremi sparino la palla fuori campo
        finalForce = Vector3.ClampMagnitude(finalForce, maxForce);

        // 4. APPLICAZIONE FISICA
        rb.isKinematic = false;
        rb.AddForce(finalForce, ForceMode.Impulse);

        // Effetto visivo di rotazione proporzionale allo swipe
        rb.AddTorque(transform.right * (normY * 15f), ForceMode.Impulse);

        OnBallLaunched();
    }

    private void ResetBallState() {
        rb.isKinematic = true;
        isSwiping = false;
    }
    private IEnumerator ResetTimer() {
        yield return new WaitForSeconds(maxLifeTime);
        if (isLaunched) {
            ResetBall();
        }
    }

    public void OnBallLaunched() {
        isLaunched = true;

        // 2. Controllo Tempo: Avviamo il timer per il reset forzato
        if (autoResetCoroutine != null) StopCoroutine(autoResetCoroutine);
        autoResetCoroutine = StartCoroutine(ResetTimer());
    }

    public void ResetBall() {
        // Fermiamo il timer se il reset avviene prima del tempo (es. per la Y)
        if (autoResetCoroutine != null) StopCoroutine(autoResetCoroutine);

        isLaunched = false;

        // Reset Fisico
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset Trasformata
        transform.position = initialBallPosition;
        transform.rotation = initialBallRotation;

        Debug.Log("Palla resettata!");
    }
}
