using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] GameObject cellPrefab;
    [SerializeField] float cellSize = 1.1f;
    [SerializeField] int width = 4;
    [SerializeField] int height = 4;

    int[,] board;
    bool[,] moved;
    CellView[,] cells;

    void Start()
    {
        board = new int[width, height];
        moved = new bool[width, height];
        cells = new CellView[width, height];

        CreateGrid();

        // テスト初期配置
        DropNumber(2, 2);
        DropNumber(1, 2);

        UpdateView();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayerDrop(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PlayerDrop(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) PlayerDrop(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) PlayerDrop(3);
    }

    void PlayerDrop(int x)
    {
        DropNumber(x, 2);
        ResolveBoard();
        UpdateView();
    }

    void ResolveBoard()
    {
        while (true)
        {
            ClearMoved();
            ApplyGravity();

            if (MergeVerticalOnce()) continue;
            if (MergeHorizontalOnce()) continue;

            break;
        }
    }

    void ClearMoved()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                moved[x, y] = false;
    }

    void DropNumber(int x, int value)
    {
        for (int y = 0; y < height; y++)
        {
            if (board[x, y] == 0)
            {
                board[x, y] = value;
                moved[x, y] = true;
                return;
            }
        }
    }

    void ApplyGravity()
    {
        bool movedOnce;
        do
        {
            movedOnce = false;
            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height; y++)
                {
                    if (board[x, y] != 0 && board[x, y - 1] == 0)
                    {
                        board[x, y - 1] = board[x, y];
                        board[x, y] = 0;

                        moved[x, y - 1] = true;
                        moved[x, y] = false;

                        movedOnce = true;
                    }
                }
            }
        } while (movedOnce);
    }

    bool MergeVerticalOnce()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                if (board[x, y] != 0 && board[x, y] == board[x, y - 1])
                {
                    board[x, y] *= 2;
                    board[x, y - 1] = 0;

                    moved[x, y] = true;
                    moved[x, y - 1] = false;

                    return true;
                }
            }
        }
        return false;
    }

    bool MergeHorizontalOnce()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                if (board[x, y] == 0) continue;
                if (board[x, y] != board[x + 1, y]) continue;

                bool leftMoved = moved[x, y];
                bool rightMoved = moved[x + 1, y];

                if (leftMoved && !rightMoved)
                {
                    board[x, y] *= 2;
                    board[x + 1, y] = 0;

                    moved[x, y] = true;
                    moved[x + 1, y] = false;
                }
                else
                {
                    board[x + 1, y] *= 2;
                    board[x, y] = 0;

                    moved[x + 1, y] = true;
                    moved[x, y] = false;
                }

                return true;
            }
        }
        return false;
    }

    void CreateGrid()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var obj = Instantiate(cellPrefab);
                obj.transform.position = new Vector3(x * cellSize, y * cellSize, 0);
                cells[x, y] = obj.GetComponent<CellView>();
            }
        }
    }

    void UpdateView()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cells[x, y].SetValue(board[x, y]);
    }
}
