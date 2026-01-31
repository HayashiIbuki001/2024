using UnityEngine;

public class BoardSystem
{
    private int width;
    private int height;
    private int[,] grid;
    private Vector2Int? activeCell;

    public BoardSystem(int w, int h)
    {
        width = w;
        height = h;
        grid = new int[w, h];
    }

    // 仮：外から activeCell をセットする想定
    public void SetActiveCell(int x, int y)
    {
        activeCell = new Vector2Int(x, y);
    }

    public void ResolveChain()
    {
        while (activeCell.HasValue)
        {
            while (TryMergeActive()) { }

            ApplyFall();
            activeCell = FindNextActiveCell();
        }
    }

    bool TryMergeActive()
    {
        var p = activeCell.Value;
        int x = p.x;
        int y = p.y;
        int v = grid[x, y];

        // 縦（下）
        if (y > 0 && grid[x, y - 1] == v)
        {
            MergeDown(x, y);
            activeCell = new Vector2Int(x, y - 1);
            return true;
        }

        // 横（左）
        if (x > 0 && grid[x - 1, y] == v)
        {
            MergeIntoActive(x, y, x - 1, y);
            return true;
        }

        // 横（右）
        if (x < width - 1 && grid[x + 1, y] == v)
        {
            MergeIntoActive(x, y, x + 1, y);
            return true;
        }

        return false;
    }

    void MergeDown(int x, int y)
    {
        grid[x, y - 1] *= 2;
        grid[x, y] = 0;
    }

    void MergeIntoActive(int ax, int ay, int bx, int by)
    {
        grid[ax, ay] *= 2;
        grid[bx, by] = 0;
    }

    void ApplyFall()
    {
        bool moved;
        do
        {
            moved = false;
            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height; y++)
                {
                    if (grid[x, y] != 0 && grid[x, y - 1] == 0)
                    {
                        grid[x, y - 1] = grid[x, y];
                        grid[x, y] = 0;
                        moved = true;
                    }
                }
            }
        }
        while (moved);
    }

    Vector2Int? FindNextActiveCell()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int v = grid[x, y];
                if (v == 0) continue;

                if (y > 0 && grid[x, y - 1] == v)
                    return new Vector2Int(x, y);

                if (x > 0 && grid[x - 1, y] == v)
                    return new Vector2Int(x, y);

                if (x < width - 1 && grid[x + 1, y] == v)
                    return new Vector2Int(x, y);
            }
        }
        return null;
    }
}
