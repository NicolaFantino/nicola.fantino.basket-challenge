using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class SwipeTrail : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private float distanceFromCamera = 5f;
    [SerializeField] private float minDistanceBetweenPoints = 0.1f;
    [SerializeField] private float fadeTime = 0.5f;

    private LineRenderer lineRenderer;
    private PlayerControls controls; // Riferimento alla classe generata

    private Vector3 lastPointPosition;
    private Coroutine fadeCoroutine;
    private bool isDrawing = false; // Flag per sapere se stiamo trascinando

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;

        // 1. Inizializziamo i controlli
        controls = new PlayerControls();

        // 2. Iscrizione agli eventi (Come in ThrowBallDeterministic)
        // Quando tocchi lo schermo -> Inizia la scia
        controls.Gameplay.Click.started += ctx => StartTrail();

        // Quando alzi il dito -> Finisci la scia
        controls.Gameplay.Click.canceled += ctx => EndTrail();
    }

    private void OnEnable() {
        controls.Enable();
    }

    private void OnDisable() {
        controls.Disable();
    }

    private void Update() {
        // Se il flag è attivo, aggiorniamo la posizione della linea
        if (isDrawing) {
            UpdateTrail();
        }
    }

    private void StartTrail() {
        // Opzionale: controlla se il gioco è in pausa o Game Over
        if (GameManager.Instance != null && !GameManager.Instance.IsMatchActive) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        isDrawing = true;
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = true;

        // Aggiungi subito il primo punto dove hai toccato
        AddPoint();
    }

    private void UpdateTrail() {
        Vector3 currentPoint = GetWorldPoint();

        // Aggiungi un punto solo se ci siamo mossi abbastanza
        if (Vector3.Distance(currentPoint, lastPointPosition) > minDistanceBetweenPoints) {
            AddPoint();
        }
    }

    private void EndTrail() {
        if (!isDrawing) return;

        isDrawing = false;
        // Avvia la sfumatura invece di spegnere di colpo
        fadeCoroutine = StartCoroutine(FadeOutTrail());
    }

    private void AddPoint() {
        Vector3 newPoint = GetWorldPoint();

        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newPoint);

        lastPointPosition = newPoint;
    }

    // --- QUI CAMBIA LA LETTURA DELLA POSIZIONE ---
    private Vector3 GetWorldPoint() {
        // Invece di Input.mousePosition, leggiamo dal New Input System
        Vector2 inputPos = controls.Gameplay.Point.ReadValue<Vector2>();

        // Convertiamo in Vector3 aggiungendo la profondità Z
        Vector3 screenPos = new Vector3(inputPos.x, inputPos.y, distanceFromCamera);

        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    private IEnumerator FadeOutTrail() {
        float timer = 0;
        Gradient originalGradient = lineRenderer.colorGradient;

        while (timer < fadeTime) {
            timer += Time.deltaTime;
            float alphaMultiplier = 1 - (timer / fadeTime);

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                originalGradient.colorKeys,
                new GradientAlphaKey[] {
                    new GradientAlphaKey(originalGradient.alphaKeys[0].alpha * alphaMultiplier, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );

            lineRenderer.colorGradient = gradient;
            yield return null;
        }

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
        lineRenderer.colorGradient = originalGradient;
    }
}
