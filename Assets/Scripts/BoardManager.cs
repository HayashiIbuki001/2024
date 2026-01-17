using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // ===== 設定 =====
    [SerializeField] GameObject cellPrefab;
    [SerializeField] float cellSize = 1.1f;
    [SerializeField] float fallTime = 0.15f;
    [SerializeField] int width = 4;
    [SerializeField] int height = 4;

    // ===== 盤面データ =====
    int[,] gridValues;          // 数値だけの盤面
    CellView[,] cells;          // 見た目用セル

    Vector2Int? activeCell;     // 今合体判定しているセル

    void Start()
    {
        gridValues = new int[width, height];
        cells = new CellView[width, height];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) DropByPlayer(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) DropByPlayer(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) DropByPlayer(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) DropByPlayer(3);
    }

    /// <summary>
    /// プレイヤー操作
    /// </summary>
    /// <param name="x">落下する行</param>
    void DropByPlayer(int x)
    {
        // セル生成 → アクティブセルに設定
        activeCell = SpawnCell(x, 2);

        // 盤面の合体・落下を解決
        ResolveChain();
    }

    /// <summary>
    /// 盤面処理
    /// </summary>
    void ResolveChain()
    {
        // 合体できるセルがある限り続ける
        while (activeCell.HasValue)
        {
            // 合体できるだけ合体
            while (TryMergeActiveCell()) { }

            // 重力適用
            ApplyFall();

            // 次に合体できるセルを探す
            activeCell = FindNextActiveCell();
        }
    }

    /// <summary>
    /// 合体判定
    /// </summary>
    /// <returns></returns>
    bool TryMergeActiveCell()
    {
        var pos = activeCell.Value;
        int x = pos.x;
        int y = pos.y;
        int value = gridValues[x, y];

        // ▼ 縦合体（必ず下）
        if (y > 0 && gridValues[x, y - 1] == value)
        {
            MergeToDown(x, y);
            return true;
        }

        // ▼ 横合体（主役セルに吸収）
        if (x > 0 && gridValues[x - 1, y] == value)
        {
            MergeToActive(x, y, x - 1, y);
            return true;
        }
        if (x < width - 1 && gridValues[x + 1, y] == value)
        {
            MergeToActive(x, y, x + 1, y);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 合体処理(縦)
    /// </summary>
    void MergeToDown(int x, int y)
    {
        // 数値処理
        gridValues[x, y - 1] *= 2;
        gridValues[x, y] = 0;

        // 見た目処理
        var main = cells[x, y - 1]; // 残るセル
        var dead = cells[x, y];     // 消えるセル

        main?.SetValue(gridValues[x, y - 1]);
        main?.PlayMergeEffect();

        if (dead != null)
        {
            Destroy(dead.gameObject);
            cells[x, y] = null;
        }

        // 新しい主役セル
        activeCell = new Vector2Int(x, y - 1);
    }

    /// <summary>
    /// 合体処理(横)
    /// </summary>
    void MergeToActive(int ax, int ay, int bx, int by)
    {
        gridValues[ax, ay] *= 2;
        gridValues[bx, by] = 0;

        var main = cells[ax, ay];
        var dead = cells[bx, by];

        main?.SetValue(gridValues[ax, ay]);
        main?.PlayMergeEffect();

        if (dead != null)
        {
            Destroy(dead.gameObject);
            cells[bx, by] = null;
        }

        activeCell = new Vector2Int(ax, ay);
    }

    /// <summary>
    /// 重力処理
    /// </summary>
    void ApplyFall()
    {
        bool moved;

        // 落とせなくなるまで繰り返す
        do
        {
            moved = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height; y++)
                {
                    // 上にセルがあり、下が空いている
                    if (gridValues[x, y] != 0 && gridValues[x, y - 1] == 0)
                    {
                        // 数値移動
                        gridValues[x, y - 1] = gridValues[x, y];
                        gridValues[x, y] = 0;

                        // 見た目移動
                        var cell = cells[x, y];
                        cells[x, y - 1] = cell;
                        cells[x, y] = null;

                        cell?.MoveTo(
                            new Vector3(x * cellSize, (y - 1) * cellSize, 0),
                            fallTime
                        );

                        moved = true;
                    }
                }
            }
        }
        while (moved);
    }

    /// <summary>
    /// 次の合体可能セルを探索
    /// </summary>
    /// <returns></returns>
    Vector2Int? FindNextActiveCell()
    {
        // 左→右、下→上で探索
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int value = gridValues[x, y];
                if (value == 0) continue;

                // 縦
                if (y > 0 && gridValues[x, y - 1] == value)
                    return new Vector2Int(x, y);

                // 横
                if (x > 0 && gridValues[x - 1, y] == value)
                    return new Vector2Int(x, y);
                if (x < width - 1 && gridValues[x + 1, y] == value)
                    return new Vector2Int(x, y);
            }
        }
        return null;
    }

    /// <summary>
    /// セルを生成
    /// </summary>
    Vector2Int? SpawnCell(int x, int value)
    {
        // 下から順に空きを探す
        for (int y = 0; y < height; y++)
        {
            if (gridValues[x, y] != 0) continue;

            gridValues[x, y] = value;

            var obj = Instantiate(cellPrefab);
            obj.transform.position =
                new Vector3(x * cellSize, y * cellSize, 0);

            var cell = obj.GetComponent<CellView>();
            cell.SetValue(value);

            cells[x, y] = cell;
            return new Vector2Int(x, y);
        }
        return null;
    }
}
