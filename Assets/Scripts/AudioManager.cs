using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource seSource;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // ï€ë∂ílì«Ç›çûÇ›
        bgmSource.volume = PlayerPrefs.GetFloat("BGM", 0.5f);
        seSource.volume = PlayerPrefs.GetFloat("SE", 3);
    }

    // ===== çƒê∂ =====
    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySE(AudioClip clip)
    {
        seSource.PlayOneShot(clip);
    }

    // ===== âπó  =====
    public void SetBGMVolume(float v)
    {
        bgmSource.volume = v;
    }

    public void SetSEVolume(float v)
    {
        seSource.volume = v;
    }
}
