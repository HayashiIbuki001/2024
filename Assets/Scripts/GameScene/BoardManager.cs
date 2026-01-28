using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] BoardView view;
    [SerializeField] float cellSize = 1.1f;
    [SerializeField] int width = 4;
    [SerializeField] int height = 4;

    [Header("破壊モード")]
    [SerializeField] GameObject cursorView;
    [SerializeField] UnityEngine.UI.Slider gaugeSlider;
    [SerializeField] private RectTransform fillArea;   // ゲージ本体の RectTransform
    [SerializeField] private RectTransform tick33;
    [SerializeField] private RectTransform tick66;
    public float destroyGauge = 0f;
    public float gaugeStep = 33.3f;
    Vector2Int cursor = Vector2Int.zero;
    bool isDestroyMode = false;
    bool isDropping = false;

    BoardLogic logic;
    int dropX;

    DropValueProvider dropper = new DropValueProvider();

    private void Start()
    {
        if (gaugeSlider != null)
        {
            gaugeSlider.maxValue = 100f;
            gaugeSlider.value = 0;
        }

        view.Init(width, height);
        view.CreateBackground();
        logic = new BoardLogic(width, height);

        scoreManager.Init();
        cursorView.SetActive(false);

        // ★目盛りを初期配置
        UpdateTicksPosition();
    }


    private void Update()
    {
        if (isDestroyMode)
        {
            UpdateDestroyInput();
            UpdateCursorView();
        }
        else
        {
            UpdateDropInput();
        }
    }

    private void UpdateDropInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
            dropX = (dropX - 1 + width) % width;
        if (Input.GetKeyDown(KeyCode.D))
            dropX = (dropX + 1) % width;
        if (Input.GetKeyDown(KeyCode.Space))
            Drop();
    }

    private void UpdateDestroyInput()
    {
        if (Input.GetKeyDown(KeyCode.A)) cursor.x = Mathf.Max(0, cursor.x - 1);
        if (Input.GetKeyDown(KeyCode.D)) cursor.x = Mathf.Min(width - 1, cursor.x + 1);
        if (Input.GetKeyDown(KeyCode.W)) cursor.y = Mathf.Min(height - 1, cursor.y + 1);
        if (Input.GetKeyDown(KeyCode.S)) cursor.y = Mathf.Max(0, cursor.y - 1);

        if (Input.GetKeyDown(KeyCode.Space))
            DestroyCell(cursor.x, cursor.y);
    }

    public void OnClickDestroyButton()
    {
        isDestroyMode = !isDestroyMode;
        cursorView.SetActive(isDestroyMode);
    }

    private void UpdateCursorView()
    {
        if (cursorView == null) return;

        // 盤面のセルとは独立して、整数座標から絶対位置を計算
        cursorView.transform.position = new Vector3(cursor.x * cellSize, cursor.y * cellSize, -1);
    }


    private void Drop()
    {
        if (isDropping) return;  // 連打防止

        int value = dropper.Get();
        var pos = logic.SpawnCell(dropX, value);
        if (!pos.HasValue) return;

        view.CreateCell(pos.Value.x, pos.Value.y, value);

        isDropping = true;
        int chainScore = logic.ResolveChain();
        scoreManager.AddScore(chainScore);

        Invoke(nameof(FinishDrop), 0.12f);
    }

    private void FinishDrop()
    {
        RefreshView();
        isDropping = false;
    }


    private void RefreshView()
    {
        view.Refresh(logic.gridValues);

        var last = logic.GetLastMergedCell();
        if (last.HasValue)
            view.PlayMergeEffect(last.Value);
    }

    private void AddGaugeByValue(int value)
    {
        int lv = (int)Mathf.Log(value, 2);
        float increase = (lv * lv) / 4f;
        destroyGauge = Mathf.Min(destroyGauge + increase, 100f);

        UpdateGaugeUI();
    }

    private void UpdateGaugeUI()
    {
        if (gaugeSlider == null) return;

        gaugeSlider.value = destroyGauge;
        var fill = gaugeSlider.fillRect.GetComponent<UnityEngine.UI.Image>();

        if (destroyGauge < 33.3f) fill.color = Color.gray;                // 破壊不可
        else if (destroyGauge < 66.6f) fill.color = new Color(0.5f, 1f, 0f); // 破壊1回可能
        else if (destroyGauge < 100f) fill.color = Color.yellow;           // 破壊2回可能
        else fill.color = Color.orange;                                    // 破壊3回可能
    }


    private void DestroyCell(int x, int y)
    {
        int value = logic.gridValues[x, y];
        if (value == 2 || value == 4 || value == 8 || value == 16) return; // 破壊不可

        view.RemoveCell(x, y);
        logic.gridValues[x, y] = 0;

        destroyGauge = Mathf.Max(0, destroyGauge - gaugeStep); // 破壊時にゲージ減
        UpdateGaugeUI();
    }

    private void UpdateTicksPosition()
    {
        if (fillArea == null) return;

        float width = fillArea.rect.width;

        if (tick33 != null)
            tick33.anchoredPosition = new Vector2(width * 0.333f, tick33.anchoredPosition.y);

        if (tick66 != null)
            tick66.anchoredPosition = new Vector2(width * 0.666f, tick66.anchoredPosition.y);
    }

    private void AddGaugeByLevel(int value)
    {
        int lv = (int)Mathf.Log(value, 2);
        float increase = (lv * lv) / 4f; // Lvの二乗 / 4 で増える
        destroyGauge = Mathf.Min(destroyGauge + increase, 100f);

        UpdateGaugeUI();
    }
}
