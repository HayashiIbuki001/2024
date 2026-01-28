using DG.Tweening;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    [SerializeField] GameObject bgCellPrefab;
    [SerializeField] GameObject cellPrefab;
    [SerializeField] float cellSize = 1.1f;
    [SerializeField] CellAnimator animator;

    CellView[,] cells;
    int width, height;

    public float CellSize => cellSize; // Åöí«â¡

    public void Init(int w, int h)
    {
        width = w;
        height = h;
        cells = new CellView[w, h];
    }

    public void CreateBackground()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Instantiate(bgCellPrefab, new Vector3(x * cellSize, y * cellSize, 1), Quaternion.identity, transform);
    }

    public void CreateCell(int x, int y, int value)
    {
        var obj = Instantiate(cellPrefab);
        obj.transform.position = new Vector3(x * cellSize, y * cellSize, 0);

        var cell = obj.GetComponent<CellView>();
        cell.SetValue(value);

        cells[x, y] = cell;
    }

    public void Refresh(int[,] grid)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int value = grid[x, y];
                if (value == 0)
                {
                    if (cells[x, y] != null)
                    {
                        Destroy(cells[x, y].gameObject);
                        cells[x, y] = null;
                    }
                    continue;
                }

                if (cells[x, y] == null)
                    CreateCell(x, y, value);
                else
                {
                    cells[x, y].SetValue(value);
                    animator.MoveCell(cells[x, y], x, y);
                }
            }
        }
    }

    public void PlayMergeEffect(Vector2Int pos)
    {
        var cell = cells[pos.x, pos.y];
        cell?.PlayMergeEffect();
    }



    public void RemoveCell(int x, int y)
    {
        if (cells[x, y] == null) return;

        Destroy(cells[x, y].gameObject);
        cells[x, y] = null;
    }

}
