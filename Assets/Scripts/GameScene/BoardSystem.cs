using UnityEngine;
using System;

/// <summary>
/// 盤面データとゲームロジックを管理するクラス
/// </summary>
public class BoardSystem
{
    public event Action<float> OnDestroyGaugeChanged;

    private int width, height;
    private int[,] grid;
    private Vector2Int? activeCell;

    private float destroyGauge = 0f;
    private const float destroyCost = 33.3f;

    public int ChainScore { get; private set; }
    public int NextValue { get; private set; }

    public BoardSystem(int w, int h)
    {
        width = w;
        height = h;
        grid = new int[w, h];
    }

    // ===== 外部参照 =====
    /// <summary>指定セルの値を取得</summary>
    public int GetValue(int x, int y) => grid[x, y];

    // ===== 生成・落下 =====
    /// <summary>
    /// 指定列にセルを生成し解決を行う
    /// </summary>
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

    /// <summary>盤面の重力・合体処理のメインループ</summary>
    private void Resolve()
    {
        bool changed;
        int safety = 100;

        do
        {
            changed = false;

            // 落下
            if (ApplyFall())
            {
                changed = true;
                activeCell = FindNextActiveCell();
            }

            // 合体
            while (activeCell.HasValue && TryMergeActive())
            {
                changed = true;

                if (ApplyFall())
                    activeCell = FindNextActiveCell();
            }

            if (!changed)
            {
                if (FinalCheckMerge())
                {
                    changed = true;
                }
                else
                {
                    activeCell = FindNextActiveCell();
                }
            }


            safety--;

            if (safety <= 0)
            {
                Debug.LogWarning("Resolve safety hit");
                break;
            }

        } while (changed);
    }


    // ===== 合体 =====
    /// <summary>アクティブセルの隣接セルと合体可能なら合体</summary>
    private bool TryMergeActive()
    {
        if (!activeCell.HasValue) return false;

        int x = activeCell.Value.x;
        int y = activeCell.Value.y;
        int v = grid[x, y];
        if (v == 0) return false;

        // 下
        if (y > 0 && grid[x, y - 1] == v)
        {
            int newValue = v * 2;
            grid[x, y - 1] = newValue;
            grid[x, y] = 0;

            ChainScore += newValue;
            AddDestroyGauge(newValue);

            activeCell = new Vector2Int(x, y - 1);
            return true;
        }

        // 左
        if (x > 0 && grid[x - 1, y] == v)
        {
            int newValue = v * 2;
            grid[x, y] = newValue;
            grid[x - 1, y] = 0;

            ChainScore += newValue;
            AddDestroyGauge(newValue);

            activeCell = new Vector2Int(x, y);
            return true;
        }

        // 右
        if (x < width - 1 && grid[x + 1, y] == v)
        {
            int newValue = v * 2;
            grid[x, y] = newValue;
            grid[x + 1, y] = 0;

            ChainScore += newValue;
            AddDestroyGauge(newValue);

            activeCell = new Vector2Int(x, y);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 全探索
    /// </summary>
    /// <returns>盤面が動くかどうか</returns>
    bool FinalCheckMerge()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int v = grid[x, y];
                if (v == 0) continue;

                // 下
                if (y > 0 && grid[x, y - 1] == v)
                {
                    int newValue = v * 2;
                    Debug.Log($"[FinalCheck] merge at ({x},{y}) v={v}");

                    grid[x, y - 1] = newValue;
                    grid[x, y] = 0;

                    ChainScore += newValue;
                    AddDestroyGauge(newValue);

                    activeCell = new Vector2Int(x, y - 1);
                    return true;
                }

                // 左
                if (x > 0 && grid[x - 1, y] == v)
                {
                    int newValue = v * 2;
                    Debug.Log($"[FinalCheck] merge at ({x},{y}) v={v}");

                    grid[x, y] = newValue;
                    grid[x - 1, y] = 0;

                    ChainScore += newValue;
                    AddDestroyGauge(newValue);

                    activeCell = new Vector2Int(x, y);
                    return true;
                }

                // 右
                if (x < width - 1 && grid[x + 1, y] == v)
                {
                    int newValue = v * 2;
                    Debug.Log($"[FinalCheck] merge at ({x},{y}) v={v}");

                    grid[x, y] = newValue;
                    grid[x + 1, y] = 0;

                    ChainScore += newValue;
                    AddDestroyGauge(newValue);

                    activeCell = new Vector2Int(x, y);
                    return true;
                }
            }
        return false;
    }



    // ===== 重力 =====
    /// <summary>セルを下に落下させる</summary>
    private bool ApplyFall()
    {
        bool moved = false;

        for (int i = 0; i < height; i++)
            for (int x = 0; x < width; x++)
                for (int y = 1; y < height; y++)
                    if (grid[x, y] != 0 && grid[x, y - 1] == 0)
                    {
                        grid[x, y - 1] = grid[x, y];
                        grid[x, y] = 0;
                        moved = true;
                    }

        return moved;
    }

    // ===== 次のアクティブセル探索 =====
    private Vector2Int? FindNextActiveCell()
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

    // ===== 破壊ゲージ =====

    public bool CanConsume(float cost)
    {
        return destroyGauge >= cost;
    }

    public void ConsumeGauge(float cost)
    {
        destroyGauge -= cost;
        OnDestroyGaugeChanged?.Invoke(destroyGauge);
    }

    private void AddDestroyGauge(int value)
    {
        int lv = (int)Mathf.Log(value, 2);
        destroyGauge += lv * lv / 4f;
        if (destroyGauge >= 100) destroyGauge = 100f;
        OnDestroyGaugeChanged?.Invoke(destroyGauge);
    }

    /// <summary>破壊スコアを計算</summary>
    public int DestroyScore(int x, int y)
    {
        int v = grid[x, y];
        if (v == 0) return 0;

        int lv = (int)Mathf.Log(v, 2);
        if (lv < 5) return 0;
        return v * lv * lv;
    }

    /// <summary>セルを破壊して再解決</summary>
    public void DestroyCell(int x, int y)
    {
        grid[x, y] = 0;
        activeCell = new Vector2Int(x, y + 1);
        Resolve();
    }

    // ===== 次セル生成 =====
    public void GenerateNextValue()
    {
        int r = UnityEngine.Random.Range(1, 101);
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
                if (grid[x, y] == 0) return false;
        return true;
    }

    public bool CanDestroy()
    {
        if (!CanConsume(destroyCost)) return false; // 破壊ゲージがたまってるか

        // DestroyScoreが0じゃない(16以下じゃない)かどうか
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (DestroyScore(x, y) > 0)
                {
                    return true; // 16以上だったら破壊可能
                }
            }
        return false;
    }
}
