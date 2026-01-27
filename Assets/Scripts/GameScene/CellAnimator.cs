using UnityEngine;
using DG.Tweening;

public class CellAnimator : MonoBehaviour
{
    [SerializeField] float cellSize = 1.1f;
    [SerializeField] float moveDuration = 0.15f;

    public void MoveCell(CellView cell, int x, int y)
    {
        Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0f);

        cell.transform.DOKill();
        cell.transform.DOMove(pos, moveDuration)
            .SetEase(Ease.OutCubic);
    }
}
