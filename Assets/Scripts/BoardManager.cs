using DG.Tweening;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // ===== 設定 =====
    [SerializeField] GameObject cellPrefab;
    [SerializeField] GameObject BGCellPrefab;
    [SerializeField] public float cellSize = 1.1f;
    //[SerializeField] float fallTime = 0.15f;
    [SerializeField] public int width = 4;
    [SerializeField] public int height = 4;
    [SerializeField] CellAnimator cellAnimator;


    // ===== 盤面データ =====
    int[,] gridValues;          // 数値だけの盤面
    CellView[,] cells;          // 見た目用セル

    Vector2Int? activeCell;     // 今合体判定しているセル
    private int dropWidthIndex; // 落とすときの縦のマス

    // ===== 予告セル =====
    CellView previewCell;
    int previewValue;

    void Start()
    {
        gridValues = new int[width, height];
        cells = new CellView[width, height];
        BackgroundCreate();

        // 予告セル生成
        previewValue = GetDropValue();
        var obj = Instantiate(cellPrefab);
        previewCell = obj.GetComponent<CellView>();
        previewCell.SetValue(previewValue);

        UpdatePreviewPosition();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            dropWidthIndex = (dropWidthIndex - 1 + width) % width;
            UpdatePreviewPosition();
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            dropWidthIndex = (dropWidthIndex + 1) % width;
            UpdatePreviewPosition();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DropByPlayer(dropWidthIndex);

            // 次の予告セル更新
            previewValue = GetDropValue();
            previewCell.SetValue(previewValue);
            UpdatePreviewPosition();
        }
    }

    private void BackgroundCreate()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector3(x * cellSize, y * cellSize, 0);
                var cell = Instantiate(BGCellPrefab, pos, Quaternion.identity);

            }
        }
    }

    /// <summary>
    /// プレイヤー操作
    /// </summary>
    /// <param name="x">落下する行</param>
    void DropByPlayer(int x)
    {
        // セル生成 → アクティブセルに設定
        activeCell = SpawnCell(x, previewValue);

        // 盤面の合体・落下を解決
        ResolveChain();

        // 見た目を最終状態に合わせる
        RefreshView();
    }

    int GetDropValue()
    {
        int r = Random.Range(1, 101);
        int sum = 0;

        int[] values = { 2, 4, 8, 16 };   // 落とす数字
        int[] rates = { 60, 25, 10, 5 }; // それぞれの確率

        for (int i = 0; i < values.Length; i++)
        {
            sum += rates[i];
            if (r <= sum)
                return values[i];
        }
        return 2;
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

            // 重力適用（ロジックのみ）
            ApplyFall();

            // 次に合体できるセルを探す
            activeCell = FindNextActiveCell();
        }
    }

    /// <summary>
    /// 合体判定
    /// </summary>
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

        // 見た目
        var main = cells[x, y - 1]; // 残る
        var dead = cells[x, y];     // 消える

        if (dead != null && main != null)
        {
            // ★ 瞬間移動防止：一瞬だけ寄せる
            dead.transform.DOKill();
            dead.transform.DOMove(main.transform.position, 0.06f);
            Destroy(dead.gameObject, 0.06f);
        }

        cells[x, y] = null;

        main?.SetValue(gridValues[x, y - 1]);
        main?.PlayMergeEffect();

        // 新しい主役
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

        if (dead != null && main != null)
        {
            // ★ 瞬間移動防止：一瞬だけ寄せる
            dead.transform.DOKill();
            dead.transform.DOMove(main.transform.position, 0.06f);
            Destroy(dead.gameObject, 0.06f);
        }

        cells[bx, by] = null;

        main?.SetValue(gridValues[ax, ay]);
        main?.PlayMergeEffect();

        activeCell = new Vector2Int(ax, ay);
    }


    /// <summary>
    /// 重力処理（ロジック専用）
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

                        // 参照移動
                        cells[x, y - 1] = cells[x, y];
                        cells[x, y] = null;

                        moved = true;
                    }
                }
            }
        }
        while (moved);
    }

    /// <summary>
    /// 見た目を盤面に同期
    /// </summary>
    void RefreshView()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cells[x, y] == null) continue;

                cellAnimator.MoveCell(cells[x, y], x, y);
            }
        }
    }


    /// <summary>
    /// 次の合体可能セルを探索
    /// </summary>
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

    // ===== 予告セル位置更新 =====
    void UpdatePreviewPosition()
    {
        previewCell.transform.position =
            new Vector3(
                dropWidthIndex * cellSize,
                height * cellSize + 0.5f,
                0
            );
    }
}
