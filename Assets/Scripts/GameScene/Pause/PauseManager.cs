using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] BoardManager boardManager;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider seSlider;

    [Header("ページ")]
    [SerializeField] GameObject pausePage;
    [SerializeField] GameObject settingPage;

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

    public void OnBGMChanged(float v)
    {
        AudioManager.instance.SetBGMVolume(v);
        PlayerPrefs.SetFloat("BGM", v);
        PlayerPrefs.Save();
    }

    public void OnSEChanged(float v)
    {
        AudioManager.instance.SetSEVolume(v);
        PlayerPrefs.SetFloat("SE", v);
        PlayerPrefs.Save();
    }

    public void GoSettingPage()
    {
        settingPage.SetActive(true);
        pausePage.SetActive(false);
    }

    public void BackPausePage()
    {
        pausePage.SetActive(true);
        settingPage.SetActive(false);
    }
}
