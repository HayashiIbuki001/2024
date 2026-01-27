using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    [SerializeField] private GameObject startButton;

    public void OnClickStartButton()
    {
        if (startButton != null)
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError("スタートボタンがアタッチされていません");
        }
    }
}
