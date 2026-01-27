using TMPro;
using UnityEngine;

public class ResultDataManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        scoreText.text = GameData.LastScore.ToString("N0");
    }
}
