using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultController : MonoBehaviour
{
    [SerializeField] private GameObject TitleButton;
    [SerializeField] private GameObject RetryButton;

    public void OnClickTitleButton()
    {
        if (TitleButton != null)
        {
            SceneManager.LoadScene("TitleScene");
        }
        else
        {
            Debug.LogError("タイトルボタンがアタッチされていません");
        }
    }

    public void OnClickRetryButton()
    {
        if (RetryButton != null)
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError("リトライボタンがアタッチされていません");
        }
    }
}
