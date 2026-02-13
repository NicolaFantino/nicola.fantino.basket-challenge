using UnityEngine;

public class HoopValidator : MonoBehaviour {
    [Header("Detection Settings")]
    [SerializeField] private string ballTag = "Ball";
    [SerializeField] private string topTriggerName = "Trigger_Top";
    [SerializeField] private string bottomTriggerName = "Trigger_Bottom";

    private void OnTriggerEnter(Collider other) {

        if (!other.CompareTag(ballTag)) return;

        ThrowBall ball = other.GetComponent<ThrowBall>();

        if (ball != null) {
            if (gameObject.name == topTriggerName) {

                ball.setPassedTopTrigger(true);

                if (ball.getIsShotPerfect()) {
                    Rigidbody rb = other.GetComponent<Rigidbody>();
                    if (rb != null) {
                        Vector3 hoopCenter = transform.position;
                        other.transform.position = new Vector3(hoopCenter.x, other.transform.position.y, hoopCenter.z);

                        // Mathf.Min assicura che se la palla stava salendo, ora scende.
                        rb.velocity = new Vector3(0f, Mathf.Min(rb.velocity.y, -0.1f), 0f);
                        rb.angularVelocity = rb.angularVelocity * 0.5f;
                    }
                }
            }

            //If the ball enters the bottom trigger, we check if it passed through the top trigger first to validate the score
            if (gameObject.name == bottomTriggerName) {
                if (ball.getPassedTopTrigger()) {
                    Player shooter = other.GetComponentInParent<Player>();

                    if (shooter != null && GameManager.Instance != null) {
                        GameManager.Instance.AwardPoints(shooter, ball);
                        //Debug.Log($"<color=green>CANESTRO VALIDO!</color> Tiratore: {shooter.PlayerName}");
                    }
                }
            }
        }
    }
}
