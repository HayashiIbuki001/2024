using System.Collections;
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
    [SerializeField] Transform boardRoot;
    [SerializeField] Transform mainCamera;

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

    [Header("エフェクト")]
    [SerializeField] GameObject destroyModeEffect;
    [SerializeField] GameObject margeEffect;
    [SerializeField] float effectFadeTime = 0.3f;
    [SerializeField] RectTransform gameCanvas;
    [SerializeField] float shakeTime = 0.15f;
    [SerializeField] float shakePower = 15f;

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

    private Coroutine margeRoutine;
    private Coroutine shakeRoutine;

    // ===== 初期化 =====
    private void Start()
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

        margeEffect.SetActive(false);
        destroyModeEffect.SetActive(false);
    }

    // ===== 背景 =====
    private void CreateBackground()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Instantiate(BGCellPrefab, new Vector3(x * cellSize, y * cellSize, 0), Quaternion.identity, boardRoot);
    }

    // ===== 予告セル =====
    private void InitPreviewCell()
    {
        boardSystem.GenerateNextValue();
        previewValue = boardSystem.NextValue;
        previewCell = Instantiate(cellPrefab).GetComponent<CellView>();
        previewCell.SetValue(previewValue);
        UpdatePreviewPosition();
    }

    // ===== 入力 =====
    private void MoveDropIndex(int dir)
    {
        dropWidthIndex = (dropWidthIndex + dir + width) % width;
        UpdatePreviewPosition();
    }

    private void Drop()
    {
        if (isResolving) return;
        isResolving = true;
        DropByPlayer(dropWidthIndex);
        isResolving = false;
    }

    private void MoveCursor(Vector2Int dir)
    {
        if (!isDestroyMode) return;
        cursor += dir;
        cursor.x = Mathf.Clamp(cursor.x, 0, width - 1);
        cursor.y = Mathf.Clamp(cursor.y, 0, height - 1);
        UpdateCursorView();
    }

    private void TryDestroy()
    {
        if (!isDestroyMode) return;

        if (!boardSystem.CanConsume(gaugeStep)) return;

        int score = boardSystem.DestroyScore(cursor.x, cursor.y);
        if (score <= 0) return;

        AudioManager.instance.PlaySE(destroySE);
        PlayShake();

        boardSystem.ConsumeGauge(gaugeStep);
        scoreManager.Add(score);

        boardSystem.DestroyCell(cursor.x, cursor.y);
        SyncViewFromBoard();
        RefreshView();
    }

    // ===== 落下 =====
    private bool DropByPlayer(int x)
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

            PlayMargeEffect();
        }

        boardSystem.GenerateNextValue();
        previewValue = boardSystem.NextValue;
        previewCell.SetValue(previewValue);
        UpdatePreviewPosition();

        CheckGameOver();
        return true;
    }

    private void RefreshView()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (cells[x, y] != null)
                    cellAnimator.MoveCell(cells[x, y], x, y);
    }

    // ===== UI =====
    private void UpdatePreviewPosition()
    {
        previewCell.transform.position =
            new Vector3(dropWidthIndex * cellSize, height * cellSize + 0.5f, 0);
    }

    private void UpdateCursorView()
    {
        if (cursorView == null) return;
        cursorView.transform.position =
            new Vector3(cursor.x * cellSize, cursor.y * cellSize, -1);
    }

    // ===== 破壊モード =====
    private void OnDestroyModeChanged(bool mode)
    {
        isDestroyMode = mode;

        cursorView.SetActive(isDestroyMode);
        destroyModeEffect.SetActive(isDestroyMode);

        AudioManager.instance.PlaySE(destroyModeSE);
    }

    // ===== View同期 =====
    private void CreateCellView(int x, int y, int value)
    {
        var cell = Instantiate(cellPrefab,new Vector3(x * cellSize, y * cellSize, 0), Quaternion.identity, boardRoot).GetComponent<CellView>();
        cell.SetValue(value);
        cells[x, y] = cell;
    }

    private void SyncViewFromBoard()
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

    // ===== エフェクト =====
    private void PlayMargeEffect()
    {
        if (margeRoutine != null)
        {
            StopCoroutine(margeRoutine);
        }

        margeRoutine = StartCoroutine(CoMargeEffect());
    }

    private IEnumerator CoMargeEffect()
    {
        margeEffect.SetActive(true);

        CanvasGroup cg = margeEffect.GetComponent<CanvasGroup>();

        cg.alpha = 1.0f;

        float t = 0.0f;
        while (t < effectFadeTime)
        {
            t += Time.deltaTime;
            cg.alpha = 1f - (t / effectFadeTime);
            yield return null;
        }

        cg.alpha = 0f;
        margeEffect.SetActive(false);
        margeRoutine = null;
    }

    private void PlayShake()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine); 
        }
        shakeRoutine = StartCoroutine(CoShake());
    }

    private IEnumerator CoShake()
    {
        Vector3 origin = mainCamera.transform.localPosition;
        float t = 0f;

        while (t < shakeTime)
        {
            t += Time.deltaTime;

            float damper = 1f - (t / shakeTime);

            float x = Mathf.Sin(t * 40f) * shakePower * damper;
            float y = Mathf.Cos(t * 35f) * shakePower * 0.6f * damper;

            mainCamera.transform.localPosition = origin + new Vector3(x, y, 0);

            yield return null;
        }

        mainCamera.transform.localPosition = origin;
        shakeRoutine = null;
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
