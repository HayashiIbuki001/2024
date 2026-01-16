using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] GameObject cellPrefab;
    [SerializeField] float cellSize = 1.1f;
    [SerializeField] float fallDuration = 0.15f;
    [SerializeField] int width = 4;
    [SerializeField] int height = 4;

    int[,] board;
    CellView[,] cellMap;

    Vector2Int? hero;

    void Start()
    {
        board = new int[width, height];
        cellMap = new CellView[width, height];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayerDrop(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayerDrop(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayerDrop(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayerDrop(3);
    }

    // =====================
    // プレイヤー操作
    // =====================

    void PlayerDrop(int x)
    {
        hero = DropNumber(x, 2);
        ResolveBoard();
    }

    // =====================
    // 盤面解決ループ
    // =====================

    void ResolveBoard()
    {
        while (hero.HasValue)
        {
            // 主役連鎖フェーズ
            while (TryHeroMerge()) { }

            // 重力フェーズ
            ApplyGravity();

            // 重力後に次の主役を探す
            hero = FindNextHeroAfterGravity();
        }
    }

    // =====================
    // 主役合体判定
    // =====================

    bool TryHeroMerge()
    {
        var h = hero.Value;
        int x = h.x;
        int y = h.y;
        int v = board[x, y];

        // 縦合体（最優先・必ず下）
        if (y > 0 && board[x, y - 1] == v)
        {
            MergeDown(x, y);
            return true;
        }

        // 横合体（主役優先・左→右）
        if (x > 0 && board[x - 1, y] == v)
        {
            MergeHorizontalHero(x, y, x - 1, y);
            return true;
        }
        if (x < width - 1 && board[x + 1, y] == v)
        {
            MergeHorizontalHero(x, y, x + 1, y);
            return true;
        }

        return false;
    }

    // =====================
    // 合体処理
    // =====================

    // 縦合体：必ず下に合体
    void MergeDown(int x, int y)
    {
        board[x, y - 1] *= 2;
        board[x, y] = 0;

        var main = cellMap[x, y - 1];
        var dead = cellMap[x, y];

        if (main != null)
        {
            main.SetValue(board[x, y - 1]);
            main.PlayMergeEffect();
        }

        if (dead != null)
        {
            Destroy(dead.gameObject);
            cellMap[x, y] = null;
        }

        hero = new Vector2Int(x, y - 1);
    }

    // 横合体：主役セルに合体
    void MergeHorizontalHero(int hx, int hy, int dx, int dy)
    {
        board[hx, hy] *= 2;
        board[dx, dy] = 0;

        var main = cellMap[hx, hy];
        var dead = cellMap[dx, dy];

        if (main != null)
        {
            main.SetValue(board[hx, hy]);
            main.PlayMergeEffect();
        }

        if (dead != null)
        {
            Destroy(dead.gameObject);
            cellMap[dx, dy] = null;
        }

        hero = new Vector2Int(hx, hy);
    }

    // =====================
    // 重力処理
    // =====================

    void ApplyGravity()
    {
        bool moved;
        do
        {
            moved = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height; y++)
                {
                    if (board[x, y] != 0 && board[x, y - 1] == 0)
                    {
                        board[x, y - 1] = board[x, y];
                        board[x, y] = 0;

                        var cell = cellMap[x, y];
                        cellMap[x, y - 1] = cell;
                        cellMap[x, y] = null;

                        cell?.MoveTo(
                            new Vector3(x * cellSize, (y - 1) * cellSize, 0),
                            fallDuration
                        );

                        moved = true;
                    }
                }
            }
        }
        while (moved);
    }

    // =====================
    // 重力後の主役探索
    // =====================

    Vector2Int? FindNextHeroAfterGravity()
    {
        // 左→右、下→上
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int v = board[x, y];
                if (v == 0) continue;

                // 縦
                if (y > 0 && board[x, y - 1] == v)
                    return new Vector2Int(x, y);

                // 横
                if (x > 0 && board[x - 1, y] == v)
                    return new Vector2Int(x, y);
                if (x < width - 1 && board[x + 1, y] == v)
                    return new Vector2Int(x, y);
            }
        }
        return null;
    }

    // =====================
    // セル生成
    // =====================

    Vector2Int? DropNumber(int x, int value)
    {
        for (int y = 0; y < height; y++)
        {
            if (board[x, y] == 0)
            {
                board[x, y] = value;

                var obj = Instantiate(cellPrefab);
                obj.transform.position = new Vector3(x * cellSize, y * cellSize, 0);
                var cell = obj.GetComponent<CellView>();
                cell.SetValue(value);

                cellMap[x, y] = cell;
                return new Vector2Int(x, y);
            }
        }
        return null;
    }
}
