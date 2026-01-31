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
    int[,] gridValues;
    CellView[,] cells;
    Vector2Int? activeCell;

    // ===== 落下 =====
    int dropWidthIndex;

    // ===== 予告セル =====
    CellView previewCell;
    int previewValue;

    // ===== 状態 =====
    bool isGameOver;

    // ===== 破壊ゲージ =====
    [SerializeField] Slider gaugeSlider;
    [SerializeField] RectTransform tick33;
    [SerializeField] RectTransform tick66;
    [SerializeField] RectTransform fillArea;

    float destroyGauge = 0f;
    const float gaugeStep = 33.3f;

    // ===== カーソル =====
    Vector2Int cursor = Vector2Int.zero;
    [SerializeField] GameObject cursorView;
    bool isDestroyMode = false;

    // ===== スコア =====
    [SerializeField] TextMeshProUGUI scoreText;
    int totalScore;
    int chainScore;

    void Start()
    {
        // 入力
        playerController.OnMove += MoveDropIndex;
        playerController.OnDrop += Drop;

        playerController.isDestroyMode += ToggleDestroyMode;
        playerController.OnMoveCursor += MoveCursor;
        playerController.OnDestroy += TryDestroy;

        // 初期化
        gaugeSlider.maxValue = 100f;
        totalScore = 0;
        scoreText.text = totalScore.ToString("N0");

        gridValues = new int[width, height];
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
        UpdateTicksPosition();
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
        // BoardSystemにSpawn＋落下＋Mergeを任せる
        activeCell = boardSystem.SpawnAndResolve(x, previewValue);
        if (!activeCell.HasValue) return false;

        // 盤面が変化した結果をViewに反映
        RefreshView();


        if (boardSystem.ChainScore > 0)
            scoreManager.Add(boardSystem.ChainScore);

        CheckGameOver();

        // 予告セル更新
        boardSystem.GenerateNextValue();
        previewValue = boardSystem.NextValue;
        previewCell.SetValue(previewValue);
        UpdatePreviewPosition();

        return true;
    }

    // ===== 盤面処理（※ここは後で BoardSystem に置き換える）=====


    void ApplyFall()
    {
        bool moved;
        do
        {
            moved = false;
            for (int x = 0; x < width; x++)
                for (int y = 1; y < height; y++)
                {
                    if (gridValues[x, y] != 0 && gridValues[x, y - 1] == 0)
                    {
                        gridValues[x, y - 1] = gridValues[x, y];
                        gridValues[x, y] = 0;

                        cells[x, y - 1] = cells[x, y];
                        cells[x, y] = null;

                        moved = true;
                    }
                }
        } while (moved);
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

    void UpdateGaugeUI()
    {
        gaugeSlider.value = destroyGauge;
        Image fill = gaugeSlider.fillRect.GetComponent<Image>();

        if (destroyGauge < gaugeStep) fill.color = Color.gray;
        else if (destroyGauge < gaugeStep * 2) fill.color = new Color(0.5f, 1f, 0f);
        else if (destroyGauge < gaugeStep * 3) fill.color = Color.yellow;
        else fill.color = Color.orange;

        UpdateTicksPosition();
    }

    void UpdateTicksPosition()
    {
        float w = fillArea.rect.width;
        tick33.anchoredPosition = new Vector2(w * 0.333f, tick33.anchoredPosition.y);
        tick66.anchoredPosition = new Vector2(w * 0.666f, tick66.anchoredPosition.y);
    }

    // ===== 破壊 =====

    void TryDestroyBlock(int x, int y)
    {
        int score = boardSystem.DestroyScore(x, y);
        if (score <= 0 || destroyGauge < gaugeStep) return;

        destroyGauge -= gaugeStep;
        scoreManager.Add(score);

        boardSystem.DestroyCell(x, y);
        RefreshView();
    }

    void DestroyBlock(int x, int y)
    {
        gridValues[x, y] = 0;
        Destroy(cells[x, y].gameObject);
        cells[x, y] = null;

        ApplyFall();
        RefreshView();
    }

    // ===== その他 =====

    void CheckGameOver()
    {
        if (IsBoardFull())
            GameOver();
    }

    bool IsBoardFull()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (gridValues[x, y] == 0)
                    return false;
        return true;
    }

    void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("GameOver");
    }
}
