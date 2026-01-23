using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    // ===== 設定 =====
    [SerializeField] GameObject cellPrefab;
    [SerializeField] GameObject BGCellPrefab;
    [SerializeField] public float cellSize = 1.1f;
    
    [SerializeField] public int width = 4;
    [SerializeField] public int height = 4;
    [SerializeField] CellAnimator cellAnimator;

    [SerializeField] private RectTransform tick33;
    [SerializeField] private RectTransform tick66;
    [SerializeField] private RectTransform fillArea;

    [SerializeField] private GameObject scorePopUpPrefab;
    [SerializeField] private RectTransform scorePopUpRoot;


    // ===== 盤面データ =====
    int[,] gridValues;          // 数値だけの盤面
    CellView[,] cells;          // 見た目用セル

    Vector2Int? activeCell;     // 今合体判定しているセル
    private int dropWidthIndex; // 落とすときの縦のマス

    // ===== 予告セル =====
    CellView previewCell;
    int previewValue;

    private bool isGameOver;

    // ===== ゲージ =====
    private float destroyGauge = 0f;
    private const float gaugeStep = 33.3f;
    [SerializeField] private Slider gaugeSlider;

    Vector2Int cursor = Vector2Int.zero;
    [SerializeField] GameObject cursorView;

    private bool isDestroyMode = false;

    [SerializeField] public TextMeshProUGUI scoreText;
    private int totalScore = 0;
    private int chainScore = 0;
    Queue<string> scoreQueue = new Queue<string>();
    bool isScorePopupPlaying;

    void Start()
    {
        gaugeSlider.maxValue = 100f;
        totalScore = 0;
        scoreText.text = totalScore.ToString("N0");

        gridValues = new int[width, height];
        cells = new CellView[width, height];
        BackgroundCreate();

        // 予告セル生成
        previewValue = GetDropValue();
        var obj = Instantiate(cellPrefab);
        previewCell = obj.GetComponent<CellView>();
        previewCell.SetValue(previewValue);

        cursor = new Vector2Int(0, 0);
        UpdateCursorView();
        UpdatePreviewPosition();
        UpdateTicksPosition();
    }

    void Update()
    {
        if (!isDestroyMode)
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
                if (DropByPlayer(dropWidthIndex))
                {

                    // 次の予告セル更新
                    previewValue = GetDropValue();
                    previewCell.SetValue(previewValue);
                    UpdatePreviewPosition();
                }
            }
        }

        if (isDestroyMode)
        {
            if (Input.GetKeyDown(KeyCode.A)) cursor.x--;
            if (Input.GetKeyDown(KeyCode.D)) cursor.x++;
            if (Input.GetKeyDown(KeyCode.S)) cursor.y--;
            if (Input.GetKeyDown(KeyCode.W)) cursor.y++;

            cursor.x = Mathf.Clamp(cursor.x, 0, width - 1);
            cursor.y = Mathf.Clamp(cursor.y, 0, height - 1);

            UpdateCursorView();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryDestroyBlock(cursor.x, cursor.y);
                UpdateGaugeUI();
            }
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
    private bool DropByPlayer(int x)
    {
        // セル生成 → アクティブセルに設定
        activeCell = SpawnCell(x, previewValue);
        if (!activeCell.HasValue) return false;

        // 盤面の合体・落下を解決
        ResolveChain();

        // 見た目を最終状態に合わせる
        RefreshView();

        CheckGameOver();

        return true;
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
        chainScore = 0;

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

        if (chainScore > 0)
        {
            AddScore(chainScore);
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

        chainScore += gridValues[x, y - 1];

        OnCellMerged();
        UpdateGaugeUI();

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
            dead.transform.DOKill();
            dead.transform.DOMove(main.transform.position, 0.06f);
            Destroy(dead.gameObject, 0.06f);
        }

        cells[bx, by] = null;

        main?.SetValue(gridValues[ax, ay]);
        main?.PlayMergeEffect();

        chainScore += gridValues[ax, ay];

        OnCellMerged();
        UpdateGaugeUI();

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

            UpdateGaugeUI();
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

    private void AddGauge(float amount)
    {
        destroyGauge = Mathf.Min(destroyGauge + amount, 100f);
    }

    public void TryDestroyBlock(int x, int y)
    {
        int value = gridValues[x, y];
        if (value == 0) return;

        int n = (int)Mathf.Log(value, 2); // lvl計算
        if (n < 5) return;

        if (destroyGauge < gaugeStep) return;

        destroyGauge -= gaugeStep;

        int total = value * n * n;

        // ★ 二行表示
        AddScore(
            total,
            $"{value:N0} × Lv{n}²\n+{total:N0}"
        );

        DestroyBlock(x, y);
        UpdateGaugeUI();
    }


    private void DestroyBlock(int x, int y)
    {

        gridValues[x, y] = 0;
        if (cells[x, y] != null)
        {
            Destroy(cells[x, y].gameObject);
            cells[x, y] = null;
        }

        ApplyFall(); // 落下処理
        activeCell = FindNextActiveCell();
        ResolveChain(); // 合体処理
        RefreshView(); // 見た目
    }

    public void OnClickDestroyButton()
    {
        isDestroyMode = !isDestroyMode;
        Debug.Log($"破壊モード：{isDestroyMode}");

        if (isDestroyMode)
        {
            cursorView.SetActive(true);
        }
        else
        {
            cursorView.SetActive(false);
        }
    }

    private void UpdateCursorView()
    {
        if (cursorView == null) return;

        cursorView.transform.position = new Vector3(cursor.x * cellSize, cursor.y * cellSize, -1);
    }

    private void UpdateGaugeUI()
    {
        if (gaugeSlider != null)
        {
            gaugeSlider.value = destroyGauge;

            Image fill = gaugeSlider.fillRect.GetComponent<Image>();

            Color brightYellowGreen = new Color(0.5f, 1f, 0f);

            if (destroyGauge < gaugeStep)
                fill.color = Color.gray; // 破壊不可
            else if (destroyGauge < gaugeStep * 2)
                fill.color = brightYellowGreen; // 破壊1回可能
            else if (destroyGauge < gaugeStep * 3)
                fill.color = Color.yellow; // 破壊2回可能
            else
                fill.color = Color.orange; // 99.9f ~ max
        }

        UpdateTicksPosition();
    }

    private void UpdateTicksPosition()
    {
        float width = fillArea.rect.width;

        // 33% の位置
        tick33.anchoredPosition = new Vector2(width * 0.333f, tick33.anchoredPosition.y);

        // 66% の位置
        tick66.anchoredPosition = new Vector2(width * 0.666f, tick66.anchoredPosition.y);
    }

    void OnCellMerged()
    {
        if (!activeCell.HasValue) return;
        
        int value = gridValues[activeCell.Value.x, activeCell.Value.y];
        AddGaugeByLevel(value);
    }

    private void AddGaugeByLevel(int value)
    {
        int lv = (int)Mathf.Log(value, 2);
        float increase = (lv * lv) / 4f;

        destroyGauge = Mathf.Min(destroyGauge + increase, 100f);
        UpdateGaugeUI();
    }

    private void AddScore(int value, string popupMessage = null)
    {
        if (value <= 0) return;

        const int maxScore = 1_000_000_000;
        totalScore = Mathf.Min(totalScore + value, maxScore);
        scoreText.text = totalScore.ToString("N0");

        // 表示文字列が無ければ通常表示
        if (string.IsNullOrEmpty(popupMessage))
            popupMessage = $"+{value:N0}";

        scoreQueue.Enqueue(popupMessage);

        if (!isScorePopupPlaying)
        {
            PlayNextPupup();
        }
    }


    private void PlayNextPupup()
    {
        if (scoreQueue.Count == 0)
        {
            isScorePopupPlaying = false;
            return;
        }

        isScorePopupPlaying = true;

        string message = scoreQueue.Dequeue();
        ShowScorePopup(message, PlayNextPupup);
    }


    private void ShowScorePopup(string message, System.Action onComplete)
    {
        var obj = Instantiate(scorePopUpPrefab, scorePopUpRoot);
        var text = obj.GetComponent<TextMeshProUGUI>();
        var rt = obj.GetComponent<RectTransform>();

        text.text = message;
        text.alpha = 1f;

        var scoreRT = scoreText.rectTransform;

        // スコアの少し下に固定
        rt.anchoredPosition =
            scoreRT.anchoredPosition +
            new Vector2(0, -scoreRT.rect.height);

        // 右へスライド
        rt.DOAnchorPosX(rt.anchoredPosition.x + 80f, 0.6f);

        text.DOFade(0f, 0.6f).OnComplete(() =>
        {
            Destroy(obj);
            onComplete?.Invoke();
        });
    }






    private bool IsBoardFull()
    {

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridValues[x, y] == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void CheckGameOver()
    {
        if (IsBoardFull() && FindNextActiveCell() == null)
        {
            GameOver();
        }
    }
    private void GameOver()
    {
        if (isGameOver) return;

        // ゲームオーバー処理
        isGameOver = true;
        Debug.Log("ゲームオーバー");
    }
}
