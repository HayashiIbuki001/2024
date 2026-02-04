using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 破壊ゲージのUI更新を管理するクラス
/// </summary>
public class DestroyGaugeUI : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private Slider gaugeSlider;
    [SerializeField] private RectTransform tick33;
    [SerializeField] private RectTransform tick66;
    [SerializeField] private RectTransform fillArea;

    private float destroyGauge = 0f;
    private const float gaugeStep = 33.3f;

    // ===== 初期化 =====
    private void Start()
    {
        gaugeSlider.maxValue = 100f;
        UpdateGaugeUI();
        UpdateTicksPosition();
    }

    /// <summary>
    /// 破壊ゲージの値を設定
    /// </summary>
    public void SetGauge(float value)
    {
        destroyGauge = value;
        UpdateGaugeUI();
    }

    /// <summary>
    /// スライダーと色を更新
    /// </summary>
    private void UpdateGaugeUI()
    {
        if (gaugeSlider == null) return;

        gaugeSlider.value = destroyGauge;
        Image fill = gaugeSlider.fillRect.GetComponent<Image>();

        Color brightYellowGreen = new Color(0.5f, 1f, 0f);

        if (destroyGauge < gaugeStep)
            fill.color = Color.gray;       // 破壊不可
        else if (destroyGauge < gaugeStep * 2)
            fill.color = brightYellowGreen; // 破壊1回可能
        else if (destroyGauge < gaugeStep * 3)
            fill.color = Color.yellow;      // 破壊2回可能
        else
            fill.color = Color.orange;      // 99.9%～最大
    }

    /// <summary>
    /// ゲージの33%と66%位置に目盛りを表示
    /// </summary>
    private void UpdateTicksPosition()
    {
        float width = fillArea.rect.width;
        tick33.anchoredPosition = new Vector2(width * 0.333f, tick33.anchoredPosition.y);
        tick66.anchoredPosition = new Vector2(width * 0.666f, tick66.anchoredPosition.y);
    }
}
