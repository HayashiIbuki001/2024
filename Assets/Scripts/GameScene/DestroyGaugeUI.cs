using UnityEngine;
using UnityEngine.UI;

public class DestroyGaugeUI : MonoBehaviour
{
    [SerializeField] private Slider gaugeSlider;
    [SerializeField] private RectTransform tick33;
    [SerializeField] private RectTransform tick66;
    [SerializeField] private RectTransform fillArea;

    private float destroyGauge = 0f;
    private const float gaugeStep = 33.3f;

    private void Start()
    {
        UpdateGaugeUI();
        UpdateTicksPosition();
    }

    private void UpdateGaugeUI()
    {
        if (gaugeSlider != null)
        {
            gaugeSlider.value = destroyGauge;

            Image fill = gaugeSlider.fillRect.GetComponent<Image>();

            Color brightYellowGreen = new Color(0.5f, 1f, 0f);

            if (destroyGauge < gaugeStep)
                fill.color = Color.gray; // ”j‰ó•s‰Â
            else if (destroyGauge < gaugeStep * 2)
                fill.color = brightYellowGreen; // ”j‰ó1‰ñ‰Â”\
            else if (destroyGauge < gaugeStep * 3)
                fill.color = Color.yellow; // ”j‰ó2‰ñ‰Â”\
            else
                fill.color = Color.orange; // 99.9f ~ max
        }
    }

    private void UpdateTicksPosition()
    {
        float width = fillArea.rect.width;

        // 33% ‚ÌˆÊ’u
        tick33.anchoredPosition = new Vector2(width * 0.333f, tick33.anchoredPosition.y);

        // 66% ‚ÌˆÊ’u
        tick66.anchoredPosition = new Vector2(width * 0.666f, tick66.anchoredPosition.y);
    }
}
