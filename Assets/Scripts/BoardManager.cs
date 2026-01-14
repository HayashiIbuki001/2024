using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float cellSize = 1.1f;

    private CellView[,] cells;

    [SerializeField] private int width = 4; // 横
    [SerializeField] private int height = 4; // 縦

    private int[,] board;
    private bool[,] isNew;
    

    void Start()
    {
        SetBoard();
        CreateGrid();

        // 盤面下段を作る
        DropNumber(0, 2); // 左
        DropNumber(2, 2); // 右
        ClearIsNew();

        // 真ん中に主役を落とす
        DropNumber(1, 2);

        ApplyMerge();
        ClearIsNew();
        PrintBoard();
        UpdateView();
    }

    /// <summary>
    /// 初期配置のため0を全マスに入れる
    /// </summary>
    private void SetBoard()
    {
        board = new int[width, height];
        isNew = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                board[x, y] = 0;
                isNew[x, y] = false;
            }
        }
    }

    /// <summary>
    /// 盤面を生成する
    /// </summary>
    private void CreateGrid()
    {
        cells = new CellView[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject obj = Instantiate(cellPrefab);
                obj.transform.position = new Vector3(x * cellSize, y * cellSize, 0);


                CellView cell = obj.GetComponent<CellView>();
                cell.x = x;
                cell.y = y;

                cells[x,y] = cell;
            }
        }
    }

    /// <summary>
    /// 数字を落とすメソッド
    /// </summary>
    /// <param name="x">マスの行</param>
    /// <param name="value">入れる数字</param>
    void DropNumber(int x, int value)
    {
        for (int y = 0; y < height; y++)
        {
            if (board[x, y] == 0)
            {
                board[x, y] = value;
                isNew[x, y] = true;
                return;
            }
        }

        Debug.Log("ここには設置できません。");
    }

    /// <summary>
    /// 現在のボードをLogに出力
    /// </summary>
    void PrintBoard()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            string line = "";
            for (int x = 0; x < width; x++)
            {
                line += board[x, y] + " ";
            }
            Debug.Log(line);
        }
    }

    /// <summary>
    /// 重力を計算する
    /// </summary>
    void ApplyGravity()
    {
        bool moved;

        do
        {
            moved = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height; y++)
                    if (board[x, y] != 0 && board[x, y - 1] == 0)
                    {
                        board[x, y - 1] = board[x, y];
                        board[x, y] = 0;

                        isNew[x, y - 1] = isNew[x, y];
                        isNew[x, y] = false;
                        moved = true;
                    }
            }

        } while (moved); // 処理が止まるまで動かす
        UpdateView();
    }

    /// <summary>
    /// 縦横の合体処理
    /// </summary>
    void ApplyMerge()
    {
        bool merged;

        do
        {
            merged = false;

            if (ApplyMergeVertical())
            {
                merged = true;
                continue;
            }

            if (ApplyMergeHorizontal())
                merged = true;
        } while (merged); // 処理が止まるまで回す
    }

    /// <summary>
    /// 縦行の合体処理(最優先)
    /// </summary>
    /// <returns>合体したかどうかのフラグ</returns>
    bool ApplyMergeVertical()
    {
        bool merged = false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                if (!isNew[x, y]) continue;
                if (board[x, y] != 0 && board[x, y] == board[x, y - 1])
                {
                    board[x, y - 1] *= 2; // 最新で更新されたマスを倍に
                    board[x, y] = 0; // 合体されたマスを0に

                    isNew[x, y - 1] = true;
                    merged = true;
                    ApplyGravity();
                }
            }
        }
        return merged;
    }

    /// <summary>
    /// 横列の合体処理
    /// </summary>
    /// <returns>合体したかどうかのフラグ</returns>
    bool ApplyMergeHorizontal()
    {
        bool merged = false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!isNew[x, y]) continue;
                if (board[x, y] != 0 && x > 0 && board[x, y] == board[x - 1, y])
                {
                    board[x, y] *= 2; // 最新で更新されたマスを倍に
                    board[x - 1, y] = 0; // 合体されたマスを0に

                    isNew[x, y] = true;
                    isNew[x - 1, y] = false;
                    merged = true;
                    ApplyGravity();
                    return merged;
                }

                if (isNew[x, y] && x < width - 1 && board[x, y] == board[x + 1, y])
                {
                    board[x, y] *= 2; // 最新で更新されたマスを倍に
                    board[x + 1, y] = 0; // 合体されたマスを0に

                    isNew[x,y] = true;
                    isNew[x + 1, y] = false;
                    merged = true;
                    ApplyGravity();
                    return merged;
                }
            }
        }
        return merged;
    }

    /// <summary>
    /// 全てのマスのisNewをfalseにリセット
    /// </summary>
    void ClearIsNew()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                isNew[x, y] = false;
    }

    /// <summary>
    /// UI更新
    /// </summary>
    private void UpdateView()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cells[x, y].SetValue(board[x, y]);
    }
}
