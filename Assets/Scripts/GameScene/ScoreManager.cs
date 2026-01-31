using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject scorePopUpPrefab;
    [SerializeField] private RectTransform scorePopUpRoot;

    private int totalScore;
    private Queue<string> queue = new();
    private bool isScorePopupPlaying = false;

    private void Start()
    {
        totalScore = 0;
        scoreText.text = "0";
    }

    public void Add(int value, string msg = null)
    {
        if (value <= 0) return;

        const int maxScore = 1_000_000_000; // 上限値
        totalScore = Mathf.Min(totalScore + value, maxScore);

        scoreText.text = totalScore.ToString("N0");

        if (string.IsNullOrEmpty(msg)) msg = $"+{value:N0}";

        queue.Enqueue(msg);
        if (!isScorePopupPlaying)
            PlayNextPupup();
    }

    private void PlayNextPupup()
    {
        if (queue.Count == 0)
        {
            isScorePopupPlaying = false;
            return;
        }

        isScorePopupPlaying = true;

        string message = queue.Dequeue();
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
}
