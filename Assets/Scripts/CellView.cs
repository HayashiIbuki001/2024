using UnityEngine;
using TMPro;
using DG.Tweening;

public class CellView : MonoBehaviour
{
    [SerializeField] TextMeshPro valueText;
    [SerializeField] SpriteRenderer bg;

    Tween moveTween;

    public void SetValue(int v)
    {
        valueText.text = v == 0 ? "" : v.ToString();
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
