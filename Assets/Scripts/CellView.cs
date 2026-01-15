using UnityEngine;
using TMPro;

public class CellView : MonoBehaviour
{
    public int x;
    public int y;

    [SerializeField] private TextMeshPro valueText;
    [SerializeField] private SpriteRenderer bg;

    [Header("Color Settings")]
    [SerializeField] private float saturation = 0.8f;
    [SerializeField] private float value = 0.9f;
    [SerializeField] private int maxLevel = 11; // 2048想定

    private void Awake()
    {
        if (!bg) bg = GetComponent<SpriteRenderer>();
    }

    public void SetValue(int v)
    {
        valueText.text = v == 0 ? "" : v.ToString();
        bg.color = GetColor(v);
    }

    Color GetColor(int v)
    {
        if (v == 0) return Color.gray;

        int level = (int)Mathf.Log(v, 2);
        float t = Mathf.Clamp01((float)level / maxLevel);

        // 色相を 0〜360° 回す
        float hue = t; // 0〜1（Unityは0〜1）

        return Color.HSVToRGB(hue, saturation, value);
    }
}
