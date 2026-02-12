using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム盤面の管理と操作を行うクラス
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] PlayerController playerController;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] BoardSystem boardSystem;
    [SerializeField] CellAnimator cellAnimator;
    [SerializeField] DestroyGaugeUI gaugeUI;
    [SerializeField] PauseManager pauseManager;

    [Header("セル設定")]
    [SerializeField] GameObject cellPrefab;
    [SerializeField] GameObject BGCellPrefab;
    [SerializeField] float cellSize = 1.1f;

    [Header("盤面サイズ")]
    [SerializeField] int width = 5;
    [SerializeField] int height = 5;

    [Header("カーソル")]
    [SerializeField] GameObject cursorView;

    [Header("ポーズ")]
    [SerializeField] GameObject pauseCanvas;
    private bool isPaused;

    [Header("効果音/BGM")]
    [SerializeField] AudioClip gameBGM;
    [SerializeField] AudioClip conbineSE;
    [SerializeField] AudioClip dropSE;
    [SerializeField] AudioClip destroyModeSE;
    [SerializeField] AudioClip destroySE;
    [SerializeField] AudioClip pushSE;
    //[SerializeField] AudioClip gameOverSE;

    // ===== 内部状態 =====
    private CellView[,] cells;
    private int dropWidthIndex;
    private CellView previewCell;
    private int previewValue;
    private Vector2Int cursor = Vector2Int.zero;
    private bool isDestroyMode = false;
    private bool isGameOver = false;
    private bool isResolving;
    private const float gaugeStep = 33.3f;


    // ===== 初期化 =====
    void Start()
    {
        boardSystem = new BoardSystem(width, height);
        boardSystem.OnDestroyGaugeChanged += gaugeUI.SetGauge;

        playerController.OnMove += MoveDropIndex;
        playerController.OnDrop += Drop;
        playerController.OnDestroyModeChanged += OnDestroyModeChanged;
        playerController.OnMoveCursor += MoveCursor;
        playerController.OnDestroy += TryDestroy;
        playerController.OnPause += TogglePause;

        cells = new CellView[width, height];

        AudioManager.instance.PlayBGM(gameBGM);

        CreateBackground();
        InitPreviewCell();
        UpdateCursorView();
    }

    // ===== 背景 =====
    void CreateBackground()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Instantiate(BGCellPrefab, new Vector3(x * cellSize, y * cellSize, 0), Quaternion.identity);
    }

    // ===== 予告セル =====
    void InitPreviewCell()
    {
        boardSystem.GenerateNextValue();
        previewValue = boardSystem.NextValue;
        previewCell = Instantiate(cellPrefab).GetComponent<CellView>();
        previewCell.SetValue(previewValue);
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
        if (isResolving) return;
        isResolving = true;
        DropByPlayer(dropWidthIndex);
        isResolving = false;
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

        if (!boardSystem.CanConsume(gaugeStep)) return;

        int score = boardSystem.DestroyScore(cursor.x, cursor.y);
        if (score <= 0) return;

        AudioManager.instance.PlaySE(destroySE);

        boardSystem.ConsumeGauge(gaugeStep);
        scoreManager.Add(score);

        boardSystem.DestroyCell(cursor.x, cursor.y);
        SyncViewFromBoard();
        RefreshView();
    }

    // ===== 落下 =====
    bool DropByPlayer(int x)
    {
        if (!boardSystem.SpawnAndResolve(x, previewValue))
            return false;

        AudioManager.instance.PlaySE(dropSE);

        SyncViewFromBoard();
        RefreshView();

        if (boardSystem.ChainScore > 0)
        {
            AudioManager.instance.PlaySE(conbineSE);  
            scoreManager.Add(boardSystem.ChainScore);
        }

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

    // ===== 破壊モード =====
    void OnDestroyModeChanged(bool mode)
    {
        isDestroyMode = mode;
        cursorView.SetActive(isDestroyMode);

        AudioManager.instance.PlaySE(destroyModeSE);
    }

    // ===== View同期 =====
    void CreateCellView(int x, int y, int value)
    {
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

    // ===== ゲーム状態 =====
    public void TogglePause()
    {
        isPaused = !isPaused;
        AudioManager.instance.PlaySE(pushSE);
        playerController.SetPause(isPaused);

        pauseCanvas.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused) pauseManager.ResetToPausePage();
    }


    private void CheckGameOver()
    {
        if (boardSystem.IsBoardFull())
            if (!boardSystem.CanDestroy())
                GameOver();
    }

    private void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        //audioSource.PlayOneShot(gameOverSE);

        playerController.StopControl();
        previewCell.gameObject.SetActive(false);
        cursorView.SetActive(false);

        GameData.LastScore = scoreManager.totalScore;

        SceneManager.LoadScene("ResultScene");
    }
}
