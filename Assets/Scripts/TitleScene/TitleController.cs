using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    [SerializeField] private GameObject startButton;
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip clickSE;

    private void Start()
    {
        AudioManager.instance.PlayBGM(titleBGM);
    }

    public void OnClickStartButton()
    {
        if (startButton != null)
        {
            AudioManager.instance.PlaySE(clickSE);
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError("スタートボタンがアタッチされていません");
        }
    }
}
