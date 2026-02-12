using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource seSource;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySE(AudioClip clip)
    {
        seSource.PlayOneShot(clip);
    }

    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.Play();
    }
}
