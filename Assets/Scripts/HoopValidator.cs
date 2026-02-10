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
        ThrowBallDeterministic ball = other.GetComponent<ThrowBallDeterministic>();

        if (ball != null) {
            // LOGICA TRIGGER SUPERIORE
            if (gameObject.name == topTriggerName) {
                // La palla è passata sopra il cerchio
                ball.setPassedTopTrigger(true);
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
