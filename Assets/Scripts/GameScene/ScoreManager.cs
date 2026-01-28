using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] GameObject scorePopUpPrefab;
    [SerializeField] RectTransform scorePopUpRoot;

    int totalScore = 0;
    Queue<string> scoreQueue = new Queue<string>();
    bool isPopupPlaying;

    public void Init()
    {
        totalScore = 0;
        scoreText.text = totalScore.ToString("N0");
    }

    public void AddScore(int value, string popupMessage = null)
    {
        if (value <= 0) return;

        totalScore = Mathf.Min(totalScore + value, 1_000_000_000);
        scoreText.text = totalScore.ToString("N0");

        if (string.IsNullOrEmpty(popupMessage))
            popupMessage = $"+{value:N0}";

        scoreQueue.Enqueue(popupMessage);

        if (!isPopupPlaying)
            PlayNext();
    }

    void PlayNext()
    {
        if (scoreQueue.Count == 0)
        {
            isPopupPlaying = false;
            return;
        }

        isPopupPlaying = true;
        ShowPopup(scoreQueue.Dequeue(), PlayNext);
    }

    void ShowPopup(string message, System.Action onComplete)
    {
        var obj = Instantiate(scorePopUpPrefab, scorePopUpRoot);
        var text = obj.GetComponent<TextMeshProUGUI>();
        var rt = obj.GetComponent<RectTransform>();

        text.text = message;
        text.alpha = 1f;

        var scoreRT = scoreText.rectTransform;
        rt.anchoredPosition =
            scoreRT.anchoredPosition +
            new Vector2(0, -scoreRT.rect.height);

        rt.DOAnchorPosX(rt.anchoredPosition.x + 80f, 0.6f);
        text.DOFade(0f, 0.6f).OnComplete(() =>
        {
            Destroy(obj);
            onComplete?.Invoke();
        });
    }

}
