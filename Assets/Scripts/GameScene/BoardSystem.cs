using UnityEngine;

public class BoardSystem
{
    int width;
    int height;
    int[,] grid;

    Vector2Int? activeCell;

    public int ChainScore { get; private set; }
    public int NextValue { get; private set; }

    public BoardSystem(int w, int h)
    {
        width = w;
        height = h;
        grid = new int[w, h];
    }

    // ===== 外部参照 =====
    public int GetValue(int x, int y) => grid[x, y];

    // ===== 生成 =====
    public bool SpawnAndResolve(int column, int value)
    {
        ChainScore = 0;

        for (int y = 0; y < height; y++)
        {
            if (grid[column, y] != 0) continue;

            grid[column, y] = value;
            activeCell = new Vector2Int(column, y);
            Resolve();
            return true;
        }
        return false;
    }


    // ===== メイン解決 =====
    void Resolve()
    {
        bool changed;
        int safety = 100;

        do
        {
            changed = false;

            if (ApplyFall())
                changed = true;

            while (activeCell.HasValue && TryMergeActive())
            {
                changed = true;
                ApplyFall();
            }

            if (!changed)
                activeCell = FindNextActiveCell();

        } while (changed && safety-- > 0);
    }

    // ===== 合体 =====
    bool TryMergeActive()
    {
        if (!activeCell.HasValue) return false;

        int x = activeCell.Value.x;
        int y = activeCell.Value.y;
        int v = grid[x, y];

        if (v == 0) return false;

        // 下
        if (y > 0 && grid[x, y - 1] == v)
        {
            grid[x, y - 1] *= 2;
            grid[x, y] = 0;
            ChainScore += grid[x, y - 1];
            activeCell = new Vector2Int(x, y - 1);
            return true;
        }

        // 左
        if (x > 0 && grid[x - 1, y] == v)
        {
            grid[x, y] *= 2;
            grid[x - 1, y] = 0;
            ChainScore += grid[x, y];
            return true;
        }

        // 右
        if (x < width - 1 && grid[x + 1, y] == v)
        {
            grid[x, y] *= 2;
            grid[x + 1, y] = 0;
            ChainScore += grid[x, y];
            return true;
        }

        return false;
    }

    // ===== 重力 =====
    bool ApplyFall()
    {
        bool moved = false;

        for (int i = 0; i < height; i++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height; y++)
                {
                    if (grid[x, y] != 0 && grid[x, y - 1] == 0)
                    {
                        grid[x, y - 1] = grid[x, y];
                        grid[x, y] = 0;

                        activeCell = new Vector2Int(x, y - 1);
                        moved = true;
                    }
                }
            }
        }
        return moved;
    }

    // ===== 次のアクティブ探索 =====
    Vector2Int? FindNextActiveCell()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int v = grid[x, y];
                if (v == 0) continue;

                if (y > 0 && grid[x, y - 1] == v) return new Vector2Int(x, y);
                if (x > 0 && grid[x - 1, y] == v) return new Vector2Int(x, y);
                if (x < width - 1 && grid[x + 1, y] == v) return new Vector2Int(x, y);
            }
        return null;
    }

    // ===== 破壊 =====
    public int DestroyScore(int x, int y)
    {
        int v = grid[x, y];
        if (v == 0) return 0;

        int lv = (int)Mathf.Log(v, 2);
        if (lv < 5) return 0;
        return v * lv * lv;
    }

    public void DestroyCell(int x, int y)
    {
        grid[x, y] = 0;
        activeCell = new Vector2Int(x, y + 1);
        Resolve();
    }

    // ===== 次セル =====
    public void GenerateNextValue()
    {
        int r = Random.Range(1, 101);
        if (r <= 60) NextValue = 2;
        else if (r <= 85) NextValue = 4;
        else if (r <= 95) NextValue = 8;
        else NextValue = 16;
    }

    // ===== 判定 =====
    public bool IsBoardFull()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == 0)
                    return false;
        return true;
    }
}
