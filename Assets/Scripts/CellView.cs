using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CellView : MonoBehaviour
{
    public int x;
    public int y;

    [SerializeField] private TextMeshPro valueText;
    [SerializeField] private SpriteRenderer bg;

    private void Start()
    {
        bg = GetComponent<SpriteRenderer>();
    }

    public void SetValue(int value)
    {
        valueText.text = value == 0 ? "" : value.ToString();
        bg.color = GetColor(value);
    }

    Color GetColor(int value)
    {
        if (value == 0) return Color.gray;
        if (value == 2) return new Color(0.9f, 0.9f, 0.8f);
        if (value == 4) return new Color(0.9f, 0.8f, 0.6f);
        return Color.white;
    }


}

