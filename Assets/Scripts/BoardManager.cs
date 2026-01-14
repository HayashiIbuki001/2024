using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private int width = 4; // â°
    [SerializeField] private int height = 4; // èc

    private int[,] board;

    void Start()
    {
        board = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                board[x, y] = 0;
            }
        }

        DropNumber(1, 2);
        DropNumber(1, 4);
        ApplyGravity();
        PrintBoard();
    }

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

        Debug.Log("Ç±Ç±Ç…ÇÕê›íuÇ≈Ç´Ç‹ÇπÇÒÅB");
    }

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

    void ApplyGravity()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++)
                if (board[x, y] != 0 && board[x, y - 1] == 0)
                {
                    board[x, y - 1] = board[x, y];
                    board[x, y] = 0;
                }
        }
    }
}
