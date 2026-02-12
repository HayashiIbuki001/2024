using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] BoardManager boardManager;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider seSlider;

    [Header("ページ")]
    [SerializeField] GameObject pausePage;
    [SerializeField] GameObject settingPage;

    [Header("効果音")]
    [SerializeField] AudioClip pushSE;

    private void OnEnable()
    {
        // イベント発火させずに値セット
        bgmSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("BGM", 1f));
        seSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SE", 1f));
    }

    public void OnResume()
    {
        boardManager.TogglePause();
    }

    public void OnTitleButton()
    {
        AudioManager.instance.PlaySE(pushSE);
        SceneManager.LoadScene("TitleScene");
    }

    public void ResetToPausePage()
    {
        pausePage.SetActive(true);
        settingPage.SetActive(false);
    }

    public void OnBGMChanged(float v)
    {
        AudioManager.instance.SetBGMVolume(v);
        PlayerPrefs.SetFloat("BGM", v);
    }

    public void OnSEChanged(float v)
    {
        AudioManager.instance.SetSEVolume(v);
        PlayerPrefs.SetFloat("SE", v);
    }

    public void OnSEPreview()
    {
        AudioManager.instance.PlaySE(pushSE);
    }

    public void GoSettingPage()
    {
        AudioManager.instance.PlaySE(pushSE);
        settingPage.SetActive(true);
        pausePage.SetActive(false);
    }

    public void BackPausePage()
    {
        AudioManager.instance.PlaySE(pushSE);
        pausePage.SetActive(true);
        settingPage.SetActive(false);
    }
}
