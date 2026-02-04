using UnityEngine;
using UnityEngine.UI;

public class PowerBarUI : MonoBehaviour {

    private Slider powerSlider;
    private RectTransform sliderRect;

    [Header("Segments")]
    [SerializeField] private RectTransform bankRect;
    [SerializeField] private RectTransform perfectRect;

    private float totalHeight;

    void Awake() {

        powerSlider = GetComponent<Slider>();
        sliderRect = GetComponent<RectTransform>();

        // Using width because the slider is rotated 90 degrees
        totalHeight = sliderRect.rect.width;

    }

    public void SetupZones(float perferctMin, float perfectMax, float bankMin, float bankMax) {
        // Se non è stato ancora assegnato in Awake
        if (totalHeight <= 0) totalHeight = sliderRect.rect.height;

        // Posizioniamo i segmenti
        PositionSegment(bankRect, bankMin, bankMax);
        PositionSegment(perfectRect, perferctMin, perfectMax);
    }

    private void PositionSegment(RectTransform segment, float min, float max) {
        // totalHeight in realtà è la larghezza (160) perché lo slider è "sdraiato"
        float segmentLength = (max - min) * totalHeight;
        float xPos = min * totalHeight;

        // Impostiamo la lunghezza (Width) del segmento lungo l'asse X dello slider
        // Manteniamo la Height (Y) originale del segmento (es. 20)
        segment.sizeDelta = new Vector2(segmentLength, segment.sizeDelta.y);

        // Lo spostiamo lungo la X (che visivamente è la verticale grazie ai 90°)
        segment.anchoredPosition = new Vector2(xPos, 0);
    }

    public void UpdateUI(float power) {
        powerSlider.value = power; // Aggiorna il fill e l'handle automaticamente
    }

    public void ResetUI() {
        powerSlider.value = 0; // Riporta l'handle alla base
    }

    public void SetVisible(bool isVisible) {
        gameObject.SetActive(isVisible);
    }
}
