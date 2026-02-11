using UnityEngine;

public class HoopValidator : MonoBehaviour {
    [Header("Detection Settings")]
    [SerializeField] private string ballTag = "Ball";
    [SerializeField] private string topTriggerName = "Trigger_Top";
    [SerializeField] private string bottomTriggerName = "Trigger_Bottom";

    private void OnTriggerEnter(Collider other) {
        // 1. Controllo Tag
        if (!other.CompareTag(ballTag)) return;

        // 2. Recupero lo script di tiro direttamente dalla palla
        ThrowBall ball = other.GetComponent<ThrowBall>();

        if (ball != null) {
            // LOGICA TRIGGER SUPERIORE
            if (gameObject.name == topTriggerName) {
                // La palla è passata sopra il cerchio
                ball.setPassedTopTrigger(true);

                // MAGIA DEL PERFECT SHOT (Swish Magnet)
                if (ball.getIsShotPerfect()) {
                    Rigidbody rb = other.GetComponent<Rigidbody>();
                    if (rb != null) {
                        // 1. Allinea la palla perfettamente al centro del canestro sull'asse X e Z
                        // (Mantiene la sua altezza Y attuale per non farla teletrasportare visibilmente)
                        Vector3 hoopCenter = transform.position;
                        other.transform.position = new Vector3(hoopCenter.x, other.transform.position.y, hoopCenter.z);

                        // 2. Uccide la velocità orizzontale e la spinge dritta verso il basso
                        // Mathf.Min assicura che se la palla stava salendo, ora scende.
                        rb.velocity = new Vector3(0f, Mathf.Min(rb.velocity.y, -0.1f), 0f);

                        // 3. (Opzionale) Ferma la rotazione eccessiva per un effetto "retina che frena la palla"
                        rb.angularVelocity = rb.angularVelocity * 0.5f;
                    }
                }
            }

            // LOGICA TRIGGER INFERIORE
            if (gameObject.name == bottomTriggerName) {
                // Il punto è valido SOLO SE è passata prima dal trigger sopra 
                // e se non ha già segnato in questo lancio
                if (ball.getPassedTopTrigger()) {

                    // Recuperiamo il componente Player (che è nel genitore della palla)
                    Player shooter = other.GetComponentInParent<Player>();

                    if (shooter != null && GameManager.Instance != null) {
                        // Comunichiamo al manager il giocatore e se il tiro era perfetto
                        GameManager.Instance.AwardPoints(shooter, ball);
                        Debug.Log($"<color=green>CANESTRO VALIDO!</color> Tiratore: {shooter.PlayerName}");
                    }
                }
            }
        }
    }
}
