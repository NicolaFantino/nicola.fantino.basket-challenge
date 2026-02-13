using UnityEngine;
using TMPro;
using System.Collections;

public class ScorePopup : MonoBehaviour {

    [Header("References")]
    [SerializeField] private TextMeshProUGUI textMesh;

    [Header("Animation Settings")]
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float lifeTime = 1.5f;
    [SerializeField] private float scaleAmount = 1.5f;

    public void Setup(int points, bool isPerfect, bool isBonus) {

        if (isBonus) {
            textMesh.text = $"+{points}\nBONUS!";
            textMesh.color = Color.yellow;
        } else {
            textMesh.text = isPerfect ? $"+{points}!" : $"+{points}";
            textMesh.color = isPerfect ? Color.green : Color.white;
        }

        StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine() {
        float timer = 0;
        Vector3 initialPos = transform.position;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * scaleAmount;

        while (timer < lifeTime) {
            timer += Time.deltaTime;
            float progress = timer / lifeTime;
            transform.position = initialPos + (Vector3.up * moveSpeed * progress);

            if (progress < 0.2f) {
                transform.localScale = Vector3.Lerp(startScale, endScale, progress * 5);
            } else {
                transform.localScale = Vector3.Lerp(endScale, Vector3.one, (progress - 0.2f));
            }

            //Fade Out
            if (progress > 0.7f) {
                float fadeAlpha = Mathf.Lerp(1, 0, (progress - 0.7f) * 3.3f);
                textMesh.alpha = fadeAlpha;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
