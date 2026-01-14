using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private int width = 4; // 横
    [SerializeField] private int height = 4; // 縦

    private int[,] board;

    void Start()
    {
        SetBoard();
        DropNumber(1, 2);
        DropNumber(1, 4);
        ApplyGravity();
        PrintBoard();
    }

    /// <summary>
    /// 初期配置のため0を全マスに入れる
    /// </summary>
    private void SetBoard()
    {
        board = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                board[x, y] = 0;
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
        for (int y = 0; y < 4; y++)
        {
            if (board[x, y] == 0)
            {
                board[x, y] = value;
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
            for (int x = 0; x < height; x++)
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

                        moved = true;
                    }
            }
        } while (moved);
    }
}
