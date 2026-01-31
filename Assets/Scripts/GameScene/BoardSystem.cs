using UnityEngine;

public class BoardSystem
{
    private int width;
    private int height;
    private int[,] grid;
    private Vector2Int? activeCell;

    public int ChainScore {  get; private set; }
    public int NextValue { get; private set; }

    public BoardSystem(int w, int h)
    {
        width = w;
        height = h;
        grid = new int[w, h];
    }

    public Vector2Int? SpawnAndResolve(int column, int value)
    {
        // 新しいセルを追加
        activeCell = SpawnCell(column, value);
        if (!activeCell.HasValue) return null;

        // 合体＋落下
        ResolveChain();

        return activeCell;
    }

    Vector2Int? SpawnCell(int x, int value)
    {
        for (int y = 0; y < height; y++)
        {
            if (grid[x, y] != 0) continue;

            grid[x, y] = value;
            activeCell = new Vector2Int(x, y);  
            return activeCell;
        }
        return null;
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

    private bool TryMergeActive()
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

    private void MergeDown(int x, int y)
    {
        grid[x, y - 1] *= 2;
        grid[x, y] = 0;

        ChainScore += grid[x, y - 1];
    }

    private void MergeIntoActive(int ax, int ay, int bx, int by)
    {
        grid[ax, ay] *= 2;
        grid[bx, by] = 0;

        ChainScore += grid[ax, ay];
    }

    private void ApplyFall()
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

    private Vector2Int? FindNextActiveCell()
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

    public int DestroyScore(int x, int y)
    {
        int value = grid[x, y];
        int lv = (int)Mathf.Log(value, 2);
        if (lv < 5) return 0;
        return value * lv * lv;
    }

    public void DestroyCell(int x, int y)
    {
        grid[x, y] = 0;

        ApplyFall();

        activeCell = FindNextActiveCell();
        ResolveChain();
    }

    public void GenerateNextValue()
    {
        int r = Random.Range(1, 101);
        if (r <= 60) NextValue =  2;
        if (r <= 85) NextValue = 4;
        if (r <= 95) NextValue = 8;
        NextValue = 16;
    }
}
