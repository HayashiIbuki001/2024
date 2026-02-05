using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

/// <summary>
/// スコアの管理とポップアップ表示を行うクラス
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject scorePopUpPrefab;
    [SerializeField] private RectTransform scorePopUpRoot;

    private int totalScore;
    private Queue<string> queue = new();
    private bool isScorePopupPlaying = false;

    private float destroyGauge = 0f;
    private const float gaugeStep = 33.3f;
    public bool CanDestroy(float cost) => destroyGauge >= cost;

    // ===== 初期化 =====
    private void Start()
    {
        totalScore = 0;
        scoreText.text = "0";
    }

    /// <summary>
    /// スコアを加算し、ポップアップ表示
    /// </summary>
    /// <param name="value">加算するスコア</param>
    /// <param name="msg">ポップアップ表示するメッセージ</param>
    public void Add(int value, string msg = null)
    {
        if (value <= 0) return;

        const int maxScore = 1_000_000_000;
        totalScore = Mathf.Min(totalScore + value, maxScore);

        scoreText.text = totalScore.ToString("N0");

        if (string.IsNullOrEmpty(msg))
            msg = $"+{value:N0}";

        queue.Enqueue(msg);

        if (!isScorePopupPlaying)
            PlayNextPopup();
    }

    /// <summary>
    /// キューにあるスコアポップアップを順に再生
    /// </summary>
    private void PlayNextPopup()
    {
        if (queue.Count == 0)
        {
            isScorePopupPlaying = false;
            return;
        }

        isScorePopupPlaying = true;

        string message = queue.Dequeue();
        ShowScorePopup(message, PlayNextPopup);
    }

    /// <summary>
    /// スコアポップアップの生成とアニメーション再生
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    /// <param name="onComplete">アニメーション終了時のコールバック</param>
    private void ShowScorePopup(string message, System.Action onComplete)
    {
        var obj = Instantiate(scorePopUpPrefab, scorePopUpRoot);
        var text = obj.GetComponent<TextMeshProUGUI>();
        var rt = obj.GetComponent<RectTransform>();

        text.text = message;
        text.alpha = 1f;

        var scoreRT = scoreText.rectTransform;

        // スコアの少し下に表示
        rt.anchoredPosition = scoreRT.anchoredPosition + new Vector2(0, -scoreRT.rect.height);

        // 右へスライド＋フェードアウト
        rt.DOAnchorPosX(rt.anchoredPosition.x + 80f, 0.6f);
        text.DOFade(0f, 0.6f).OnComplete(() =>
        {
            Destroy(obj);
            onComplete?.Invoke();
        });
    }
}
