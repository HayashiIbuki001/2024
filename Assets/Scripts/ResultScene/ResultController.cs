using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultController : MonoBehaviour
{
    [SerializeField] private GameObject TitleButton;
    [SerializeField] private GameObject RetryButton;

    [SerializeField] private AudioClip resultBGM;
    [SerializeField] private AudioClip clickSE;


    private void Start()
    {
        AudioManager.instance.PlayBGM(resultBGM);
    }

    public void OnClickTitleButton()
    {
        if (TitleButton != null)
        {
            AudioManager.instance.PlaySE(clickSE);
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
            AudioManager.instance.PlaySE(clickSE);
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError("リトライボタンがアタッチされていません");
        }
    }
}
