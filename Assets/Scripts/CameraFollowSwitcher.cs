using UnityEngine;
using Cinemachine;

public class CameraFollowSwitcher : MonoBehaviour {

    public static CameraFollowSwitcher Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera mainCam;

    // --- MEMORIA DELLO STATO INIZIALE ---
    [SerializeField] private Transform originalFollowTarget;

    // Body (Framing Transposer)
    [SerializeField] private Vector3 originalBodyOffset;
    [SerializeField] private float origX_Damping;
    [SerializeField] private float origY_Damping;
    [SerializeField] private float origZ_Damping;

    // Componenti Cinemachine
    private CinemachineFramingTransposer framingTransposer;

    private void Awake() {
        if (Instance == null) Instance = this;
        framingTransposer = mainCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        /*if (mainCam != null) {
            originalFollowTarget = mainCam.Follow;

            // 1. RECUPERO E SALVATAGGIO DATI BODY (Framing Transposer)
            
            if (framingTransposer != null) {
                originalBodyOffset = framingTransposer.m_TrackedObjectOffset;

                // Salviamo il damping originale (quello "morbido" per la palla)
                origX_Damping = framingTransposer.m_XDamping;
                origY_Damping = framingTransposer.m_YDamping;
                origZ_Damping = framingTransposer.m_ZDamping;
            }
        }*/
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
    }
}
