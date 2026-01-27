using UnityEngine;
using TMPro;
using DG.Tweening;

public class CellView : MonoBehaviour
{
    [SerializeField] TextMeshPro valueText;
    [SerializeField] SpriteRenderer bg;
    [SerializeField] private int maxLevel = 11;

    Tween moveTween;

    public void SetValue(int v)
    {
        valueText.text = v == 0 ? "" : v.ToString();

        // 色
        int level = (int)Mathf.Log(v, 2);
        float t = Mathf.Clamp01(level / (float)maxLevel);

        float hue = Mathf.Lerp(0.6f, 0.0f, t);
        float sat = 0.8f;
        float val = 0.9f;

        Color c = Color.HSVToRGB(hue, sat, val);
        bg.color = c;
    }

    public void MoveTo(Vector3 pos, float duration)
    {
        moveTween?.Kill();
        moveTween = transform.DOMove(pos, duration);
    }

    public void PlayMergeEffect()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOScale(1.2f, 0.12f).SetLoops(2, LoopType.Yoyo);
    }

    void OnDestroy()
    {
        moveTween?.Kill();
        transform.DOKill();
    }
}
