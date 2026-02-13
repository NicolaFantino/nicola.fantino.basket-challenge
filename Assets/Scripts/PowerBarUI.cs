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
        PositionSegment(bankRect, bankMin, bankMax);
        PositionSegment(perfectRect, perferctMin, perfectMax);
    }

    private void PositionSegment(RectTransform segment, float min, float max) {
       
        float segmentLength = (max - min) * totalHeight;
        float xPos = min * totalHeight;

        // Set the length of the segment along the X axis of the slider
        segment.sizeDelta = new Vector2(segmentLength, segment.sizeDelta.y);

        //Position segment along x axis, that is vertically
        segment.anchoredPosition = new Vector2(xPos, 0);
    }

    public void UpdateUI(float power) {
        powerSlider.value = power;
    }

    public void ResetUI() {
        powerSlider.value = 0;
    }

    public void SetVisible(bool isVisible) {
        gameObject.SetActive(isVisible);
    }
}
