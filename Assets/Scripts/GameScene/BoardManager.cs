using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardManager : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] BoardSystem boardSystem;

    // ===== 設定 =====
    [SerializeField] GameObject cellPrefab;
    [SerializeField] GameObject BGCellPrefab;
    [SerializeField] float cellSize = 1.1f;

    [SerializeField] int width = 4;
    [SerializeField] int height = 4;

    [SerializeField] CellAnimator cellAnimator;

    // ===== 盤面（※最終的に BoardSystem に移行予定）=====
    CellView[,] cells;

    // ===== 落下 =====
    int dropWidthIndex;

    // ===== 予告セル =====
    CellView previewCell;
    int previewValue;

    // ===== 状態 =====
    bool isGameOver;

    float destroyGauge = 0f;
    const float gaugeStep = 33.3f;

    // ===== カーソル =====
    Vector2Int cursor = Vector2Int.zero;
    [SerializeField] GameObject cursorView;
    bool isDestroyMode = false;

    void Start()
    {
        boardSystem = new BoardSystem(width, height);

        // 入力
        playerController.OnMove += MoveDropIndex;
        playerController.OnDrop += Drop;

        playerController.isDestroyMode += ToggleDestroyMode;
        playerController.OnMoveCursor += MoveCursor;
        playerController.OnDestroy += TryDestroy;

        cells = new CellView[width, height];

        CreateBackground();

        // 予告セル
        boardSystem.GenerateNextValue();
        previewValue = boardSystem.NextValue;
        previewCell = Instantiate(cellPrefab).GetComponent<CellView>();
        previewCell.SetValue(previewValue);

        cursor = Vector2Int.zero;
        UpdateCursorView();
        UpdatePreviewPosition();
    }

    // ===== 入力 =====

    void MoveDropIndex(int dir)
    {
        dropWidthIndex = (dropWidthIndex + dir + width) % width;
        UpdatePreviewPosition();
    }

    void Drop()
    {
        DropByPlayer(dropWidthIndex);
    }

    void ToggleDestroyMode()
    {
        isDestroyMode = !isDestroyMode;
        cursorView.SetActive(isDestroyMode);
    }

    void MoveCursor(Vector2Int dir)
    {
        if (!isDestroyMode) return;

        cursor += dir;
        cursor.x = Mathf.Clamp(cursor.x, 0, width - 1);
        cursor.y = Mathf.Clamp(cursor.y, 0, height - 1);

        UpdateCursorView();
    }

    void TryDestroy()
    {
        if (!isDestroyMode) return;
        TryDestroyBlock(cursor.x, cursor.y);
    }

    // ===== 生成 =====

    void CreateBackground()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Instantiate(BGCellPrefab, new Vector3(x * cellSize, y * cellSize, 0), Quaternion.identity);
    }

    // ===== 落下 =====

    bool DropByPlayer(int x)
    {
        if (!boardSystem.SpawnAndResolve(x, previewValue))
            return false;

        SyncViewFromBoard();
        RefreshView();

        if (boardSystem.ChainScore > 0)
            scoreManager.Add(boardSystem.ChainScore);

        boardSystem.GenerateNextValue();
        previewValue = boardSystem.NextValue;
        previewCell.SetValue(previewValue);
        UpdatePreviewPosition();

        CheckGameOver();
        return true;
    }





    void RefreshView()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (cells[x, y] != null)
                    cellAnimator.MoveCell(cells[x, y], x, y);
    }

    // ===== UI =====

    void UpdatePreviewPosition()
    {
        previewCell.transform.position =
            new Vector3(dropWidthIndex * cellSize, height * cellSize + 0.5f, 0);
    }

    void UpdateCursorView()
    {
        if (cursorView == null) return;
        cursorView.transform.position =
            new Vector3(cursor.x * cellSize, cursor.y * cellSize, -1);
    }

    // ===== 破壊 =====

    void TryDestroyBlock(int x, int y)
    {
        int score = boardSystem.DestroyScore(x, y);
        if (score <= 0 || destroyGauge < gaugeStep) return;

        destroyGauge -= gaugeStep;
        scoreManager.Add(score);

        boardSystem.DestroyCell(x, y);
        SyncViewFromBoard();
        RefreshView();
    }

    void CreateCellView(int x, int y, int value)
    {
        Debug.Log($"CreateCellView {x},{y} v={value}");

        var cell = Instantiate(cellPrefab).GetComponent<CellView>();
        cell.SetValue(value);
        cell.transform.position = new Vector3(x * cellSize, y * cellSize, 0);
        cells[x, y] = cell;
    }

    void SyncViewFromBoard()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int v = boardSystem.GetValue(x, y);

                if (v == 0 && cells[x, y] != null)
                {
                    Destroy(cells[x, y].gameObject);
                    cells[x, y] = null;
                }
                else if (v != 0)
                {
                    if (cells[x, y] == null)
                        CreateCellView(x, y, v);
                    else
                        cells[x, y].SetValue(v);
                }
            }
    }


    // ===== その他 =====

    void CheckGameOver()
    {
        if (boardSystem.IsBoardFull())
            GameOver();
    }

    void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("GameOver");
    }
}
