using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("오디오 소스")]
    public AudioSource sfxSource;   // 효과음용
    public AudioSource bgmSource;   // 배경음용 (루프)

    [Header("클립")]
    public AudioClip explosionClip; // 폭발/피격
    public AudioClip boostClip;     // 부스터
    public AudioClip warningClip;   // 락온 경고
    public AudioClip bgmClip;       // 배경음악

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // PlayOneShot은 timeScale 0(게임오버)에서도 정상 재생됨
    public void PlayExplosion() { PlaySfx(explosionClip); }
    public void PlayBoost()     { PlaySfx(boostClip); }
    public void PlayWarning()   { PlaySfx(warningClip); }

    void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }
}
