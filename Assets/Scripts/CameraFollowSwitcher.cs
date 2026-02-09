using UnityEngine;
using Cinemachine;

public class CameraFollowSwitcher : MonoBehaviour {

    public static CameraFollowSwitcher Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera mainCam;

    // --- MEMORIA DELLO STATO INIZIALE ---
    private Transform originalFollowTarget;

    // Body (Framing Transposer)
    private Vector3 originalBodyOffset;
    private float origX_Damping;
    private float origY_Damping;
    private float origZ_Damping;

    // Aim (Composer)
    private float origHorz_Damping;
    private float origVert_Damping;

    // Componenti Cinemachine
    private CinemachineFramingTransposer framingTransposer;
    private CinemachineComposer composer;

    private void Awake() {
        if (Instance == null) Instance = this;

        if (mainCam != null) {
            originalFollowTarget = mainCam.Follow;

            // 1. RECUPERO E SALVATAGGIO DATI BODY (Framing Transposer)
            framingTransposer = mainCam.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framingTransposer != null) {
                originalBodyOffset = framingTransposer.m_TrackedObjectOffset;

                // Salviamo il damping originale (quello "morbido" per la palla)
                origX_Damping = framingTransposer.m_XDamping;
                origY_Damping = framingTransposer.m_YDamping;
                origZ_Damping = framingTransposer.m_ZDamping;
            }

            // 2. RECUPERO E SALVATAGGIO DATI AIM (Composer)
            composer = mainCam.GetCinemachineComponent<CinemachineComposer>();
            if (composer != null) {
                origHorz_Damping = composer.m_HorizontalDamping;
                origVert_Damping = composer.m_VerticalDamping;
            }
        }
    }

    /// <summary>
    /// Chiamato al LANCIO: Segue la palla con movimento FLUIDO
    /// </summary>
    public void SwitchToBall(Transform ballTransform) {
        if (mainCam == null) return;

        // 1. Cambia Target
        mainCam.Follow = ballTransform;

        // 2. Gestione Body (Framing Transposer)
        if (framingTransposer != null) {
            // Azzera l'offset per stare sulla palla
            framingTransposer.m_TrackedObjectOffset = Vector3.zero;

            // RIPRISTINA il Damping originale (Volo morbido)
            framingTransposer.m_XDamping = origX_Damping;
            framingTransposer.m_YDamping = origY_Damping;
            framingTransposer.m_ZDamping = origZ_Damping;
        }

        // 3. Gestione Aim (Composer)
        if (composer != null) {
            // RIPRISTINA il Damping originale (Rotazione morbida)
            composer.m_HorizontalDamping = origHorz_Damping;
            composer.m_VerticalDamping = origVert_Damping;
        }
    }

    /// <summary>
    /// Chiamato al RESET: Torna al player con scatto ISTANTANEO
    /// </summary>
    public void ResetToPlayer() {
        if (mainCam == null) return;

        // 1. Torna al Player
        mainCam.Follow = originalFollowTarget;

        // 2. Gestione Body
        if (framingTransposer != null) {
            // Ripristina l'offset "sopra la spalla"
            framingTransposer.m_TrackedObjectOffset = originalBodyOffset;

            // AZZERA il Damping (Scatto istantaneo sulla nuova posizione)
            framingTransposer.m_XDamping = 0f;
            framingTransposer.m_YDamping = 0f;
            framingTransposer.m_ZDamping = 0f;
        }

        // 3. Gestione Aim
        if (composer != null) {
            // AZZERA il Damping (Inquadra subito il canestro dalla nuova angolazione)
            composer.m_HorizontalDamping = 0f;
            composer.m_VerticalDamping = 0f;
        }
    }
}
