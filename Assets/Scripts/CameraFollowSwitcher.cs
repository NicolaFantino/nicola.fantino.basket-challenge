using UnityEngine;
using Cinemachine;

public class CameraFollowSwitcher : MonoBehaviour {

    public static CameraFollowSwitcher Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera mainCam;

    //Reference to the starting transform that the camera follows
    [SerializeField] private Transform originalFollowTarget;

    //Parameters for smooth transition to the ball or instant snap back to the player
    [SerializeField] private Vector3 originalBodyOffset;
    [SerializeField] private float origX_Damping;
    [SerializeField] private float origY_Damping;
    [SerializeField] private float origZ_Damping;

    private CinemachineFramingTransposer framingTransposer;

    private void Awake() {
        if (Instance == null) Instance = this;
        framingTransposer = mainCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        
        framingTransposer.m_TrackedObjectOffset = originalBodyOffset;
        framingTransposer.m_XDamping = origX_Damping;
        framingTransposer.m_YDamping = origY_Damping;
        framingTransposer.m_ZDamping = origZ_Damping;
    }

    //Called by the ThrowBallPlayer script when the ball is thrown
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

    //Called by the ThrowBallPlayer script when the ball is reset
    public void ResetToPlayer() {
        if (mainCam == null) return;

        mainCam.Follow = originalFollowTarget;

        if (framingTransposer != null) {

            framingTransposer.m_TrackedObjectOffset = originalBodyOffset;

            framingTransposer.m_XDamping = 0f;
            framingTransposer.m_YDamping = 0f;
            framingTransposer.m_ZDamping = 0f;
        }
    }
}
