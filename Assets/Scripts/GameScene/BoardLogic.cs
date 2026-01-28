using UnityEngine;
using System;

public class BoardLogic
{
    public int width { get; private set; }
    public int height { get; private set; }

    public int[,] gridValues;

    Vector2Int? activeCell;
    Vector2Int? lastMergedCell;

    public Action<Vector2Int, Vector2Int> OnMergeMove; // fromÅ®to ÇÃà⁄ìÆí ím

    int totalChainScore = 0;

    public BoardLogic(int width, int height)
    {
        this.width = width;
        this.height = height;
        gridValues = new int[width, height];
    }

    public Vector2Int? SpawnCell(int x, int value)
    {
        for (int y = 0; y < height; y++)
        {
            if (gridValues[x, y] == 0)
            {
                gridValues[x, y] = value;
                activeCell = new Vector2Int(x, y);
                return activeCell;
            }
        }
        return null;
    }

    public int ResolveChain()
    {
        totalChainScore = 0;
        lastMergedCell = null;
        int safety = 0;

        while (activeCell.HasValue && safety < 100)
        {
            while (TryMergeActiveCell()) { }
            ApplyFall();
            activeCell = FindNextActiveCell();
            safety++;
        }

        return totalChainScore;
    }

    bool TryMergeActiveCell()
    {
        if (!activeCell.HasValue) return false;

        var pos = activeCell.Value;
        int x = pos.x;
        int y = pos.y;
        int value = gridValues[x, y];
        if (value == 0) return false;

        // â∫óDêÊ
        if (y > 0 && gridValues[x, y - 1] == value)
        {
            MergeToDown(x, y);
            return true;
        }

        // ç∂
        if (x > 0 && gridValues[x - 1, y] == value)
        {
            MergeToActive(x, y, x - 1, y);
            return true;
        }

        // âE
        if (x < width - 1 && gridValues[x + 1, y] == value)
        {
            MergeToActive(x, y, x + 1, y);
            return true;
        }

        return false;
    }

    void MergeToDown(int x, int y)
    {
        gridValues[x, y - 1] *= 2;
        gridValues[x, y] = 0;

        lastMergedCell = new Vector2Int(x, y - 1);
        OnMergeMove?.Invoke(new Vector2Int(x, y), lastMergedCell.Value);

        totalChainScore += gridValues[x, y - 1];
        activeCell = lastMergedCell;
    }

    void MergeToActive(int ax, int ay, int bx, int by)
    {
        gridValues[ax, ay] *= 2;
        gridValues[bx, by] = 0;

        lastMergedCell = new Vector2Int(ax, ay);
        OnMergeMove?.Invoke(new Vector2Int(bx, by), lastMergedCell.Value);

        totalChainScore += gridValues[ax, ay];
        activeCell = lastMergedCell;
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
                    if (gridValues[x, y] != 0 && gridValues[x, y - 1] == 0)
                    {
                        gridValues[x, y - 1] = gridValues[x, y];
                        gridValues[x, y] = 0;
                        moved = true;
                    }
                }
            }
        } while (moved);
    }

    Vector2Int? FindNextActiveCell()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int v = gridValues[x, y];
                if (v == 0) continue;

                if (y > 0 && gridValues[x, y - 1] == v) return new Vector2Int(x, y);
                if (x > 0 && gridValues[x - 1, y] == v) return new Vector2Int(x, y);
                if (x < width - 1 && gridValues[x + 1, y] == v) return new Vector2Int(x, y);
            }
        return null;
    }

    public Vector2Int? GetLastMergedCell() => lastMergedCell;
}
